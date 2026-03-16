# Handoff

## Repository

- GitHub: [sms1875/webview2-custom-shell-demo](https://github.com/sms1875/webview2-custom-shell-demo)
- Main solution: [WebView2CustomShellDemo.sln](C:\Users\user\Documents\Playground\WebView2CustomShellDemo.sln)

## Goal

This repository explores a `.NET Framework 4.8 + WinForms + WebView2` shell window with:

- custom title bar
- rounded corners
- shadow effect
- no DWM dependency
- old Windows compatibility considerations

## Current implementation

Core files:

- [ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs)
- [NativeMethods.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\NativeMethods.cs)

Current shell structure:

- `FormBorderStyle.None`
- title bar drag and resize via `WM_NCHITTEST` and `WM_NCLBUTTONDOWN`
- outer rounded window via `CreateRoundRectRgn + SetWindowRgn`
- inner rounded content via `_chromeHost.Region`
- shadow via `CS_DROPSHADOW`

Overlay border experiments were tried and then removed. The project is currently back on the simpler region-based structure.

## Current visual debug state

`ShellForm.cs` currently contains an 8-color debug border guide so border segments can be compared visually:

1. top-left corner
2. top edge
3. top-right corner
4. right edge
5. bottom-right corner
6. bottom edge
7. bottom-left corner
8. left edge

This is temporary diagnostic UI, not the intended final appearance.

## Findings so far

- Top corners generally read better than bottom corners.
- Bottom corners still look less natural than the top, especially on bright backgrounds.
- This appears to be a real rendering limitation of the current `Region`-based outer shape, not just a subjective impression.
- The problem is more visible on bottom/right edges because of pixel stair-stepping and lower contrast.

## What was tested

- standard borderless form with custom title bar
- visible border drawn as a line
- visible border rendered as a filled ring area
- temporary overlay border form for smoother appearance
- reverted back to the simpler previous structure
- 8-color segment debug comparison added

## Documents

- Overview: [README.md](C:\Users\user\Documents\Playground\README.md)
- Case comparison: [implementation-cases.md](C:\Users\user\Documents\Playground\docs\implementation-cases.md)
- Feature and technology guide: [feature-and-technology-guide.md](C:\Users\user\Documents\Playground\docs\feature-and-technology-guide.md)

## Build and run

```powershell
cd C:\Users\user\Documents\Playground
dotnet restore .\WebView2CustomShellDemo.sln
dotnet build .\WebView2CustomShellDemo.sln -c Debug
```

Executable:

- [CustomShellWebView2Demo.exe](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\bin\Debug\net48\CustomShellWebView2Demo.exe)

## Recommended next steps

Option A:

- keep the current structure
- remove the 8-color debug guide
- apply only visual compensation to lower corners and bottom/right edges

Option B:

- move to a higher-quality outer rendering strategy
- likely separate overlay/shadow composition
- more complex, but better if true per-pixel edge quality is required

## Notes for the next machine

- `artifacts/` is ignored and should not be relied on as source of truth.
- The repo already contains the important design documents.
- If continuing visual work, inspect `ShellForm.cs` first.
- If continuing architecture work, compare the current code against the docs before changing the shell strategy again.
