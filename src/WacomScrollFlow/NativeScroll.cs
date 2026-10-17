using System.Runtime.InteropServices;
using OpenTabletDriver.Plugin;

namespace WacomScrollFlow;

internal static class NativeScroll
{


    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X, Y;
        public uint Data, Flags, Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion { [FieldOffset(0)] public MouseInput Mouse; }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input { public uint Type; public InputUnion Union; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, Input[] inputs, int size);

    private static bool warned;
    public static void Scroll(int amount, bool horizontal)
    {
        if (!OperatingSystem.IsWindows()) return;
        var input = new Input { Union = new InputUnion { Mouse = new MouseInput
        {
            Data = unchecked((uint)amount), Flags = horizontal ? 0x1000u : 0x0800u
        } } };
        if (SendInput(1, new[] { input }, Marshal.SizeOf<Input>()) != 1 && !warned)
        {
            warned = true;
            Log.Write("Scroll Flow", "Windows rejected wheel input. Check whether the target application is elevated.", LogLevel.Warning);
        }
    }


}
