using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Wella.Common;
using Wella.Controllers;
using Wella.Models;

namespace Wella.Views
{
    public class EventDialog : Form
    {
        private readonly CalendarController _controller;
        private readonly DateTime _date;
        private readonly Action _onDataChanged;

        private FlowLayoutPanel _flpEvents;
        private TextBox _txtEventTitle;
        private TextBox _txtEventDesc;
        private ComboBox _cmbColor;
        private Button _btnAddEvent;
        private Label _lblHeader;

        public EventDialog(CalendarController controller, DateTime date, Action onDataChanged)
        {
            _controller = controller;
            _date = date;
            _onDataChanged = onDataChanged;

            InitializeComponent();
            LoadEvents();
        }

        private void InitializeComponent()
        {
            // 다이얼로그 폼 속성 설정 (어두운 Sleek Dark 테마 적용)
            this.Text = "Wella - 일정 상세 및 추가";
            this.Size = new Size(420, 600);
            this.MinimumSize = new Size(420, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppConfig.ColorBgDark;
            this.ForeColor = AppConfig.ColorTextLight;
            this.Font = AppConfig.FontBody;

            // 1. 상단 날짜 타이틀 헤더 영역
            _lblHeader = new Label
            {
                Text = $"{_date:yyyy년 MM월 dd일} 일정",
                Font = AppConfig.FontSubtitle,
                ForeColor = AppConfig.ColorAccent,
                Location = new Point(20, 15),
                Width = 360,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_lblHeader);

            // 2. 일정 카드 리스트 스크롤 영역 (FlowLayoutPanel)
            _flpEvents = new FlowLayoutPanel
            {
                Location = new Point(20, 50),
                Size = new Size(365, 230),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(8)
            };
            
            // FlowLayoutPanel의 테두리를 둥글게 그리기 위한 Paint 핸들러
            _flpEvents.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    Rectangle rect = new Rectangle(0, 0, _flpEvents.Width - 1, _flpEvents.Height - 1);
                    GdiHelper.DrawRoundedRectangle(g, p, rect, 8);
                }
            };
            // 내부 스크롤바 생성 및 사이즈 변경에 따른 카드 너비 반응형 처리
            _flpEvents.SizeChanged += (s, e) =>
            {
                foreach (Control c in _flpEvents.Controls)
                {
                    c.Width = _flpEvents.ClientSize.Width - 16;
                }
            };
            this.Controls.Add(_flpEvents);

            // 3. 새 일정 추가 컨테이너 패널
            Panel pnlAdd = new Panel
            {
                Location = new Point(20, 295),
                Size = new Size(365, 250),
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(15)
            };
            pnlAdd.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    Rectangle rect = new Rectangle(0, 0, pnlAdd.Width - 1, pnlAdd.Height - 1);
                    GdiHelper.DrawRoundedRectangle(g, p, rect, 8);
                }
            };

            Label lblAddTitle = new Label
            {
                Text = "새 일정 추가",
                ForeColor = AppConfig.ColorTextLight,
                Font = AppConfig.FontBodyBold,
                Location = new Point(15, 12),
                AutoSize = true
            };

            _txtEventTitle = new TextBox
            {
                Location = new Point(15, 37),
                Width = 335,
                PlaceholderText = "일정 제목을 입력하세요",
                BackColor = AppConfig.ColorContentBg,
                ForeColor = AppConfig.ColorTextLight,
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtEventDesc = new TextBox
            {
                Location = new Point(15, 72),
                Width = 335,
                Height = 60,
                Multiline = true,
                PlaceholderText = "메모/설명 입력",
                BackColor = AppConfig.ColorContentBg,
                ForeColor = AppConfig.ColorTextLight,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblColor = new Label
            {
                Text = "중요도:",
                ForeColor = AppConfig.ColorTextMuted,
                Location = new Point(15, 147),
                Width = 60,
                AutoSize = true
            };

            _cmbColor = new ComboBox
            {
                Location = new Point(80, 144),
                Width = 270,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = AppConfig.ColorContentBg,
                ForeColor = AppConfig.ColorTextLight
            };
            _cmbColor.Items.Add(new ColorItem("높음 (Red)", AppConfig.ColorPriorityHigh));
            _cmbColor.Items.Add(new ColorItem("보통 (Yellow)", AppConfig.ColorPriorityMedium));
            _cmbColor.Items.Add(new ColorItem("낮음 (Blue)", AppConfig.ColorPriorityLow));
            _cmbColor.SelectedIndex = 1;

            _btnAddEvent = new Button
            {
                Location = new Point(15, 190),
                Width = 335,
                Height = 40,
                Text = "일정 추가",
                FlatStyle = FlatStyle.Flat,
                BackColor = AppConfig.ColorAccent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnAddEvent.FlatAppearance.BorderSize = 0;
            _btnAddEvent.Click += BtnAddEvent_Click;

            // 버튼 마우스 호버 효과
            _btnAddEvent.MouseEnter += (s, e) => _btnAddEvent.BackColor = AppConfig.ColorAccentHover;
            _btnAddEvent.MouseLeave += (s, e) => _btnAddEvent.BackColor = AppConfig.ColorAccent;

            pnlAdd.Controls.Add(lblAddTitle);
            pnlAdd.Controls.Add(_txtEventTitle);
            pnlAdd.Controls.Add(_txtEventDesc);
            pnlAdd.Controls.Add(lblColor);
            pnlAdd.Controls.Add(_cmbColor);
            pnlAdd.Controls.Add(_btnAddEvent);

            this.Controls.Add(pnlAdd);
        }

        // 현재 날짜의 등록된 일정을 로드하고 카드로 추가
        private void LoadEvents()
        {
            _flpEvents.Controls.Clear();
            var dayEvents = _controller.GetEventsForDate(_date);

            if (dayEvents.Count == 0)
            {
                Label lblNoEvent = new Label
                {
                    Text = "등록된 일정이 없습니다.",
                    ForeColor = AppConfig.ColorTextMuted,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(_flpEvents.ClientSize.Width - 16, 80),
                    Font = AppConfig.FontBody
                };
                _flpEvents.Controls.Add(lblNoEvent);
            }
            else
            {
                foreach (var ev in dayEvents)
                {
                    _flpEvents.Controls.Add(CreateEventCard(ev));
                }
            }
        }

        // 아름다운 커스텀 사각형 카드 드로잉 및 내용 렌더링
        private Control CreateEventCard(CalendarEvent ev)
        {
            Panel card = new Panel
            {
                Width = _flpEvents.ClientSize.Width - 16,
                Height = 75,
                BackColor = AppConfig.ColorContentBg,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(10)
            };

            Color accent = ColorTranslator.FromHtml(ev.CategoryColor);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 왼쪽의 두꺼운 컬러 테두리 바 그리기 (중요도 색상 반영)
                using (Brush b = new SolidBrush(accent))
                {
                    g.FillRectangle(b, 0, 0, 6, card.Height);
                }

                // 카드 외곽선 테두리
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    g.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label title = new Label
            {
                Text = ev.Title,
                ForeColor = AppConfig.ColorTextLight,
                Font = AppConfig.FontBodyBold,
                Location = new Point(12, 10),
                Width = card.Width - 55,
                Height = 20,
                UseCompatibleTextRendering = true
            };

            Label desc = new Label
            {
                Text = string.IsNullOrWhiteSpace(ev.Description) ? "(메모 없음)" : ev.Description,
                ForeColor = AppConfig.ColorTextMuted,
                Font = AppConfig.FontSmall,
                Location = new Point(12, 32),
                Width = card.Width - 55,
                Height = 35,
                UseCompatibleTextRendering = true
            };

            // 삭제 버튼 (플랫 X 모양)
            Button btnDel = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppConfig.ColorTextMuted,
                Size = new Size(25, 25),
                Location = new Point(card.Width - 32, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.FlatAppearance.MouseDownBackColor = Color.FromArgb(80, 40, 40);
            btnDel.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 30, 30);
            btnDel.Click += (s, e) =>
            {
                if (MessageBox.Show("이 일정을 삭제하시겠습니까?", "일정 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _controller.DeleteEvent(ev.Id);
                    LoadEvents();
                    _onDataChanged?.Invoke();
                }
            };

            card.Controls.Add(title);
            card.Controls.Add(desc);
            card.Controls.Add(btnDel);

            // 카드 리포지셔닝 시 삭제 버튼 자동 정렬
            card.SizeChanged += (s, e) =>
            {
                btnDel.Left = card.Width - 32;
                title.Width = card.Width - 55;
                desc.Width = card.Width - 55;
            };

            return card;
        }

        // 새 일정 추가 버튼 클릭 핸들러
        private void BtnAddEvent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtEventTitle.Text))
            {
                MessageBox.Show("일정 제목을 입력해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var colorItem = _cmbColor.SelectedItem as ColorItem;
            string hexColor = ColorTranslator.ToHtml(colorItem != null ? colorItem.Color : AppConfig.ColorAccent);

            var newEvent = new CalendarEvent
            {
                EventDate = _date,
                Title = _txtEventTitle.Text.Trim(),
                Description = _txtEventDesc.Text.Trim(),
                CategoryColor = hexColor
            };

            _controller.AddEvent(newEvent);
            
            _txtEventTitle.Clear();
            _txtEventDesc.Clear();
            
            LoadEvents();
            _onDataChanged?.Invoke();
        }

        private class ColorItem
        {
            public string Name { get; }
            public Color Color { get; }
            public ColorItem(string name, Color color) { Name = name; Color = color; }
            public override string ToString() => Name;
        }
    }
}
