# WacomScrollFlow

English | [简体中文](README-zh.md)

Scroll with your pen while keeping the cursor in place. Hold a pen or tablet button, touch the tablet, and move the pen to scroll vertically or horizontally. Releasing the button while the tip is down pauses scrolling without triggering an accidental click.

For OpenTabletDriver 0.6.7 or later on Windows, Linux, and macOS. Use standard Absolute Mode or Relative Mode. Desktop scrolling on Linux and macOS still needs real-device verification.

## Installation

1. Download `WacomScrollFlow-<version>.zip` from [Releases](https://github.com/HollisMeynell/OTD_WacomScrollFlow/releases/latest).
2. Open the plugin directory from OpenTabletDriver. Create a `WacomScrollFlow` folder and extract the ZIP into it, with `WacomScrollFlow.dll` directly inside the folder.
3. Completely restart OpenTabletDriver.
4. Enable both filters for the same tablet:
   - **Scroll Flow: Input** — place before other filters.
   - **Scroll Flow: Cursor Lock** — place after other filters.
5. In **Scroll Flow: Cursor Lock**, set **Relative output mode** to `true` for OTD Relative Mode, or `false` for Absolute Mode.
6. In **Scroll Flow: Input**, choose your **Button source** and **Button number**.

## Usage

- Hold the configured button to keep the pen cursor in place. Hovering does not scroll.
- Touch the tablet and move up or down to scroll. By default, an upward stroke scrolls down and a downward stroke scrolls up.
- Move horizontally by more than **100 output pixels**, with horizontal displacement more than **three times** vertical displacement, to switch to horizontal scrolling. A leftward stroke scrolls right and a rightward stroke scrolls left. Lift the tip to reset the direction.
- Release the button to pause. If the tip is still touching, the cursor stays locked and tip clicks remain disabled until you lift it. Press the button again before lifting to resume scrolling.

The chosen Pen/Aux button's normal OTD action is suppressed while the plugin is active; you do not need to assign it a separate action. Other pointing devices can still move the cursor.

## Settings

Configure these options in **Scroll Flow: Input**:

| Setting | Default | Description |
| --- | --- | --- |
| Button source | Pen | `Pen` for a pen side button; `Aux` for a tablet button |
| Button number | 1 | Button number, starting from 1 |
| Tip pressure (%) | 1 | Minimum pressure to start recognizing scrolling movement |
| Vertical threshold (mm) | 0.5 | Distance to move before vertical scrolling starts |
| Wheel units per mm | 24 | Scrolling speed; increase for faster scrolling |
| Reverse vertical | false | Reverse the vertical scrolling direction |
| Reverse horizontal | false | Reverse the horizontal scrolling direction |

The horizontal switch threshold and ratio are fixed. **Relative output mode** is configured separately in **Scroll Flow: Cursor Lock** and must match your OTD output mode.

Windows and Linux use 120 wheel units per notch; macOS uses pixel scrolling. Adjust **Wheel units per mm** to suit your platform and preference.

## Troubleshooting

- **No scrolling:** enable both filters, check the button source and number, and make sure **Relative output mode** matches your OTD output mode. The pen must touch the tablet with enough pressure.
- **Cursor remains fixed after releasing the button:** lift the tip. This prevents an unintended click at the end of a scrolling gesture.
- **Scroll direction feels reversed:** enable the corresponding **Reverse** setting.
- **Filter conflicts:** disable other filters first, then re-enable them one at a time. Windows Ink, custom output modes, and asynchronous interpolation filters have not been verified.
- **Unsupported output:** if the selected pointer cannot scroll, the plugin leaves normal pen input active and records a warning in the OTD log.

## License

[MIT No Attribution (MIT-0)](LICENSE). Free to use, modify, and redistribute, including commercially, without an attribution requirement.
