# WacomScrollFlow

English | [简体中文](README-zh.md)

**Source update:** Keyboard button polling has been removed from the current source; only Pen/Aux buttons are supported. The existing v0.1.2 release still includes Keyboard mode and has not been rebuilt.

A Windows scrolling plugin for [OpenTabletDriver](https://opentabletdriver.net/). Hold a configurable button and move your pen with the tip touching the tablet to scroll while keeping the cursor in place.

**Tested successfully with OpenTabletDriver 0.6.7 on real hardware.** Built with .NET 8 against the OpenTabletDriver 0.6.6.2 plugin API. Windows only; other driver versions have not been verified.

## How it works

1. Hold the configured button to lock the pen cursor. Hovering does not scroll.
2. Touch the tablet and move the pen to scroll. By default, an upward stroke scrolls down and a downward stroke scrolls up. A leftward stroke scrolls right and a rightward stroke scrolls left.
3. Vertical scrolling starts after the configured vertical threshold. When horizontal displacement exceeds **100 output pixels** and is **more than three times** vertical displacement, the gesture switches to horizontal scrolling until the tip is lifted. Absolute mode measures coordinate differences; relative mode accumulates output movement.
4. Release the button to pause scrolling. If the tip is still touching the tablet, the cursor stays locked and tip clicks remain suppressed until you lift the pen. Press the button again before lifting to resume scrolling without replaying movement made while paused. This prevents button bounce from turning a scroll gesture into a click.

The cursor lock applies to the tablet output. Other pointing devices can still move the cursor.

## Installation

1. Download `WacomScrollFlow-<version>-win.zip` from [Releases](https://github.com/HollisMeynell/OTD_WacomScrollFlow/releases).
2. Open the plugin directory from OpenTabletDriver, create a `WacomScrollFlow` folder, and extract the ZIP into it. The DLL should be directly inside that folder.
3. Completely restart OpenTabletDriver.
4. Enable **both** filters for the same tablet:
   - **Scroll Flow: Input**: place before other filters. Reads the original position, pressure, and button state.
   - **Scroll Flow: Cursor Lock**: place after other filters. Locks the transformed output. Set **Relative output mode** to `true` when using OTD Relative Mode; leave it `false` for Absolute Mode.

The release ZIP contains the plugin DLL and documentation, without OpenTabletDriver's own assemblies. Publishing a GitHub Release does not automatically list the plugin in Plugins Manager; that requires a separate submission to the official plugin repository.

Standard Absolute Mode and Relative Mode are the intended output modes. Windows Ink, custom output modes, and asynchronous interpolation filters have not been verified. Start with other filters disabled when troubleshooting.

## Configuration

Configure these options in **Scroll Flow: Input**:

| Setting | Default | Meaning |
| --- | --- | --- |
| Button source | Pen | `Pen` side button or `Aux` tablet button |
| Button number | 1 | Pen/Aux buttons are numbered from 1. |
| Tip pressure (%) | 1 | Pressure required to recognize scrolling movement |
| Vertical threshold (mm) | 0.5 | Tablet displacement required to start vertical scrolling |
| Wheel units per mm | 24 | Wheel units per millimeter; 120 Windows wheel units usually equal one notch |
| Reverse vertical | false | Reverse vertical scrolling direction |
| Reverse horizontal | false | Reverse horizontal scrolling direction |

The horizontal switch threshold (100px) and dominance ratio (3) are fixed. Configure **Relative output mode** separately in **Scroll Flow: Cursor Lock**.

Select the scrolling button directly in the plugin; no OTD action binding is needed. The plugin suppresses the selected Pen/Aux button's normal binding while active.

## Build and test

Install the .NET 8 SDK (8.0.403 or a newer .NET 8 feature band), then run from the repository root:

```powershell
dotnet build src/WacomScrollFlow/WacomScrollFlow.csproj -c Release
dotnet test tests/WacomScrollFlow.Tests/WacomScrollFlow.Tests.csproj -c Release
```

Production code is in `src/WacomScrollFlow/`; xUnit tests are in `tests/WacomScrollFlow.Tests/`. Tests cover gesture direction, pixel thresholds, tip lifting, fractional wheel accumulation, cursor locking, button masking, and click suppression during button bounce.

To create a release ZIP and SHA256 checksum locally after building:

```powershell
./scripts/Package.ps1 -Version 0.1.2
```

## Releases

Pushing a version tag triggers GitHub Actions to validate the version, run the tests, build the plugin, package the DLL at the ZIP root, and publish a GitHub Release with the ZIP and its SHA256 checksum. The workflow uses GitHub's built-in token; no additional secret is required.

For a new release, update `<Version>` in `src/WacomScrollFlow/WacomScrollFlow.csproj`, commit and push the change, then push a matching tag:

```powershell
git tag -a v0.1.3 -m "Release v0.1.3"
git push origin v0.1.3
```

## Implementation

The input filter reads tablet millimeters and suppresses pressure before OTD's binding handler. The output filter observes transformed pixel movement before locking the cursor, preserving the last absolute position or zeroing relative displacement. Wheel events use Windows `SendInput`.

See OTD's [output pipeline](https://github.com/OpenTabletDriver/OpenTabletDriver/blob/v0.6.7/OpenTabletDriver.Plugin/Output/OutputMode.cs). If the first report arrives while the button is held and no absolute cursor anchor is available, movement is withheld until normal positioning resumes.

## License

Licensed under [MIT No Attribution (MIT-0)](LICENSE), including the project's v0.1.2 release. You may use, modify, and redistribute this software, including commercially, without an attribution requirement.
