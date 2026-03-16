# 기능 및 기술 선택 가이드

## 문서 목적

이 문서는 현재 샘플 프로젝트가 제공하는 기능, 검토했던 기술 후보, 각 후보를 어떻게 사용할 수 있는지, 그리고 이번 샘플에서 왜 현재 방식을 선택했는지를 한 번에 설명하기 위한 문서입니다.

비교 케이스 중심 문서는 [implementation-cases.md](C:\Users\user\Documents\Playground\docs\implementation-cases.md)에서 볼 수 있고, 이 문서는 그 내용을 설계 관점에서 요약한 안내서입니다.

## 이 프로젝트가 제공하는 기능

현재 샘플 프로젝트는 아래 기능을 제공합니다.

- 시스템 기본 프레임이 아닌 커스텀 타이틀 바
- 타이틀 바 드래그 이동
- 최소화, 최대화, 닫기 버튼
- 모서리 라운드 8px
- DWM 없이 기본 그림자 효과
- 테두리 리사이즈 hit-test
- 최대화 시 작업 영역 맞춤
- WebView2 내장 브라우저 영역

핵심 구현 파일:

- [ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs)
- [NativeMethods.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\NativeMethods.cs)

## 요구사항별 기술 후보

## 1. 커스텀 타이틀 바

### 후보 A: `FormBorderStyle.None` + 직접 그린 타이틀 바

사용 방법:

- 폼의 `FormBorderStyle`을 `None`으로 둡니다.
- 상단에 `Panel`을 두고 아이콘, 제목, 버튼을 직접 배치합니다.
- 이동은 `ReleaseCapture`와 `SendMessage(WM_NCLBUTTONDOWN, HTCAPTION)` 또는 `WM_NCHITTEST`로 처리합니다.
- 가장자리 리사이즈는 `WM_NCHITTEST`에서 직접 반환합니다.

장점:

- 제어 범위가 가장 넓습니다.
- 타이틀 바 높이, 색, 폰트, 버튼 배치를 완전히 원하는 대로 만들 수 있습니다.
- WebView2를 포함한 일반 WinForms 컨트롤 조합과 잘 맞습니다.

단점:

- 시스템 프레임이 해주던 동작을 일부 직접 구현해야 합니다.

이번 샘플에서 채택 여부:

- 채택

선택 이유:

- 요구사항 1번과 가장 직접적으로 맞습니다.
- Win7 포함 구형 환경을 고려해도 구현 예측이 쉽습니다.
- DWM에 의존하지 않아도 됩니다.

### 후보 B: 커스텀 프레임 라이브러리 사용

사용 방법:

- 외부 라이브러리의 베이스 폼을 상속하거나 패키지를 붙입니다.
- 라이브러리 속성으로 그림자, 테두리, 타이틀 스타일을 설정합니다.

장점:

- 빠르게 프로토타입을 만들 수 있습니다.

단점:

- 내부 동작을 이해하지 못하면 WebView2, 고DPI, 구형 OS 조합에서 디버깅이 어려워집니다.
- 유지보수 책임이 라이브러리 상태에 크게 좌우됩니다.

이번 샘플에서 채택 여부:

- 미채택

미채택 이유:

- 이번 저장소는 "어떤 기술을 직접 선택해야 하는지" 보여주는 샘플 성격이 더 강합니다.
- 외부 라이브러리에 감춰진 구현 대신, 핵심 Win32 메시지 흐름을 직접 드러내는 편이 문서 목적에 더 맞았습니다.

## 2. 라운드 코너

### 후보 A: `CreateRoundRectRgn + SetWindowRgn`

사용 방법:

- 폼 리사이즈 시점이나 초기화 시점에 `CreateRoundRectRgn`으로 둥근 region을 생성합니다.
- `SetWindowRgn`으로 해당 region을 폼에 적용합니다.
- 최대화 상태에서는 region을 제거합니다.

장점:

- 구현이 단순합니다.
- Win7 같은 구형 환경에서도 비교적 안정적으로 동작합니다.
- WebView2와 함께 사용할 때 충돌 가능성이 낮습니다.

단점:

- 완전한 per-pixel anti-aliasing처럼 아주 부드럽지는 않습니다.

이번 샘플에서 채택 여부:

- 채택

선택 이유:

- `라운드 8` 요구사항을 가장 적은 리스크로 만족시킵니다.
- DWM 없이 구현 가능하고, 문서화도 쉽습니다.

### 후보 B: `TransparencyKey` 기반 모서리 비우기

사용 방법:

- 폼에 특정 색을 `TransparencyKey`로 지정합니다.
- 모서리 바깥을 그 색으로 칠해 잘린 것처럼 보이게 만듭니다.

장점:

- 개념적으로 이해하기 쉽습니다.

단점:

- 클릭 영역, 깜빡임, 자식 컨트롤 합성 문제가 생길 수 있습니다.
- WebView2와 조합할 때 주의가 필요합니다.

이번 샘플에서 채택 여부:

- 미채택

미채택 이유:

- 문서 예시용 아이디어로는 좋지만, 실사용 기본안으로는 region 방식보다 불안정합니다.

### 후보 C: `Layered Window / UpdateLayeredWindow`

사용 방법:

- 윈도우를 레이어드 방식으로 만들고 알파 채널로 가장자리를 부드럽게 표현합니다.

장점:

- 가장 시각적으로 완성도가 높습니다.

단점:

- 구현 복잡도가 높습니다.
- HWND 기반 컨트롤인 WebView2와의 조합에서 검증 포인트가 많습니다.

이번 샘플에서 채택 여부:

- 미채택

미채택 이유:

- 이번 저장소의 목표는 "바로 시도 가능한 안정적인 기본안" 제공입니다.
- layered 계열은 2차 실험 주제로 두는 편이 맞습니다.

## 3. 그림자 효과

### 후보 A: `CS_DROPSHADOW`

사용 방법:

- 폼의 `CreateParams.ClassStyle`에 `CS_DROPSHADOW`를 추가합니다.

장점:

- 구현이 가장 간단합니다.
- DWM 없이도 가장 빠르게 기본 그림자를 얻을 수 있습니다.

단점:

- 모양과 강도를 세밀하게 조절하기 어렵습니다.

이번 샘플에서 채택 여부:

- 채택

선택 이유:

- 이 저장소의 1차 목표는 "동작하는 기본 조합"입니다.
- 가장 적은 코드로 바로 결과를 보여줄 수 있습니다.

### 후보 B: 별도 Shadow Window

사용 방법:

- 본 창 주변에 보조 윈도우를 두고 그림자를 직접 그립니다.
- 본 창 이동, 크기 변경, 활성 상태에 따라 같이 움직이도록 만듭니다.

장점:

- 그림자 품질과 색을 세밀하게 조절할 수 있습니다.

단점:

- 구현량이 많고, 관리할 상태가 늘어납니다.

이번 샘플에서 채택 여부:

- 미채택

미채택 이유:

- 2차 확장 포인트로는 좋지만, 첫 샘플부터 넣기엔 구조가 복잡해집니다.
- 대신 관련 오픈소스 사례를 문서에 연결했습니다.

### 후보 C: 내부 fake shadow

사용 방법:

- 폼 안쪽 가장자리에 반투명 그라데이션을 그려 shadow처럼 보이게 합니다.

장점:

- 외부 shadow window가 필요 없습니다.

단점:

- 실제 떠 있는 창의 그림자처럼 보이지는 않습니다.

이번 샘플에서 채택 여부:

- 미채택

미채택 이유:

- 요구사항의 "창 그림자" 느낌과 거리가 있습니다.

## 왜 이 조합을 선택했는가

이번 샘플의 선택 조합은 아래와 같습니다.

- 타이틀 바: `FormBorderStyle.None` + 직접 그린 상단 패널
- 이동/리사이즈: `WM_NCHITTEST`, `WM_NCLBUTTONDOWN`
- 라운드: `CreateRoundRectRgn + SetWindowRgn`
- 그림자: `CS_DROPSHADOW`

이 조합을 고른 이유는 아래 4가지입니다.

1. 구현 난이도 대비 결과가 가장 좋습니다.
2. WebView2와 같이 둘 때 충돌 가능성이 낮습니다.
3. DWM을 쓰지 않는 조건을 만족합니다.
4. Win7 같은 구형 환경을 고려할 때 설명 가능하고 디버깅 가능한 구조입니다.

## 실제로 코드에서 어떻게 쓰고 있는가

### 타이틀 바

[ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs)에서 `_titleBar`, `_iconBox`, `_titleLabel`, `_minButton`, `_maxButton`, `_closeButton`을 직접 생성합니다.

이 구조 덕분에 아래 항목을 모두 코드에서 바로 바꿀 수 있습니다.

- 타이틀 바 높이
- 타이틀 바 배경색
- 아이콘
- 제목 텍스트
- 폰트
- 버튼 hover/down 색

### 라운드 코너

[ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs) `ApplyRoundedRegion()`에서 region을 적용합니다.

바꾸는 방법:

- `CornerRadius` 값을 바꾸면 됩니다.
- 현재 값은 `8`입니다.

### 그림자

[ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs) `CreateParams` 오버라이드에서 `CS_DROPSHADOW`를 추가합니다.

바꾸는 방법:

- 기본 shadow를 유지하려면 그대로 둡니다.
- 더 강한 그림자가 필요하면 향후 `별도 Shadow Window` 클래스를 추가하는 방식으로 확장합니다.

### WebView2

[ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs) `InitializeWebViewAsync()`에서 WebView2를 초기화합니다.

바꾸는 방법:

- 시작 URL을 변경할 수 있습니다.
- WebView2 설정값을 여기서 같이 조정할 수 있습니다.

## 이 프로젝트를 어떻게 확장하면 좋은가

추천 확장 순서는 아래와 같습니다.

1. 현재 샘플을 기준으로 타이틀바 디자인 값부터 맞춥니다.
2. 라운드 값과 내부 여백을 실제 UI에 맞게 조정합니다.
3. 그림자가 약하면 별도 `ShadowForm` 기반 구현을 추가합니다.
4. Win7이 필수면 WebView2 버전 전략을 먼저 고정합니다.
5. 그 다음에 다중 모니터, 고DPI, 최대화/복원, 팝업 창을 검증합니다.

## 문서 읽는 순서 추천

1. 이 문서로 전체 구조 이해
2. [implementation-cases.md](C:\Users\user\Documents\Playground\docs\implementation-cases.md)로 후보별 장단점 확인
3. [ShellForm.cs](C:\Users\user\Documents\Playground\src\CustomShellWebView2Demo\ShellForm.cs)에서 실제 코드 확인

## 참고 오픈소스와 용도

- [MicrosoftEdge/WebView2Samples](https://github.com/MicrosoftEdge/WebView2Samples)
  - WebView2 초기화와 배포 기준점
- [NetDimension/WinForm-ModernUI](https://github.com/NetDimension/WinForm-ModernUI)
  - shadow window 방식 참고
- [RJCodeAdvance/Modern-GUI-Multi-Form-Winform](https://github.com/RJCodeAdvance/Modern-GUI-Multi-Form-Winform)
  - borderless title bar, drag, resize 흐름 참고
