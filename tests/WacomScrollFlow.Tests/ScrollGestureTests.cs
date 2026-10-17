using System.Numerics;
using Xunit;

namespace WacomScrollFlow.Tests;

public class ScrollGestureTests
{
    private readonly ScrollGesture gesture = new();
    private (int Vertical, int Horizontal) Move(float x, float y, bool touching = true,
        bool reverseVertical = false, bool reverseHorizontal = false) =>
        gesture.Update(new Vector2(x, y), touching, 0.5f, 24, reverseVertical, reverseHorizontal);

    [Fact]
    public void HoverNeverScrolls()
    {
        Move(0, 0, false);
        Assert.Equal((0, 0), Move(100, 100, false));
    }

    [Theory]
    [InlineData(-1, -24)]
    [InlineData(1, 24)]
    public void VerticalStrokeScrollsOppositeToPen(float direction, int expected)
    {
        Move(0, 0);
        Move(0, direction);
        Assert.Equal((expected, 0), Move(0, direction * 2));
    }

    [Fact]
    public void VerticalWinsWhenBothThresholdsAreReached()
    {
        Move(0, 0);
        Move(10, 1);
        Assert.Equal((24, 0), Move(20, 2));
    }

    [Fact]
    public void HorizontalRequiresThresholdAndLocksUntilTipLift()
    {
        Move(0, 0);
        Assert.Equal((0, 0), Move(2, 0));
        Assert.Equal((0, 0), Move(3, 0));
        gesture.ObserveOutput(Vector2.Zero, false);
        gesture.ObserveOutput(new Vector2(101, 0), false);
        Assert.Equal((0, -24), Move(4, 5));
        Move(50, 50, false);
        Assert.Equal((0, 0), Move(100, 100));
        Move(100, 101);
        Assert.Equal((24, 0), Move(100, 102));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HorizontalSwitchRequiresStrictPixelThresholdAndDominance(bool relative)
    {
        Move(0, 0);
        Move(0, 1);
        gesture.ObserveOutput(Vector2.Zero, relative);
        gesture.ObserveOutput(new Vector2(100, 0), relative);
        Assert.Equal((24, 0), Move(0, 2));
        gesture.ObserveOutput(relative ? new Vector2(20, 40) : new Vector2(120, 40), relative);
        Assert.Equal((24, 0), Move(0, 3));
        gesture.ObserveOutput(relative ? new Vector2(1, 0) : new Vector2(121, 40), relative);
        Assert.Equal((0, -24), Move(1, 3));
    }

    [Fact]
    public void DirectionsCanBeReversedIndependently()
    {
        Move(0, 0);
        Move(0, 1);
        Assert.Equal((-24, 0), Move(0, 2, reverseVertical: true));
        gesture.Reset();
        Move(0, 0);
        Move(3, 0); gesture.ObserveOutput(Vector2.Zero, false); gesture.ObserveOutput(new Vector2(101, 0), false);
        Assert.Equal((0, 24), Move(4, 0, reverseHorizontal: true));
    }

    [Fact]
    public void FractionalMotionAccumulatesRatherThanBeingDiscarded()
    {
        Move(0, 0);
        Move(0, 1);
        int total = 0;
        for (int i = 1; i <= 16; i++) total += Move(0, 1 + i / 64f).Vertical;
        Assert.Equal(6, total);
    }

    [Fact]
    public void ReleaseResetsAxisAndFractionalRemainder()
    {
        Move(0, 0);
        Move(0, 1);
        Move(0, 1.02f);
        gesture.Reset();
        Move(0, 0);
        Move(3, 0); gesture.ObserveOutput(Vector2.Zero, false); gesture.ObserveOutput(new Vector2(101, 0), false);
        Assert.Equal((0, 0), Move(3.02f, 0));
    }
}
