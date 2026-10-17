using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace WacomScrollFlow;

internal sealed class Session
{
    private static readonly ConditionalWeakTable<TabletReference, Session> Sessions = new();
    public static Session For(TabletReference tablet) => Sessions.GetValue(tablet, _ => new());
    public readonly object Gate = new();
    public readonly ScrollGesture Gesture = new();
    public int Locks;
    public IMouseScrollHandler? ScrollPointer;
    public bool Held;
    public bool SuppressUntilLift;
    public bool Touching;
    public bool Frozen => Held || SuppressUntilLift;
    public Vector2? Cursor;
}
