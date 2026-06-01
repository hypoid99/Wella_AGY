# Wella 일정관리 데스크탑 앱 개발 인계인수 문서 (Handoff)

이 문서는 새 채팅창 또는 다른 AI 개발자가 Wella 일정관리 데스크탑 앱 개발 작업을 원활하게 이어받아 **2단계: 할일(To-Do) 모듈**을 곧바로 구현할 수 있도록 인계하기 위해 작성되었습니다.

---

## 1. 프로젝트 개요 및 현재 개발 단계
Wella는 **C# .NET 10.0 WinForms** 환경에서 **GDI+ 그래픽**을 활용하여 미니멀하고 현대적인 어두운 테마(Sleek Dark Theme)를 구현한 데스크탑 일정관리 애플리케이션입니다.

* **현재 단계**: **1단계 (달력 모듈, 레이아웃 미니멀 개편, 초고속 렌더링 최적화, 대한민국 공휴일 탑재)** 완벽 완료 및 빌드 성공.
* **빌드 검증**: 오류(Error) 0개, 경고(Warning) 0개로 완벽하게 빌드 완료.
* **로컬 구동 명령어**: `dotnet run --project Wella`

---

## 2. 이미 구현 완료된 핵심 기능 및 파일 요약
1. **미니멀 메인 앱 셸 (`MainForm.cs`, `MainController.cs`)**:
   * 좌/우 사이드바를 완전히 제거하여 화면 낭비를 없애고, 가로 `1000 x 700` 해상도에 최적화된 메인 뷰 컨테이너 구조 구축.
   * `WS_EX_COMPOSITED` (OS 수준 통합 더블 버퍼링) 기법을 오버라이드하여 창 크기를 빠르게 조절하거나 최대화할 때 깜빡임(Flicker)과 그래픽 랙을 완벽 제거.
2. **1단계 달력 및 팝업 모달 (`CalendarView.cs`, `EventDialog.cs`)**:
   * GDI+로 7열 격자를 전체 화면에 시원하게 렌더링하며, `ResizeRedraw = true` 설정을 더해 실시간 창 조절 시 달력 선이 찢어짐 없이 갱신됨.
   * 날짜 클릭 시 어두운 모달 팝업인 `EventDialog`가 열리며 일정 조회/추가/삭제를 처리하고 메인 달력 아래의 점 표시(Dot)를 실시간 동기화.
3. **대한민국 공휴일 엔진 (`HolidayHelper.cs`, `GdiHelper.cs`)**:
   * .NET 내장 `KoreanLunisolarCalendar`를 활용해 신정 등 양력 공휴일과 매년 유동적으로 바뀌는 음력 공휴일(설날, 추석 연휴, 석탄일)을 동적 계산.
   * 공휴일인 날짜는 파스텔 레드로 표시하고 날짜 우측 옆에 공휴일 명칭이 미세 폰트로 예쁘게 병렬 출력됨.

---

## 3. 새 채팅창에서의 다음 작업 목표: 2단계 (할일 - To-Do) 구현 방법
새로운 세션 또는 다음 개발자가 이어서 진행할 **2단계: 할일(To-Do)** 모듈 개발 지침입니다.

### [1] Model 설계 (`Models/TodoItem.cs` 신규 추가)
* **필수 멤버**:
  * `int Id` (자동 증가 일련번호 또는 Guid 문자열)
  * `string Title` (할일 내용)
  * `bool IsCompleted` (완료 상태)
  * `int Priority` (1: 높음-Red, 2: 보통-Yellow, 3: 낮음-Blue)
  * `DateTime CreatedTime` (생성일시)

### [2] Controller 구현 (`Controllers/TodoController.cs` 신규 추가)
* `StorageManager`를 통해 `Data/todo_data.txt` 파일에 JSON 직렬화 연동.
* 주요 비즈니스 메서드:
  * `LoadTodos()` / `SaveTodos()`
  * `AddTodo(string title, int priority)`
  * `ToggleTodo(int id)` (완료 상태 반전 및 저장)
  * `DeleteTodo(int id)`
  * **정렬 로직**: 우선순위 순으로 정렬하고 완료되지 않은 할일을 상단에 우선 배치.

### [3] View 및 모듈 구현 (`Views/TodoView.cs` & `TodoModule.cs` 신규 추가)
* **UI 구성 및 GDI+ 렌더링**:
  * 스크롤 가능한 영역에 FlowLayoutPanel을 배치하고, 할일 카드를 GDI+ 커스텀 카드 컨트롤로 드로잉.
  * 마우스 호버 효과 제공 및 우선순위별로 왼쪽 테두리에 파스텔톤 컬러(`AppConfig` 색상 참조) 강조 표시선 렌더링.
  * 완료 상태로 변경되면 글씨에 **취소선(`Strikeout`)** 효과를 입히고, 체크박스 내부가 GDI+로 부드럽게 채워지는 그래픽 렌더링 적용.
* **모듈 연동**:
  * `TodoModule : IToolModule` 클래스를 생성하고 `ToolName => "할일"`, `ToolIconChar => "✔"`, `GetView() => TodoView 인스턴스` 정의.

### [4] 프로그램 진입점 등록 (`Program.cs` 수정)
* `Program.cs` 파일의 모듈 등록 구간에 `controller.RegisterModule(new TodoModule());` 코드를 추가하여 상단 가로 탭 바에 자동으로 "✔ 할일" 메뉴가 연동되어 확장되도록 마감합니다.
