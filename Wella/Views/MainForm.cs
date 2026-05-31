using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Wella.Common;
using Wella.Controllers;

namespace Wella.Views
{
    public class MainForm : Form
    {
        private MainController _controller;
        
        // UI 컴포넌트
        private Panel _pnlTop;
        private Panel _pnlSidebar;
        private Panel _pnlContent;
        
        private Label _lblLogo;
        private Label _lblModuleTitle;
        private Label _lblModuleDesc;
        private FlowLayoutPanel _flpNavButtons;

        // 사이드바 날짜 대시보드 그래픽용 타이머 및 컴포넌트
        private System.Windows.Forms.Timer _clockTimer;
        private Label _lblSidebarTime;
        private Label _lblSidebarDate;

        public MainForm()
        {
            // 기본 창 속성 정의
            this.Text = "Wella - 프리미엄 데스크탑 일정관리";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppConfig.ColorBgDark;
            this.ForeColor = AppConfig.ColorTextLight;
            this.DoubleBuffered = true;

            InitializeLayout();
            InitializeDashboardTimer();
        }

        private void InitializeLayout()
        {
            // 1. 상단 패널 (높이 75px, 로고 및 상단 가로 탭 바)
            _pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 75,
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(20, 10, 20, 10)
            };
            
            // 구분 경계선 드로잉
            _pnlTop.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    e.Graphics.DrawLine(p, 0, _pnlTop.Height - 1, _pnlTop.Width, _pnlTop.Height - 1);
                }
            };

            // Wella 브랜드 로고
            _lblLogo = new Label
            {
                Text = "WELLA",
                ForeColor = AppConfig.ColorAccent,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                Location = new Point(20, 16),
                AutoSize = true
            };

            // 가로 내비게이션 툴 버튼 컨테이너
            _flpNavButtons = new FlowLayoutPanel
            {
                Location = new Point(170, 14),
                Size = new Size(800, 50),
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            _pnlTop.Controls.Add(_lblLogo);
            _pnlTop.Controls.Add(_flpNavButtons);

            // 2. 좌측 사이드바 패널 (너비 240px, 서브 메뉴 및 정보 뷰)
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(15)
            };
            
            _pnlSidebar.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    e.Graphics.DrawLine(p, _pnlSidebar.Width - 1, 0, _pnlSidebar.Width - 1, _pnlSidebar.Height);
                }
            };

            // 사이드바 헤더 - 현재 활성 도구 설명
            Label lblMenuHeader = new Label
            {
                Text = "대시보드",
                Font = AppConfig.FontSubtitle,
                ForeColor = AppConfig.ColorTextLight,
                Location = new Point(15, 20),
                Width = 210,
                Height = 30
            };

            // 사이드바 중앙의 GDI+ 디자인 카드 (시계 및 상태 표시용)
            Panel dbCard = new Panel
            {
                Location = new Point(15, 60),
                Width = 210,
                Height = 150,
                BackColor = AppConfig.ColorContentBg,
                Padding = new Padding(15)
            };

            dbCard.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 은은한 그라데이션 카드 배경
                Rectangle rect = new Rectangle(0, 0, dbCard.Width, dbCard.Height);
                GdiHelper.FillGradientRoundedRectangle(g, rect, AppConfig.ColorContentBg, Color.FromArgb(42, 40, 65), 12);
                
                // 테두리
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    GdiHelper.DrawRoundedRectangle(g, p, rect, 12);
                }
            };

            _lblSidebarTime = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = AppConfig.ColorAccent,
                Location = new Point(12, 20),
                Size = new Size(180, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            _lblSidebarDate = new Label
            {
                Text = "2026년 05월 31일",
                Font = AppConfig.FontBodyBold,
                ForeColor = AppConfig.ColorTextLight,
                Location = new Point(12, 65),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Label lblQuote = new Label
            {
                Text = "Wella와 함께하는 계획적인 하루",
                Font = AppConfig.FontSmall,
                ForeColor = AppConfig.ColorTextMuted,
                Location = new Point(12, 100),
                Size = new Size(180, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            dbCard.Controls.Add(_lblSidebarTime);
            dbCard.Controls.Add(_lblSidebarDate);
            dbCard.Controls.Add(lblQuote);

            // 하단 현재 모듈 간략 정보 영역
            _lblModuleTitle = new Label
            {
                Location = new Point(15, 235),
                Width = 210,
                Height = 30,
                Font = AppConfig.FontSubtitle,
                ForeColor = AppConfig.ColorAccent,
                Text = "선택된 도구"
            };

            _lblModuleDesc = new Label
            {
                Location = new Point(15, 270),
                Width = 210,
                Height = 120,
                Font = AppConfig.FontBody,
                ForeColor = AppConfig.ColorTextMuted,
                Text = "도구의 상세 설명이 여기에 표시됩니다."
            };

            _pnlSidebar.Controls.Add(lblMenuHeader);
            _pnlSidebar.Controls.Add(dbCard);
            _pnlSidebar.Controls.Add(_lblModuleTitle);
            _pnlSidebar.Controls.Add(_lblModuleDesc);

            // 3. 중앙 콘텐츠 뷰 컨테이너 Panel (여기에 각 모듈 뷰 도킹)
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppConfig.ColorContentBg,
                Padding = new Padding(20)
            };

            // 컨트롤 등록
            this.Controls.Add(_pnlContent);
            this.Controls.Add(_pnlSidebar);
            this.Controls.Add(_pnlTop);
        }

        private void InitializeDashboardTimer()
        {
            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += (s, e) =>
            {
                _lblSidebarTime.Text = DateTime.Now.ToString("HH:mm:ss");
                _lblSidebarDate.Text = DateTime.Now.ToString("yyyy년 MM월 dd일");
            };
            _clockTimer.Start();
        }

        // 컨트롤러 초기화 및 내비게이션 탭 자동 빌드
        public void InitializeModules(MainController controller)
        {
            _controller = controller;
            _flpNavButtons.Controls.Clear();

            var modules = _controller.GetModules();
            foreach (var mod in modules)
            {
                Button btn = CreateNavButton(mod);
                _flpNavButtons.Controls.Add(btn);
            }
        }

        // 아름다운 커스텀 그래픽 호버 버튼 생성
        private Button CreateNavButton(IToolModule module)
        {
            Button btn = new Button
            {
                Text = $" {module.ToolIconChar}  {module.ToolName}",
                Font = AppConfig.FontSubtitle,
                Size = new Size(140, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = AppConfig.ColorTextMuted,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 15, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 50);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 32, 42);

            btn.Click += (s, e) =>
            {
                _controller.SwitchToModule(module);
                HighlightActiveButton(btn);
            };

            return btn;
        }

        private void HighlightActiveButton(Button activeBtn)
        {
            foreach (Control c in _flpNavButtons.Controls)
            {
                if (c is Button b)
                {
                    if (b == activeBtn)
                    {
                        b.ForeColor = AppConfig.ColorTextLight;
                        b.BackColor = AppConfig.ColorAccent;
                    }
                    else
                    {
                        b.ForeColor = AppConfig.ColorTextMuted;
                        b.BackColor = Color.Transparent;
                    }
                }
            }
        }

        // 중앙 영역에 현재 도구의 UserControl을 렌더링하고 상태 바인딩
        public void ShowModuleView(UserControl view, string title, string description)
        {
            _pnlContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            _pnlContent.Controls.Add(view);

            _lblModuleTitle.Text = title;
            _lblModuleDesc.Text = description;

            // 버튼 하이라이트 재조정
            foreach (Control c in _flpNavButtons.Controls)
            {
                if (c is Button b && b.Text.Contains(title))
                {
                    HighlightActiveButton(b);
                    break;
                }
            }
        }
    }
}
