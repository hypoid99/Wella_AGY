# Wella 일정관리 데스크탑 앱 개발 인계인수 문서 (Handoff)

이 문서는 새 채팅창 또는 다른 AI 개발자가 Wella 일정을 관리할 수 있는 데스크탑 앱 개발 작업을 원활하게 이어받아 **2단계: 할일(To-Do) 모듈**을 구현할 수 있도록 인계하기 위해 작성되었습니다.

---

## 1. 프로젝트 개요 및 아키텍처
Wella는 **C# .NET 10.0 WinForms** 환경에서 **GDI+ 그래픽**을 활용하여 현대적이고 미려한 어두운 테마(Sleek Dark Theme)를 구현한 데스크탑 일정관리 애플리케이션입니다. 

코드의 가독성 및 확장성을 극대화하기 위해 **MVC(Model-View-Controller) 패턴**과 **모듈식 탭 시스템**을 채택하고 있으며, 별도의 외부 데이터베이스 없이 **로컬 텍스트 파일(JSON 직렬화)** 형식으로 데이터를 영구 저장합니다.

---

## 2. 현재 개발 단계 및 완료 사항
* **현재 단계**: **1단계(달력 모듈 및 메인 셸 프레임워크)** 개발 및 검증 완료.
* **빌드 검증**: 오류(Error) 0개, 경고(Warning) 0개로 완벽하게 빌드 완료.
* **완료된 핵심 구현 기능**:
  1. **메인 앱 셸 (`MainForm`, `MainController`)**:
     - 상단에 가로형 모듈 전환 탭 메뉴 배치 (`IToolModule` 인터페이스 기반으로 자동 버튼 생성 및 바인딩).
     - 좌측에 GDI+ 기반으로 둥글고 세련되게 그려진 실시간 디지털 시계 및 날짜 대시보드 카드 배치.
  2. **GDI+ 그래픽 유틸리티 (`GdiHelper`)**:
     - 안티앨리어싱 기반 둥근 모서리 그리기/채우기 및 부드러운 선형 그라데이션 카드 렌더링 헬퍼 구현.
  3. **데이터 저장소 (`StorageManager`)**:
     - JSON 문자열을 로컬 파일(`Data/*.txt`)로 동기화.
     - 파일 쓰기 시 임시 파일(`*.tmp`) 및 백업 파일(`*.bak`)을 활용하여 크래시로 인한 데이터 유실을 방지하는 안전 저장 장치 탑재.
  4. **1단계 달력 모듈 (`Calendar` - MVC)**:
     - GDI+로 7열 6행 날짜 격자(Grid)를 완전히 커스텀 드로잉.
     - 마우스 호버 피드백 및 날짜 클릭 선택 시 보라색(Accent) 하이라이트 활성화.
     - 일정이 존재하는 날짜 하단에는 중요도 등급 색상(Red, Yellow, Blue)을 반영한 예쁜 표시용 도트(Dot) 최대 4개 자동 렌더링.
     - **데일리 일정 우측 카드 패널**: 선택한 날의 스케줄 조회, 둥근 외곽선 스케줄 카드, 개별 일정 삭제(✕), 우선순위별 새 일정 추가 기능 탑재.

---

## 3. 작업 디렉토리 및 파일 구성
```
Wella_AGY/
├── Wella_Design_Plan.md            # 전체 개발 상세 설계 문서
├── Wella_Handoff.md                # 본 인계인수 문서 (New Chat용)
├── Wella.slnx                       # .NET 10 XML 기반 솔루션 파일
└── Wella/
    ├── Wella.csproj                # WinForms 설정 (Nullable 비활성화)
    ├── Program.cs                  # 진입점 (MainController 가동 및 모듈 등록)
    ├── AppConfig.cs                # 테마 색상(Sleek Dark, Accent), 공통 폰트, 저장경로 정의
    │
    ├── Common/
    │   ├── StorageManager.cs       # JSON 텍스트 파일 세이프 세이버
    │   ├── GdiHelper.cs            # GDI+ 둥근 사각형 및 그라데이션 헬퍼
    │   └── IToolModule.cs          # 모듈 인터페이스
    │
    ├── Models/
    │   └── CalendarEvent.cs        # 달력 일정 데이터 구조 (ID, 날짜, 제목, 설명, 중요도색)
    │
    ├── Controllers/
    │   ├── MainController.cs       # 화면 뷰 동적 교체 관리자
    │   └── CalendarController.cs   # 달력 비즈니스 로직 및 저장 제어
    │
    └── Views/
        ├── MainForm.cs             # 상단바, 사이드바, 동적 콘텐츠 뷰 전환 메인 컨테이너
        ├── CalendarView.cs         # GDI+ 달력 메인 뷰 및 우측 Daily 카드 리스트
        └── CalendarModule.cs       # IToolModule 구현 패키지
```

---

## 4. 새 채팅창에서의 다음 작업 목표: 2단계 (할일 - To-Do) 구현 방법
새로운 세션 또는 작업자가 이어서 진행할 **2단계: 할일(To-Do)** 모듈 개발 지침입니다.

### [1] Model 설계 (`Models/TodoItem.cs` 신규 추가)
- **클래스 정의**: 
  - `int Id` (자동 증가 또는 Guid 문자열)
  - `string Title` (할일 제목)
  - `bool IsCompleted` (완료 여부)
  - `int Priority` (1: 높음, 2: 보통, 3: 낮음)
  - `DateTime CreatedTime` (생성일)

### [2] Controller 구현 (`Controllers/TodoController.cs` 신규 추가)
- `StorageManager`를 통해 `Data/todo_data.txt` 파일에 입출력 연동.
- 주요 비즈니스 메서드:
  - `LoadTodos()` / `SaveTodos()`
  - `AddTodo(string title, int priority)`
  - `ToggleTodo(int id)` (완료 상태 반전)
  - `DeleteTodo(int id)`
  - 정렬 기능 (우선순위 순 및 완료되지 않은 일정을 상단 배치)

### [3] View 및 모듈 구현 (`Views/TodoView.cs` & `TodoModule.cs` 신규 추가)
- **UI 구조**:
  - 좌측/상단: 새 할일 입력 영역 (텍스트 입력창, 우선순위 콤보박스, 추가 버튼).
  - 메인 영역: 스크롤 가능한 FlowLayoutPanel을 배치하고, 할일 아이템을 GDI+ 커스텀 카드 컨트롤로 렌더링.
- **GDI+ 페인팅 포인트**:
  - `TodoView` 카드에 호버 효과 적용.
  - 완료 상태 시 텍스트 취소선(`Strikeout`) 효과 렌더링.
  - 완료 체크박스 클릭 시 동그란 테두리에 체크 마크가 예쁘게 채워지는 그래픽 적용.
  - 중요도별로 왼쪽 테두리에 파스텔톤 강조 표시.
- **모듈 연동**:
  - `TodoModule : IToolModule` 클래스를 작성하고 `ToolName => "할일"`, `ToolIconChar => "✔"` 정의.

### [4] 프로그램 진입점 등록 (`Program.cs` 수정)
- `Program.cs`의 `Main` 함수에 `controller.RegisterModule(new TodoModule());` 추가 등록하여 상단바 탭에 자동으로 할일 메뉴가 확장되도록 결합.

---

## 5. 유용한 개발 명령어
```powershell
# 1. 의존성 복원 및 빌드 검증
dotnet build

# 2. Wella 애플리케이션 즉시 실행
dotnet run --project Wella
```
