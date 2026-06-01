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
        private Panel _pnlContent;
        
        private Label _lblLogo;
        private FlowLayoutPanel _flpNavButtons;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED: OS 수준에서 전체 폼 및 모든 자식 컨트롤들의 더블 버퍼링 활성화 (깜빡임 완전 차단 및 고속 렌더링)
                return cp;
            }
        }

        public MainForm()
        {
            // 기본 창 속성 정의
            this.Text = "Wella - 프리미엄 데스크탑 일정관리";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppConfig.ColorBgDark;
            this.ForeColor = AppConfig.ColorTextLight;
            this.DoubleBuffered = true;

            InitializeLayout();
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

            // 2. 중앙 콘텐츠 뷰 컨테이너 Panel (여기에 각 모듈 뷰 도킹)
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = AppConfig.ColorContentBg,
                Padding = new Padding(20)
            };

            // 컨트롤 등록
            this.Controls.Add(_pnlContent);
            this.Controls.Add(_pnlTop);
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
