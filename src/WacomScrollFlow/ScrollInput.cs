using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace WacomScrollFlow;

[PluginName("Scroll Flow: Input")]
[SupportedPlatform(PluginPlatform.Windows | PluginPlatform.Linux | PluginPlatform.MacOS)]
public sealed class ScrollInput : IPositionedPipelineElement<IDeviceReport>, IDisposable
{
    [TabletReference] public TabletReference Tablet { get; set; } = null!;
    public PipelinePosition Position => PipelinePosition.PreTransform;
    public event Action<IDeviceReport>? Emit;

    [Property("Button source"), DefaultPropertyValue("Pen"), PropertyValidated(nameof(ButtonSources))]
    public string ButtonSource { get; set; } = "Pen";
    public static IEnumerable<string> ButtonSources => new[] { "Pen", "Aux" };

    [Property("Button number"), DefaultPropertyValue(1),
     ToolTip("Pen/Aux: 1-based button number.")]
    public int Button { get; set; } = 1;

    [Property("Tip pressure (%)"), DefaultPropertyValue(1f)]
    public float TipPressure { get; set; } = 1;

    [Property("Vertical threshold (mm)"), DefaultPropertyValue(0.5f)]
    public float VerticalThreshold { get; set; } = 0.5f;

    [Property("Wheel units per mm"), DefaultPropertyValue(24f), ToolTip("OTD wheel units per millimeter. 120 units = one wheel notch on Windows/Linux; macOS uses pixel scrolling.")]
    public float UnitsPerMm { get; set; } = 24;

    [BooleanProperty("Reverse vertical", "Invert the default up-stroke to down-scroll direction."), DefaultPropertyValue(false)]
    public bool ReverseVertical { get; set; }

    [BooleanProperty("Reverse horizontal", "Invert the default left-stroke to right-scroll direction."), DefaultPropertyValue(false)]
    public bool ReverseHorizontal { get; set; }

    public void Consume(IDeviceReport report)
    {
        var session = Session.For(Tablet);
        lock (session.Gate)
        {
            bool? down = ButtonSource switch
            {
                "Pen" when report is ITabletReport pen => ReadButton(pen.PenButtons),
                "Aux" when report is IAuxReport aux => ReadButton(aux.AuxButtons),
                "Pen" when report is OutOfRangeReport => false,
                _ => null
            };
            if (down.HasValue)
            {
                bool held = down.Value && session.Locks > 0;
                if (held != session.Held)
                {
                    if (!session.SuppressUntilLift) session.Gesture.Reset();
                    session.Held = held;
                }
            }

            if (report is OutOfRangeReport)
            {
                session.Gesture.Reset();
                session.SuppressUntilLift = session.Touching = false;
            }

            if (report is ITabletReport physical)
            {
                if (physical.Pressure == 0) session.SuppressUntilLift = false;
                else if (session.Held) session.SuppressUntilLift = true;
                session.Touching = physical.Pressure > 0 && physical.Pressure >=
                    Tablet.Properties.Specifications.Pen.MaxPressure * Math.Clamp(Finite(TipPressure, 1), 0, 100) / 100;
                if (!session.Touching) session.Gesture.Reset();
            }

            // Read the physical state first, then hide the scroll button from
            // OTD's binding handler (it may otherwise invoke its left-click binding).
            if (session.Locks > 0)
            {
                if (ButtonSource == "Pen" && report is ITabletReport buttons)
                    buttons.PenButtons = SuppressButton(buttons.PenButtons);
                if (ButtonSource == "Aux" && report is IAuxReport auxiliary)
                    auxiliary.AuxButtons = SuppressButton(auxiliary.AuxButtons);
            }

            if (session.Held && report is ITabletReport tabletReport)
            {
                var digitizer = Tablet.Properties.Specifications.Digitizer;
                bool touching = session.Touching;
                Vector2 mm = tabletReport.Position * new Vector2(
                    digitizer.Width / digitizer.MaxX, digitizer.Height / digitizer.MaxY);
                var scroll = session.Gesture.Update(mm, touching,
                    Math.Max(0.01f, Finite(VerticalThreshold, 0.5f)),
                    Math.Clamp(Finite(UnitsPerMm, 24), 0.01f, 1200), ReverseVertical, ReverseHorizontal);
                if (scroll.Vertical != 0) session.ScrollPointer?.ScrollVertically(scroll.Vertical);
                if (scroll.Horizontal != 0) session.ScrollPointer?.ScrollHorizontally(scroll.Horizontal);
                if ((scroll.Vertical != 0 || scroll.Horizontal != 0) && session.ScrollPointer is ISynchronousPointer synchronous)
                    synchronous.Flush();
                // Suppress the tip binding before OTD's post-transform binding handler.
                tabletReport.Pressure = 0;
            }
            if (!session.Held && session.SuppressUntilLift && report is ITabletReport paused)
            {
                var digitizer = Tablet.Properties.Specifications.Digitizer;
                session.Gesture.Rebase(paused.Position * new Vector2(
                    digitizer.Width / digitizer.MaxX, digitizer.Height / digitizer.MaxY));
            }
            if (session.Frozen && report is ITabletReport suppressed) suppressed.Pressure = 0;
        }
        Emit?.Invoke(report);
    }

    private bool ReadButton(bool[] buttons) => Button > 0 && Button <= buttons.Length && buttons[Button - 1];
    private bool[] SuppressButton(bool[] buttons)
    {
        if (Button <= 0 || Button > buttons.Length) return buttons;
        var masked = (bool[])buttons.Clone();
        masked[Button - 1] = false;
        return masked;
    }
    private static float Finite(float value, float fallback) => float.IsFinite(value) ? value : fallback;

    public void Dispose()
    {
        if (Tablet is null) return;
        var session = Session.For(Tablet);
        lock (session.Gate) { session.Held = session.SuppressUntilLift = session.Touching = false; session.Gesture.Reset(); }
    }
}
