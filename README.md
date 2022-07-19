# PER.Tracy

Unofficial C# bindings for [Tracy](https://github.com/wolfpld/tracy)

This project **is not** related to [Tracy Profiler](https://github.com/wolfpld/tracy), this is a third-party project

The version of the package might not match the actual version of Tracy being used

its called per.tracy instead of like tracy.net or smth cuz
i initially made it for my [engine](https://github.com/ppr-game/PPR)

syntax almost the same as in c++ achieved using [Metalama](https://www.postsharp.net/metalama)

## Usage

enable by defining `TracyEnable`:

```msbuild
<Project Sdk="Microsoft.NET.Sdk">
  <!-- ... -->
  <PropertyGroup>
    <DefineConstants>TracyEnable</DefineConstants>
  </PropertyGroup>
  <!-- ... -->
</Project>
```

call `Profiler.<TracyMacroName>` to use the profiler (without N/C/NC suffix)

**only string literals are supported for string arguments!!**

### Automatic zones

Enable automatic zones to add a zone to every method in every type in your project
and projects that depend on your project

To enable automatic zones, define `TracyAutoZones`:

```msbuild
<Project Sdk="Microsoft.NET.Sdk">
  <!-- ... -->
  <PropertyGroup>
    <DefineConstants>TracyEnable;TracyAutoZones</DefineConstants>
  </PropertyGroup>
  <!-- ... -->
</Project>
```

## Building

1. clone the repo `git clone https://github.com/cgytrus/PER.Tracy`
2. cd into native project directory `cd ./PER.Tracy/PER.Tracy.Native`
3. create build directory `mkdir ./build/win-x64`
4. cd into build directory `cd ./build/win-x64`
5. configure cmake `cmake ../../`
6. build native project `cmake --build . --target ALL_BUILD --config Release`
7. build solution `cd ../../ && dotnet build -c Release`
