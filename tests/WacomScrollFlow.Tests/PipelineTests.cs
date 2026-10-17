using System.Numerics;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace WacomScrollFlow.Tests;

public class PipelineTests
{
    private static TabletReference Tablet() => new()
    {
        Properties = new TabletConfiguration
        {
            Specifications = new TabletSpecifications
            {
                Digitizer = new DigitizerSpecifications { Width = 100, Height = 100, MaxX = 1000, MaxY = 1000 },
                Pen = new PenSpecifications { MaxPressure = 1000 }
            }
        }
    };

    private static ITabletReport Report(bool down, uint pressure = 0) => new TabletReport
    {
        Raw = Array.Empty<byte>(), Position = new Vector2(10, 20),
        Pressure = pressure, PenButtons = new[] { down }
    };

    [Fact]
    public void InputWithoutCursorLockDoesNotSuppressPressure()
    {
        using var input = new ScrollInput { Tablet = Tablet() };
        var report = Report(true, 100);
        input.Consume(report);
        Assert.Equal(100u, report.Pressure);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HeldButtonFreezesHoverAndReleaseRestoresMovement(bool relative)
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet, RelativeOutput = relative };
        using var input = new ScrollInput { Tablet = tablet };
        cursor.Initialize();
        input.Emit += cursor.Consume;
        int emitted = 0;
        cursor.Emit += _ => emitted++;
        var initial = Report(false);
        initial.Position = new Vector2(1500, 900);
        input.Consume(initial);
        var first = Report(true);
        input.Consume(first);
        var anchor = first.Position;
        Assert.Equal(relative ? Vector2.Zero : initial.Position, anchor);
        var hover = Report(true);
        hover.Position = new Vector2(50, 70);
        input.Consume(hover);
        Assert.Equal(relative ? Vector2.Zero : anchor, hover.Position);
        var released = Report(false, 100);
        released.Position = new Vector2(55, 75);
        input.Consume(released);
        Assert.Equal(new Vector2(55, 75), released.Position);
        Assert.Equal(100u, released.Pressure);
        Assert.Equal(4, emitted);
    }

    [Fact]
    public void HeldButtonSuppressesTipBinding()
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet };
        using var input = new ScrollInput { Tablet = tablet };
        cursor.Initialize();
        var report = Report(true, 100);
        input.Consume(report);
        Assert.Equal(0u, report.Pressure);
        Assert.False(report.PenButtons[0]);
    }

    [Fact]
    public void AuxiliaryButtonStatePersistsAcrossPenReports()
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet };
        using var input = new ScrollInput { Tablet = tablet, ButtonSource = "Aux" };
        cursor.Initialize();
        IAuxReport aux = new AuxReport { Raw = Array.Empty<byte>(), AuxButtons = new[] { true } };
        input.Consume(aux);
        Assert.False(aux.AuxButtons[0]);
        var held = Report(false, 100);
        input.Consume(held);
        Assert.Equal(0u, held.Pressure);
        input.Consume(new AuxReport { Raw = Array.Empty<byte>(), AuxButtons = new[] { false } });
        var released = Report(false, 100);
        input.Consume(released);
        Assert.Equal(0u, released.Pressure);
        input.Consume(Report(false));
        var nextContact = Report(false, 100);
        input.Consume(nextContact);
        Assert.Equal(100u, nextContact.Pressure);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReleaseWhileTouchingFreezesAndSuppressesUntilLiftAcrossButtonBounce(bool relative)
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet, RelativeOutput = relative };
        using var input = new ScrollInput { Tablet = tablet };
        cursor.Initialize();
        input.Emit += cursor.Consume;
        input.Consume(Report(false));
        input.Consume(Report(true, 100));
        foreach (bool down in new[] { false, false, true, false })
        {
            var report = Report(down, 1); // Below the scroll pressure threshold, still touching.
            report.Position = new Vector2(500, 600);
            input.Consume(report);
            Assert.Equal(0u, report.Pressure);
            Assert.Equal(relative ? Vector2.Zero : new Vector2(10, 20), report.Position);
        }
        var lifted = Report(false);
        lifted.Position = new Vector2(70, 80);
        input.Consume(lifted);
        Assert.Equal(new Vector2(70, 80), lifted.Position);
        var normal = Report(false, 100);
        input.Consume(normal);
        Assert.Equal(100u, normal.Pressure);
    }

    [Fact]
    public void CursorLockDisposalRestoresPressure()
    {
        var tablet = Tablet();
        using var input = new ScrollInput { Tablet = tablet };
        var cursor = new CursorLock { Tablet = tablet };
        cursor.Initialize();
        input.Consume(Report(true, 100));
        cursor.Dispose();
        var next = Report(true, 100);
        input.Consume(next);
        Assert.Equal(100u, next.Pressure);
    }

    [Fact]
    public void FirstReportWhileHeldNeverInventsAnAbsoluteCursorPosition()
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet };
        using var input = new ScrollInput { Tablet = tablet };
        cursor.Initialize();
        input.Emit += cursor.Consume;
        int emitted = 0;
        cursor.Emit += _ => emitted++;
        input.Consume(Report(true, 100));
        input.Consume(Report(true, 100));
        Assert.Equal(0, emitted);
        input.Consume(Report(false));
        Assert.Equal(1, emitted);
    }

    [Fact]
    public void MaskingScrollButtonPreservesOtherButtonsAndOriginalArray()
    {
        var tablet = Tablet();
        using var cursor = new CursorLock { Tablet = tablet };
        using var input = new ScrollInput { Tablet = tablet };
        cursor.Initialize();
        var original = new[] { true, true };
        var report = Report(true);
        report.PenButtons = original;
        input.Consume(report);
        Assert.Equal(new[] { false, true }, report.PenButtons);
        Assert.Equal(new[] { true, true }, original);
    }

    [Fact]
    public void TabletSessionsAreIsolated()
    {
        var first = Tablet();
        using var cursor = new CursorLock { Tablet = first };
        using var input = new ScrollInput { Tablet = first };
        using var other = new ScrollInput { Tablet = Tablet() };
        cursor.Initialize();
        input.Consume(Report(true));
        var report = Report(false, 100);
        other.Consume(report);
        Assert.Equal(100u, report.Pressure);
    }
}
