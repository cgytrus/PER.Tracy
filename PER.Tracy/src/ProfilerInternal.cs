using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

namespace PER.Tracy;

// almost all the methods used for codegen
[PublicAPI]
public static class ProfilerInternal {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateLocation(string method, string file, uint line) =>
        CreateLocation(null, 0, method, file, line);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateLocation(string? name, string method, string file, uint line) =>
        CreateLocation(name, 0, method, file, line);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateLocation(uint color, string method, string file, uint line) =>
        CreateLocation(null, color, method, file, line);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateLocation(string? name, uint color, string method, string file, uint line) =>
        TracyCreateLocation(CreateString(name), CreateString(method), CreateString(file), line, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateString(string? str) {
        unsafe { return (nuint)Marshal.StringToHGlobalAnsi(str).ToPointer(); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint StartScopedZone(nuint location) => TracyCreateZone(location);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndScopedZone(nuint zone) => TracyDeleteZone(zone);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneText(nuint zone, nuint text) => TracyZoneText(zone, text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneName(nuint zone, nuint name) => TracyZoneName(zone, name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneColor(nuint zone, uint color) => TracyZoneColor(zone, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneValue(nuint zone, ulong value) => TracyZoneValue(zone, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMark() => TracyFrameMark();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkNamed(nuint name) => TracyFrameMarkNamed(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkStart(nuint name) => TracyFrameMarkStart(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkEnd(nuint name) => TracyFrameMarkEnd(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlot(nuint name, long value) => TracyPlotData(name, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlotConfig(nuint name, Profiler.PlotFormatType type) => TracyConfigurePlot(name, type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAppInfo(nuint text) => TracyMessageAppInfo(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyMessage(nuint text) => TracyTracyMessage(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyMessage(nuint text, uint color) => TracyTracyMessageColor(text, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAlloc(nuint ptr, nuint size) => TracyMemAlloc(ptr, size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyFree(nuint ptr) => TracyMemFree(ptr);

#region interop

    private const string LibName = "PER.Tracy.Native";

    [DllImport(LibName)]
    private static extern nuint TracyCreateLocation(nuint name, nuint method, nuint file, uint line, uint color);

    [DllImport(LibName)]
    private static extern nuint TracyCreateZone(nuint location);

    [DllImport(LibName)]
    private static extern void TracyDeleteZone(nuint ptr);

    [DllImport(LibName)]
    private static extern void TracyZoneText(nuint zone, nuint text);

    [DllImport(LibName)]
    private static extern void TracyZoneName(nuint zone, nuint name);

    [DllImport(LibName)]
    private static extern void TracyZoneColor(nuint zone, uint color);

    [DllImport(LibName)]
    private static extern void TracyZoneValue(nuint zone, ulong value);

    [DllImport(LibName)]
    private static extern void TracyFrameMark();

    [DllImport(LibName)]
    private static extern void TracyFrameMarkNamed(nuint name);

    [DllImport(LibName)]
    private static extern void TracyFrameMarkStart(nuint name);

    [DllImport(LibName)]
    private static extern void TracyFrameMarkEnd(nuint name);

    [DllImport(LibName)]
    private static extern void TracyPlotData(nuint name, long value);

    [DllImport(LibName)]
    private static extern void TracyConfigurePlot(nuint name, Profiler.PlotFormatType type);

    [DllImport(LibName)]
    private static extern void TracyMessageAppInfo(nuint text);

    [DllImport(LibName)]
    private static extern void TracyTracyMessage(nuint text);

    [DllImport(LibName)]
    private static extern void TracyTracyMessageColor(nuint text, uint color);

    [DllImport(LibName)]
    private static extern void TracyMemAlloc(nuint ptr, nuint size);

    [DllImport(LibName)]
    private static extern void TracyMemFree(nuint ptr);

#endregion
}
