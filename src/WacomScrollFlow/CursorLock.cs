using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace WacomScrollFlow;

[PluginName("Scroll Flow: Cursor Lock")]
[SupportedPlatform(PluginPlatform.Windows | PluginPlatform.Linux | PluginPlatform.MacOS)]
public sealed class CursorLock : IPositionedPipelineElement<IDeviceReport>, IDisposable
{
    private Session? registered;
    [TabletReference] public TabletReference Tablet { get; set; } = null!;
    [Resolved] public IAbsolutePointer? AbsolutePointer { get; set; }
    [Resolved] public IRelativePointer? RelativePointer { get; set; }
    [BooleanProperty("Relative output mode", "Enable when OTD is configured to use Relative Mode."), DefaultPropertyValue(false)]
    public bool RelativeOutput { get; set; }
    public PipelinePosition Position => PipelinePosition.PostTransform;
    public event Action<IDeviceReport>? Emit;

    [OnDependencyLoad]
    public void Initialize()
    {
        if (registered is not null) return;
        var pointer = (RelativeOutput ? (object?)RelativePointer : AbsolutePointer) as IMouseScrollHandler;
        if (pointer is null)
        {
            Log.Write("Scroll Flow", "The selected pointer does not provide scrolling. Scroll Flow is inactive. Check the output mode and Relative output mode setting.", LogLevel.Warning);
            return;
        }
        registered = Session.For(Tablet);
        lock (registered.Gate)
        {
            registered.ScrollPointer = pointer;
            registered.Locks++;
        }
    }

    public void Consume(IDeviceReport report)
    {
        var session = Session.For(Tablet);
        lock (session.Gate)
        {
            if (report is IAbsolutePositionReport position)
            {
                if (session.Frozen && session.Touching)
                    session.Gesture.ObserveOutput(position.Position, RelativeOutput, !session.Held);
                if (session.Frozen)
                {
                    if (RelativeOutput)
                        position.Position = Vector2.Zero;
                    else
                    {
                        // No previous OTD position means no safe anchor. Avoid
                        // emitting a movement until normal positioning resumes.
                        if (session.Cursor is null) return;
                        position.Position = session.Cursor.Value;
                    }
                }
                else if (!RelativeOutput)
                    session.Cursor = position.Position;
            }
        }
        Emit?.Invoke(report);
    }

    public void Dispose()
    {
        if (registered is null) return;
        lock (registered.Gate)
        {
            registered.Locks--;
            if (registered.Locks == 0) registered.ScrollPointer = null;
            registered.Held = false;
            registered.SuppressUntilLift = registered.Touching = false;
            registered.Gesture.Reset();
        }
        registered = null;
    }
}
