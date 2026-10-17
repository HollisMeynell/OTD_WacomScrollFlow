using System.Numerics;

namespace WacomScrollFlow;

// Positions and thresholds are in physical tablet millimeters.
public sealed class ScrollGesture
{
    private Vector2? origin;
    private Vector2 previous;
    private int axis;
    private double remainder;
    private Vector2? outputOrigin;
    private Vector2 outputTravel;
    private Vector2? lastOutput;

    public void Reset()
    {
        origin = null;
        outputOrigin = null;
        outputTravel = default;
        lastOutput = null;
        axis = 0;
        remainder = 0;
    }

    public void Rebase(Vector2 position) => previous = position;

    public void ObserveOutput(Vector2 position, bool relative, bool paused = false)
    {
        if (origin is null || axis == 2) return;
        if (paused)
        {
            if (!relative && outputOrigin.HasValue && lastOutput.HasValue)
                outputOrigin += position - lastOutput.Value;
            lastOutput = position;
            return;
        }
        lastOutput = position;
        if (outputOrigin is null)
        {
            outputOrigin = position;
            return;
        }
        outputTravel = relative ? outputTravel + position : position - outputOrigin.Value;
        if (Math.Abs(outputTravel.X) > 100 && Math.Abs(outputTravel.X) > 3 * Math.Abs(outputTravel.Y))
        {
            axis = 2;
            remainder = 0;
        }
    }

    public (int Vertical, int Horizontal) Update(Vector2 position, bool touching,
        float verticalThreshold,
        float unitsPerMm, bool reverseVertical, bool reverseHorizontal)
    {
        if (!touching)
        {
            Reset();
            return default;
        }
        if (origin is null)
        {
            origin = previous = position;
            return default;
        }

        Vector2 travel = position - origin.Value;
        if (axis == 0)
        {
            if (Math.Abs(travel.Y) >= verticalThreshold)
                axis = 1;
            previous = position;
            return default;
        }

        Vector2 delta = position - previous;
        previous = position;
        // Native wheel: positive vertical = up; positive horizontal = right.
        double amount = axis == 1
            ? delta.Y * (reverseVertical ? -1 : 1)
            : delta.X * (reverseHorizontal ? 1 : -1);
        remainder += amount * unitsPerMm;
        int whole = (int)Math.Clamp(Math.Truncate(remainder), -1200, 1200);
        remainder -= whole;
        return axis == 1 ? (whole, 0) : (0, whole);
    }
}
