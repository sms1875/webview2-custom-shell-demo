# .NET Framework 4.8 + WebView2 커스텀 셸 구현 케이스 정리

## 목표

`.NET Framework 4.8` WinForms에서 `WebView2`를 감싸는 폼을 만들 때 아래 요구사항을 `DWM 비사용` 전제로 구현하는 방법을 정리합니다.

1. 커스텀 타이틀 바
2. 라운드 8 코너
3. 그림자 효과
4. Win7, Win10 등 구버전 포함 고려

---

## 먼저 결론

가장 먼저 시도할 기본 조합은 아래가 좋습니다.

- 타이틀 바: `Borderless Form + WM_NCHITTEST/ReleaseCapture`
- 라운드: `CreateRoundRectRgn + SetWindowRgn`
- 그림자: `CS_DROPSHADOW`

이 조합의 장점은 구현량이 작고, WinForms/WebView2와 충돌 가능성이 비교적 낮다는 점입니다.

다만 그림자가 더 진하거나 더 부드러워야 하면 다음 단계로 `별도 Shadow Window` 방식을 검토하는 것이 좋습니다.

---

## 요구사항별 케이스

## 1. 커스텀 타이틀 바

### case 1: `FormBorderStyle.None` + 상단 패널 직접 구성

방식:

- 기본 시스템 타이틀 바를 제거
- 상단 `Panel`에 아이콘, 제목, 버튼(최소화/최대화/닫기) 배치
- 드래그는 `ReleaseCapture + SendMessage(WM_NCLBUTTONDOWN, HTCAPTION)` 또는 `WM_NCHITTEST`로 처리
- 리사이즈는 `WM_NCHITTEST`에서 모서리/변 가장자리 hit test 직접 처리

장점:

- 가장 단순하고 예측 가능
- 타이틀 바 높이, 색, 아이콘, 글자, 폰트를 완전히 통제 가능
- WebView2와 함께 쓰기 쉬움

단점:

- 최소화/최대화/복원, 작업 영역 맞춤, resize hit-test를 직접 챙겨야 함

적합도:

- 가장 추천

오픈소스 사례:

- `RJCodeAdvance/Modern-GUI-Multi-Form-Winform`
  - `FormBorderStyle.None`
  - `WndProc`로 `WM_NCHITTEST`
  - 상단 패널 드래그 처리
  - 링크: [GitHub](https://github.com/RJCodeAdvance/Modern-GUI-Multi-Form-Winform)

참고 API:

- `WM_NCHITTEST`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-nchittest)

### case 2: 라이브러리 기반 커스텀 프레임

방식:

- 폼을 직접 꾸미는 라이브러리 또는 베이스 폼 사용
- 내부적으로 title bar, border, shadow window를 캡슐화

장점:

- 처음 붙이는 속도가 빠름
- 그림자나 프레임 처리가 이미 구현된 경우가 있음

단점:

- Win7, 고DPI, WebView2와 조합 시 세부 충돌을 직접 디버깅해야 할 수 있음
- 유지보수/포크 비용이 생길 수 있음

오픈소스 사례:

- `NetDimension/WinForm-ModernUI`
  - 프레임리스 폼, border, drop shadow 지원을 표방
  - 내부적으로 별도 shadow window들을 관리
  - 링크: [GitHub](https://github.com/NetDimension/WinForm-ModernUI)
  - NuGet: `NetDimension.WinForm.ModernUI`

적합도:

- 빠른 프로토타입에는 좋지만, 최종 제품에서는 코드 이해 후 부분 차용 권장

---

## 2. 모서리 라운드 8 적용

### case 1: `CreateRoundRectRgn + SetWindowRgn`

방식:

- 폼 전체를 둥근 사각형 region으로 잘라냄
- 반지름 8이면 ellipse 크기를 16x16 정도로 전달

장점:

- Win7 포함 구버전 대응이 쉬움
- 구현이 간단하고 성능 부담이 작음
- WebView2가 들어가도 부모 창 모양만 바꾸는 구조라 비교적 안전

단점:

- 진짜 안티앨리어싱이 아니라 region 기반이라 가장자리가 아주 부드럽지는 않을 수 있음
- 최대화 시 region을 제거하는 처리 필요

적합도:

- 가장 추천

참고 API:

- `CreateRoundRectRgn`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-createroundrectrgn)
- `SetWindowRgn`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowrgn)

### case 2: 투명색으로 원 외부를 비우는 방식

방식:

- `TransparencyKey`와 특정 배경색을 이용해 바깥 모서리 부분을 투명하게 비움
- 예시로, 폼 배경에 마스크 색을 칠하고 둥근 외곽 밖을 해당 색으로 그린 뒤 `TransparencyKey`와 맞춤

장점:

- 아이디어가 직관적
- region보다 시각적으로 더 부드럽게 보이도록 트릭을 줄 수 있음

단점:

- 깜빡임, 색 번짐, 자식 컨트롤과의 상호작용, 클릭 판정 이슈가 생기기 쉬움
- WebView2 같은 HWND 기반 컨트롤과 함께 쓸 때 더 조심해야 함

적합도:

- 실험용 케이스로는 가능하지만, 기본 채택은 비추천

참고 API:

- `Form.TransparencyKey`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.form.transparencykey)

### case 3: `Layered Window / UpdateLayeredWindow` 기반 per-pixel alpha

방식:

- 윈도우 자체를 레이어드 윈도우로 만들고 알파 채널로 외곽을 부드럽게 그림

장점:

- 가장 예쁜 모서리/그림자 표현 가능

단점:

- 구현 복잡도가 높음
- WinForms 표준 흐름에서 벗어남
- WebView2는 HWND 기반 컨트롤이라 layered parent, airspace, 입력/합성 이슈를 반드시 검증해야 함

적합도:

- 이번 요구사항의 1차 시도용으로는 비추천

참고 API:

- `UpdateLayeredWindow`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-updatelayeredwindow)

---

## 3. 그림자 효과

### case 1: `CS_DROPSHADOW`

방식:

- `CreateParams.ClassStyle`에 `CS_DROPSHADOW` 추가

장점:

- 구현이 가장 간단
- DWM 없이도 전통적인 그림자를 얻는 가장 쉬운 방식
- 샘플 프로젝트에 바로 넣기 좋음

단점:

- 그림자 모양/범위/알파를 세밀하게 제어할 수 없음
- OS/테마에 따라 표현 품질 차이가 있음

적합도:

- 첫 번째 시도에 적합

참고 API:

- `Window Class Styles`의 `CS_DROPSHADOW`
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-class-styles)

### case 2: 별도 Shadow Form 또는 Shadow Window 4면 구성

방식:

- 본 창 주변에 비활성 보조 윈도우를 두고, PNG/비트맵 또는 직접 그리기로 그림자를 표현
- 본 창 이동/리사이즈/활성화 상태에 맞춰 shadow window를 동기화

장점:

- 그림자 색, 두께, 블러 느낌을 통제 가능
- DWM 없이도 비교적 그럴듯한 그림자를 만들 수 있음

단점:

- 창 이동, z-order, 활성/비활성 전환, 최대화, Alt-Tab, owner 관계 등을 꼼꼼히 다뤄야 함
- 구현량이 많음

적합도:

- `CS_DROPSHADOW`가 품질상 부족할 때 2차 선택지로 추천

오픈소스 사례:

- `NetDimension/WinForm-ModernUI`
  - `ChromeDecorator`, `ChromeShadowElement`, `ShadowTemplate.png` 기반으로 별도 그림자 요소를 운영
  - 링크: [GitHub](https://github.com/NetDimension/WinForm-ModernUI)

### case 3: 폼 내부 가장자리에 fake shadow 그리기

방식:

- 폼 바깥이 아니라 안쪽 border 영역에 반투명 그라데이션을 그림

장점:

- 구현이 쉬움
- 외부 shadow window가 필요 없음

단점:

- 진짜 바깥 그림자가 아니라서 떠 있는 창 느낌은 약함
- 콘텐츠 영역이 줄어듦

적합도:

- 외부 그림자 대안이 필요할 때만 보조적으로 고려

---

## WebView2와 같이 쓸 때의 판단

### 가장 안전한 조합

- `FormBorderStyle.None`
- `WM_NCHITTEST`
- `CreateRoundRectRgn`
- `CS_DROPSHADOW`

이 조합은 WebView2를 일반 자식 컨트롤처럼 두는 구조라 충돌 면적이 가장 작습니다.

### 주의할 조합

- `TransparencyKey`
- `Layered Window`
- `UpdateLayeredWindow`

이 계열은 WebView2가 별도 HWND를 가지는 특성상, 합성이나 투명 처리에서 문제가 생길 수 있으므로 별도 POC 없이 바로 채택하지 않는 편이 좋습니다.

---

## Win7 고려사항

### 1. DWM에 기대지 않는 것이 맞음

Win7에서도 동작시키려면 rounded corner, shadow를 앱이 직접 책임지는 접근이 필요합니다.

### 2. WebView2 런타임 배포는 별도 판단 필요

WebView2 자체는 최신 패키지를 참조해도 실제 런타임 배포 전략은 따로 잡아야 합니다.

- Evergreen Runtime: 최신 Windows 환경에는 편하지만 구형 OS 지원 정책 영향이 큼
- Fixed Version Runtime: 특정 버전을 앱과 함께 배포하는 방식이라 구형 OS 대응 검토에 유리

공식 문서:

- Get started with WebView2 in WinForms
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/winforms)
- Distribution
  - [learn.microsoft.com](https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution)
- 공식 샘플 저장소
  - [MicrosoftEdge/WebView2Samples](https://github.com/MicrosoftEdge/WebView2Samples)

실무 메모:

- Microsoft Edge 팀 공지 기준으로, `Windows 7/8/8.1`에서 지원되는 마지막 브라우저/런타임은 `버전 109`입니다.
- 해당 공지는 `2022-12-09` 게시, `2023-01-10` OS 지원 종료, `2023-01-12` 주간에 `109` 릴리스 예정이라고 안내합니다.
- 또한 `WebView2 SDK 1.0.1519.0 이상`은 `Windows 7/8/8.1`을 더 이상 지원하지 않는다고 명시되어 있습니다.
- 따라서 Win7을 반드시 지원해야 하면, 창 외형 구현보다 먼저 `WebView2 버전 전략`부터 확정해야 합니다.
- 창 외형보다 WebView2 런타임 지원 여부가 더 큰 리스크가 될 수 있습니다.

정책 참고:

- Microsoft Edge and WebView2 ending support for Windows 7 and Windows 8/8.1
  - [blogs.windows.com](https://blogs.windows.com/msedgedev/2022/12/09/microsoft-edge-and-webview2-ending-support-for-windows-7-and-windows-8-8-1/)

---

## 이번에 만든 샘플 프로젝트의 선택

샘플 프로젝트는 아래 전략을 채택했습니다.

- 타이틀 바: 직접 그린 상단 패널
- 타이틀 드래그: `WM_NCLBUTTONDOWN + HTCAPTION`
- 리사이즈: `WM_NCHITTEST`
- 라운드: `CreateRoundRectRgn`
- 그림자: `CS_DROPSHADOW`
- 최대화: 작업 영역 계산 후 region 제거

프로젝트:

- [`src/CustomShellWebView2Demo/CustomShellWebView2Demo.csproj`](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\CustomShellWebView2Demo.csproj)

핵심 코드:

- [`src/CustomShellWebView2Demo/ShellForm.cs`](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs)
- [`src/CustomShellWebView2Demo/NativeMethods.cs`](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\NativeMethods.cs)

---

## 바로 다음 실험 순서 추천

1. 현재 샘플로 커스텀 타이틀 바 + 라운드 + 기본 그림자 동작 확인
2. `CS_DROPSHADOW` 품질이 부족하면 `별도 Shadow Window` 방식 POC 추가
3. Win7이 필수라면 WebView2 Fixed Runtime 대상 버전부터 먼저 확정
4. 그 다음에 고DPI, 다중 모니터, 최대화/복원, Alt-Tab, owner dialog 케이스 검증

---

## 바로 적용 가능한 오픈소스/템플릿 후보

### 후보 1: Microsoft 공식 WebView2 샘플

- 용도: WebView2 초기화/배포 기준점
- 장점: 가장 신뢰할 수 있는 출발점
- 권장 사용법: WebView2 부분만 참고하고, 창 외형은 직접 구현
- 링크: [GitHub](https://github.com/MicrosoftEdge/WebView2Samples)

### 후보 2: NetDimension.WinForm.ModernUI

- 용도: drop shadow/프레임 구현 참고
- 장점: DWM 없이 shadow window 방식 사례를 바로 볼 수 있음
- 주의: 그대로 넣기보다는 shadow 처리 부분만 차용 권장
- 링크: [GitHub](https://github.com/NetDimension/WinForm-ModernUI)

### 후보 3: Modern-GUI-Multi-Form-Winform

- 용도: borderless + title bar drag + resize hit-test 참고
- 장점: 구조가 단순해서 빠르게 이해 가능
- 주의: WebView2, 라운드, 고DPI 대응은 직접 보강 필요
- 링크: [GitHub](https://github.com/RJCodeAdvance/Modern-GUI-Multi-Form-Winform)
