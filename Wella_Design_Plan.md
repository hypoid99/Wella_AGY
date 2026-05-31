# Wella 데스크탑 일정관리 앱 설계방안 (Design Plan)

C# .NET WinForms 환경에서 GDI+ 그래픽을 활용하여 세련되고 현대적인 UI/UX를 제공하고, 확장 가능한 MVC(Model-View-Controller) 패턴으로 설계된 모듈형 데스크탑 일정관리 애플리케이션 **Wella**의 설계방안입니다.

---

## 1. 기본 설계 원칙 및 환경
1. **개발 환경 및 그래픽 기술**
   - **언어 및 프레임워크**: C# / .NET WinForms
   - **그래픽 렌더링**: GDI+ (`System.Drawing`) 기반 커스텀 렌더링
   - **화면 깜빡임 방지**: Double Buffering 활성화 및 커스텀 더블 버퍼 패널/컨트롤 구현
   - **디자인 스타일**: 부드러운 그라데이션, 라운드 코너(둥근 모서리), 현대적인 카드 뷰 디자인, 마우스 호버 마이크로 인터랙션

2. **프로그램 아키텍처 (MVC 구조)**
   - **Model**: 데이터의 상태 관리 및 텍스트 파일 저장/로드 처리
   - **View**: GDI+를 사용한 화면 드로잉 및 사용자 이벤트 전달
   - **Controller**: 비즈니스 로직 제어, 뷰와 모델 간의 중재자 역할

3. **모듈식 도구 확장 방식**
   - 공통 인터페이스 `IToolModule` 정의를 통해 상단바/사이드바 메뉴와 각 도구 모듈 간 결합도를 낮춤
   - 신규 기능 추가 시 인터페이스를 상속받은 새 클래스(Model, View, Controller)를 프로젝트에 쉽게 등록할 수 있는 구조

4. **데이터 저장 방식**
   - 외부 DB 서버나 로컬 RDBMS(SQLite 등)를 절대 사용하지 않음
   - 각 도구별 독립된 `TEXT 파일(*.txt)`로 로컬 데이터 저장
   - 데이터 정합성 및 가독성을 위해 각 텍스트 파일 내부 데이터는 **JSON 직렬화** 포맷 사용

---

## 2. 프로젝트 디렉토리 및 파일 구성
```
Wella_AGY/
├── Wella_Design_Plan.md            # 본 설계 문서
├── Wella.sln                       # 솔루션 파일
└── Wella/
    ├── Program.cs                  # 애플리케이션 시작 진입점
    ├── AppConfig.cs                # 글로벌 테마 색상, 폰트 및 데이터 저장 경로 설정
    │
    ├── Common/                     # 공통 유틸리티 및 인터페이스
    │   ├── StorageManager.cs       # JSON 규격 텍스트 파일 입출력 헬퍼
    │   ├── GdiHelper.cs            # GDI+ 둥근 사각형 그리기 등 드로잉 헬퍼
    │   └── IToolModule.cs          # 모듈 연동을 위한 인터페이스
    │
    ├── Models/                     # 비즈니스 데이터 및 상태 정의
    │   ├── CalendarEvent.cs        # 일정 데이터 모델
    │   ├── TodoItem.cs             # 할일 데이터 모델
    │   ├── MemoItem.cs             # 메모 데이터 모델
    │   └── CalculatorState.cs      # 계산기 상태 모델
    │
    ├── Views/                      # 화면 표시 및 그리기 (UserControl 상속)
    │   ├── MainForm.cs             # 메인 프레임 (상단바, 좌측 사이드바, 콘텐츠 패널)
    │   ├── CalendarView.cs         # 달력 그래픽 뷰 (격자, 날짜 셀 렌더링)
    │   ├── TodoView.cs             # 할일 리스트 뷰 (둥근 카드 및 체크박스 렌더링)
    │   ├── MemoView.cs             # 메모 보드 뷰 (파스텔톤 카드형 메모 렌더링)
    │   └── CalculatorView.cs       # 계산기 패널 뷰 (플랫 버튼 및 디스플레이 렌더링)
    │
    └── Controllers/                # 로직 및 상호작용 제어
        ├── MainController.cs       # 전체 도구 전환 및 내비게이션 관리
        ├── CalendarController.cs   # 달력 일정 비즈니스 로직
        ├── TodoController.cs       # 할일 추가/토글/삭제 제어
        ├── MemoController.cs       # 메모 작성/수정/삭제 제어
        └── CalculatorController.cs # 계산 연산 및 수식 처리
```

---

## 3. 핵심 모듈별 상세 구현 설계

### 공통 인터페이스 (`IToolModule.cs`)
```csharp
public interface IToolModule
{
    string ToolName { get; }
    System.Drawing.Image ToolIcon { get; }
    System.Windows.Forms.UserControl GetView();
    void Initialize(StorageManager storage);
}
```

### 1단계: Calendar (달력 모듈)
- **Model**: `CalendarEvent`
  - 속성: `DateTime EventDate`, `string Title`, `string Description`, `Color ThemeColor`
- **View**: `CalendarView`
  - GDI+로 7열 격자 구조 그리기 (`Graphics.DrawString`, `FillRectangle`)
  - 이전달/다음달 이동 버튼 및 호버 인터랙션
  - 일정이 있는 날짜는 하단에 동그라미 표식 또는 파스텔톤 배경으로 강조
- **Controller**: `CalendarController`
  - 월간 일정을 메모리상에 캐시 및 필터링하여 뷰로 전달
  - 데이터 로드/저장을 `StorageManager`에 위임
- **저장소**: `calendar_data.txt` (JSON 포맷의 리스트 저장)

### 2단계: To-Do (할일 모듈)
- **Model**: `TodoItem`
  - 속성: `int Id`, `string Text`, `bool IsCompleted`, `int Priority` (1~3)
- **View**: `TodoView`
  - 둥근 사각형 테두리를 가진 카드 형태 리스트 그리기
  - 체크박스 클릭 시 GDI+ 그래픽으로 부드럽게 체크 마크 채우기 및 텍스트 취소선(`Strikeout`) 렌더링
- **Controller**: `TodoController`
  - 할일의 우선순위 정렬 및 완료 처리
- **저장소**: `todo_data.txt`

### 3단계: Memo (메모 모듈)
- **Model**: `MemoItem`
  - 속성: `int Id`, `string Title`, `string Content`, `DateTime UpdatedTime`, `string BgColorHex`
- **View**: `MemoView`
  - 핀보드(Pin Board) 형태로 포스트잇 질감의 메모 리스트 배치
  - 메모 카드 클릭 시 상세 편집 모달 또는 우측 패널 활성화
- **Controller**: `MemoController`
  - 텍스트 입력 변화 감지 및 자동 저장 기능
- **저장소**: `memo_data.txt`

### 4단계: Calculator (계산기 모듈)
- **Model**: `CalculatorState`
  - 속성: `string DisplayText`, `double CurrentValue`, `string LastOperator`
- **View**: `CalculatorView`
  - 세련된 다크/화이트 테마의 평면(Flat) 스타일 버튼 직접 드로잉
  - 키보드 입력 매핑 및 결과 디스플레이 창 그리기
- **Controller**: `CalculatorController`
  - 연속 계산 처리, 메모리 연산 기능 구현
- **저장소**: `calculator_history.txt` (이전 계산 기록 히스토리 파일)

---

## 4. UI 레이아웃 구성 설계
1. **메인 프레임 (`MainForm.cs`)**
   - `FormBorderStyle = FormBorderStyle.Sizable` (창 크기 조절 자유)
   - 크기: 기본 `1024 x 768` 픽셀 권장
2. **상단바 영역 (Top Panel)**
   - Wella 앱 로고 및 메인 드롭다운 메뉴
   - 빠른 모듈 전환 버튼 배치 (달력 | 할일 | 메모 | 계산기)
   - 호버 시 색상이 서서히 투명해지거나 변하는 그래픽 연출
3. **좌측 영역 (Sidebar Panel)**
   - 선택된 활성 모듈에 따른 서브 메뉴 트리 또는 기능별 숏컷 배치
4. **중앙 영역 (Content Panel)**
   - 선택된 도구의 `UserControl`이 도킹(`Dock = DockStyle.Fill`)되어 동적으로 변경됨

---

## 5. 검증 및 배포 전략
- **그래픽 리소스 해제**: 모든 커스텀 드로잉 코드에서 `Brush`, `Pen`, `Font` 등 GDI+ 자원은 사용 직후 반드시 `using` 블록으로 자동 해제(`Dispose`)하여 메모리 누수를 원천 차단함
- **데이터 안정성**: 텍스트 파일 저장 실패 시를 대비해 백업 파일(`*.bak`)을 우선 생성한 후 덮어쓰는 안전 쓰기(Safe Write) 메커니즘을 `StorageManager`에 내장함
