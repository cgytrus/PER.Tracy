// ReSharper disable CppInconsistentNaming
#include <Tracy.hpp>

// TODO bro screw linux im too tired for this
// build error is:
/*
/usr/bin/ld: lib/tracy/libTracyClient.a(TracyClient.cpp.o): relocation R_X86_64_TPOFF32 against symbol
             `_ZN5tracy8s_gpuCtxE' can not be used when making a shared object; recompile with -fPIC
collect2: error: ld returned 1 exit status
make[2]: *** [CMakeFiles/PER.Tracy.Native.dir/build.make:85: libPER.Tracy.Native.so] Error 1
make[1]: *** [CMakeFiles/Makefile2:96: CMakeFiles/PER.Tracy.Native.dir/all] Error 2
make: *** [Makefile:130: all] Error 2
 */
#if defined(_MSC_VER)
#define PER_EXPORT extern "C" __declspec(dllexport)
#define PER_CALL __cdecl
#elif defined(__GNUC__)
#define PER_EXPORT extern "C" __attribute__((visibility("default")))
#define PER_CALL
#else
#define PER_EXPORT
#define PER_CALL
#endif

PER_EXPORT tracy::SourceLocationData* PER_CALL TracyCreateLocation(const char* name, const char* method, const char* file,
    uint32_t line, uint32_t color) {
    const auto location = new tracy::SourceLocationData { name, method, file, line, color };
    return location;
}

PER_EXPORT tracy::ScopedZone* PER_CALL TracyCreateZone(tracy::SourceLocationData* location) {
    const auto zone = new tracy::ScopedZone(location, true);
    return zone;
}

PER_EXPORT void PER_CALL TracyDeleteZone(const tracy::ScopedZone* ptr) {
    delete ptr;
}

PER_EXPORT void PER_CALL TracyZoneText(tracy::ScopedZone& zone, const char* text) {
    ZoneTextV(zone, text, strlen(text));
}

PER_EXPORT void PER_CALL TracyZoneName(tracy::ScopedZone& zone, const char* name) {
    ZoneNameV(zone, name, strlen(name));
}

PER_EXPORT void PER_CALL TracyZoneColor(tracy::ScopedZone& zone, uint32_t color) {
    ZoneColorV(zone, color);
}

PER_EXPORT void PER_CALL TracyZoneValue(tracy::ScopedZone& zone, uint64_t value) {
    ZoneValueV(zone, value);
}

PER_EXPORT void PER_CALL TracyFrameMark() {
    FrameMark;
}

PER_EXPORT void PER_CALL TracyFrameMarkNamed(const char* name) {
    FrameMarkNamed(name);
}

PER_EXPORT void PER_CALL TracyFrameMarkStart(const char* name) {
    FrameMarkStart(name);
}

PER_EXPORT void PER_CALL TracyFrameMarkEnd(const char* name) {
    FrameMarkEnd(name);
}

PER_EXPORT void PER_CALL TracyPlotData(const char* name, int64_t value) {
    TracyPlot(name, value);
}

PER_EXPORT void PER_CALL TracyConfigurePlot(const char* name, tracy::PlotFormatType type) {
    TracyPlotConfig(name, type);
}

PER_EXPORT void PER_CALL TracyMessageAppInfo(const char* text) {
    TracyAppInfo(text, strlen(text));
}

PER_EXPORT void PER_CALL TracyTracyMessage(const char* text) {
    TracyMessage(text, strlen(text));
}

PER_EXPORT void PER_CALL TracyTracyMessageColor(const char* text, uint32_t color) {
    TracyMessageC(text, strlen(text), color);
}

PER_EXPORT void PER_CALL TracyMemAlloc(void* ptr, size_t size) {
    TracyAlloc(ptr, size);
}

PER_EXPORT void PER_CALL TracyMemFree(void* ptr) {
    TracyFree(ptr);
}
