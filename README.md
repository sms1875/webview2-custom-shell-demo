# Custom Shell WebView2 Demo

`.NET Framework 4.8 + WinForms + WebView2` 환경에서 `DWM 없이` 커스텀 타이틀 바, 라운드 코너, 그림자를 적용하는 샘플과 조사 문서를 포함합니다.

## 구성

- `docs/implementation-cases.md`: 요구사항별 구현 케이스, 장단점, 오픈소스 사례, 적용 판단 기준
- `src/CustomShellWebView2Demo`: 바로 열어볼 수 있는 WinForms 샘플 프로젝트

## 샘플 기본 전략

- 타이틀 바: `FormBorderStyle.None` + 직접 그린 상단 패널 + `WM_NCHITTEST`
- 라운드 코너: `CreateRoundRectRgn + SetWindowRgn`
- 그림자: `CS_DROPSHADOW`
- WebView2: `Microsoft.Web.WebView2` NuGet 패키지 사용

2026-03-16 기준으로 샘플 프로젝트에는 `Microsoft.Web.WebView2 1.0.3856.49`를 넣었습니다.

## 실행

```powershell
cd C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo
dotnet restore
dotnet build
```

Visual Studio에서 [`src/CustomShellWebView2Demo/CustomShellWebView2Demo.csproj`](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\CustomShellWebView2Demo.csproj)를 열어도 됩니다.
