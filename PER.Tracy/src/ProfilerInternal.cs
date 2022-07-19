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
        _instance.TracyCreateLocation(CreateString(name), CreateString(method), CreateString(file), line, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint CreateString(string? str) {
        unsafe { return (nuint)Marshal.StringToHGlobalAnsi(str).ToPointer(); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nuint StartScopedZone(nuint location) => _instance.TracyCreateZone(location);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndScopedZone(nuint zone) => _instance.TracyDeleteZone(zone);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneText(nuint text, nuint zone) => _instance.TracyZoneText(zone, text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneName(nuint name, nuint zone) => _instance.TracyZoneName(zone, name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneColor(uint color, nuint zone) => _instance.TracyZoneColor(zone, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ZoneValue(ulong value, nuint zone) => _instance.TracyZoneValue(zone, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMark() => _instance.TracyFrameMark();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkNamed(nuint name) => _instance.TracyFrameMarkNamed(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkStart(nuint name) => _instance.TracyFrameMarkStart(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FrameMarkEnd(nuint name) => _instance.TracyFrameMarkEnd(name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlot(nuint name, long value) => _instance.TracyPlotData(name, value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyPlotConfig(nuint name, Profiler.PlotFormatType type) => _instance.TracyConfigurePlot(name, type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAppInfo(nuint text) => _instance.TracyMessageAppInfo(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyMessage(nuint text) => _instance.TracyTracyMessage(text);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyMessage(nuint text, uint color) => _instance.TracyTracyMessageColor(text, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyAlloc(nuint ptr, nuint size) => _instance.TracyMemAlloc(ptr, size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TracyFree(nuint ptr) => _instance.TracyMemFree(ptr);

#region interop

    // setup for multiple architectures/oses
    // TODO: actually implement it

    private interface IProfiler {
        nuint TracyCreateLocation(nuint name, nuint method, nuint file, uint line, uint color);
        nuint TracyCreateZone(nuint location);
        void TracyDeleteZone(nuint ptr);
        void TracyZoneText(nuint zone, nuint text);
        void TracyZoneName(nuint zone, nuint name);
        void TracyZoneColor(nuint zone, uint color);
        void TracyZoneValue(nuint zone, ulong value);
        void TracyFrameMark();
        void TracyFrameMarkNamed(nuint name);
        void TracyFrameMarkStart(nuint name);
        void TracyFrameMarkEnd(nuint name);
        void TracyPlotData(nuint name, long value);
        void TracyConfigurePlot(nuint name, Profiler.PlotFormatType type);
        void TracyMessageAppInfo(nuint text);
        void TracyTracyMessage(nuint text);
        void TracyTracyMessageColor(nuint text, uint color);
        void TracyMemAlloc(nuint ptr, nuint size);
        void TracyMemFree(nuint ptr);
    }

    private static IProfiler _instance = new ProfilerX64();

    private const string MainLibName = "PER.Tracy.Native";
    private const CallingConvention CallConv = CallingConvention.Cdecl;

    private class ProfilerX64 : IProfiler {
        private const string LibName = MainLibName;

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyCreateLocation))]
        private static extern nuint TracyCreateLocationInternal(nuint name, nuint method, nuint file, uint line, uint color);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyCreateZone))]
        private static extern nuint TracyCreateZoneInternal(nuint location);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyDeleteZone))]
        private static extern void TracyDeleteZoneInternal(nuint ptr);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyZoneText))]
        private static extern void TracyZoneTextInternal(nuint zone, nuint text);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyZoneName))]
        private static extern void TracyZoneNameInternal(nuint zone, nuint name);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyZoneColor))]
        private static extern void TracyZoneColorInternal(nuint zone, uint color);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyZoneValue))]
        private static extern void TracyZoneValueInternal(nuint zone, ulong value);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyFrameMark))]
        private static extern void TracyFrameMarkInternal();

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyFrameMarkNamed))]
        private static extern void TracyFrameMarkNamedInternal(nuint name);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyFrameMarkStart))]
        private static extern void TracyFrameMarkStartInternal(nuint name);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyFrameMarkEnd))]
        private static extern void TracyFrameMarkEndInternal(nuint name);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyPlotData))]
        private static extern void TracyPlotDataInternal(nuint name, long value);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyConfigurePlot))]
        private static extern void TracyConfigurePlotInternal(nuint name, Profiler.PlotFormatType type);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyMessageAppInfo))]
        private static extern void TracyMessageAppInfoInternal(nuint text);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyTracyMessage))]
        private static extern void TracyTracyMessageInternal(nuint text);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyTracyMessageColor))]
        private static extern void TracyTracyMessageColorInternal(nuint text, uint color);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyMemAlloc))]
        private static extern void TracyMemAllocInternal(nuint ptr, nuint size);

        [DllImport(LibName, CallingConvention = CallConv, ExactSpelling = false, EntryPoint = nameof(TracyMemFree))]
        private static extern void TracyMemFreeInternal(nuint ptr);

        public nuint TracyCreateLocation(nuint name, nuint method, nuint file, uint line, uint color) =>
            TracyCreateLocationInternal(name, method, file, line, color);
        public nuint TracyCreateZone(nuint location) => TracyCreateZoneInternal(location);
        public void TracyDeleteZone(nuint ptr) => TracyDeleteZoneInternal(ptr);
        public void TracyZoneText(nuint zone, nuint text) => TracyZoneTextInternal(zone, text);
        public void TracyZoneName(nuint zone, nuint name) => TracyZoneNameInternal(zone, name);
        public void TracyZoneColor(nuint zone, uint color) => TracyZoneColorInternal(zone, color);
        public void TracyZoneValue(nuint zone, ulong value) => TracyZoneValueInternal(zone, value);
        public void TracyFrameMark() => TracyFrameMarkInternal();
        public void TracyFrameMarkNamed(nuint name) => TracyFrameMarkNamedInternal(name);
        public void TracyFrameMarkStart(nuint name) => TracyFrameMarkStartInternal(name);
        public void TracyFrameMarkEnd(nuint name) => TracyFrameMarkEndInternal(name);
        public void TracyPlotData(nuint name, long value) => TracyPlotDataInternal(name, value);
        public void TracyConfigurePlot(nuint name, Profiler.PlotFormatType type) => TracyConfigurePlotInternal(name, type);
        public void TracyMessageAppInfo(nuint text) => TracyMessageAppInfoInternal(text);
        public void TracyTracyMessage(nuint text) => TracyTracyMessageInternal(text);
        public void TracyTracyMessageColor(nuint text, uint color) => TracyTracyMessageColorInternal(text, color);
        public void TracyMemAlloc(nuint ptr, nuint size) => TracyMemAllocInternal(ptr, size);
        public void TracyMemFree(nuint ptr) => TracyMemFreeInternal(ptr);
    }

#endregion
}
