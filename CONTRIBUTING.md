# Contributing

## Development

```powershell
cd C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo
dotnet restore
dotnet build
```

## Scope

- Keep the sample focused on `.NET Framework 4.8 + WinForms + WebView2`.
- Prefer Win32 techniques that do not depend on DWM.
- Document compatibility notes when changing rounded corners, shadow, or title bar behavior.

## Pull Requests

- Include a short summary of the change.
- Note any Windows version assumptions.
- Mention whether the sample was tested with restore/build only or with manual UI verification too.
