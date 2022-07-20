using System.Runtime.CompilerServices;

using JetBrains.Annotations;

namespace PER.Tracy;

[PublicAPI]
public static class Profiler {
    [PublicAPI]
    public enum PlotFormatType : byte {
        Number,
        Memory,
        Percentage
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneScoped(string? name = null, uint color = 0) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneScoped(uint color) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneText(string text) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneName(string name) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneColor(uint color) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneValue(ulong value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMark() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkNamed(string name) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkStart(string name) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkEnd(string name) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlot(string name, long value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlotConfig(string name, PlotFormatType type) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAppInfo(string text) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyMessage(string text, uint color = 0) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAlloc(nuint ptr, nuint size) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyFree(nuint ptr) { }
}
