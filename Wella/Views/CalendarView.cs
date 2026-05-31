using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Wella.Common;
using Wella.Controllers;
using Wella.Models;

namespace Wella.Views
{
    public class CalendarView : UserControl
    {
        private CalendarController _controller;
        
        // 상태 필드
        private DateTime _currentMonthStart;
        private DateTime _selectedDate;
        private int _hoveredCellIndex = -1; // -1: 없음, 0~41: 날짜 셀
        
        // 렌더링용 영역 계산 정보
        private Rectangle _btnPrevRect;
        private Rectangle _btnNextRect;
        private readonly List<Rectangle> _dayCellRects = new List<Rectangle>();
        
        // 우측 관리 패널용 컨트롤들
        private Panel _pnlRightSide;
        private Label _lblSelectedDate;
        private FlowLayoutPanel _flpEvents;
        private Panel _pnlAddEvent;
        private TextBox _txtEventTitle;
        private TextBox _txtEventDesc;
        private ComboBox _cmbColor;
        private Button _btnAddEvent;
        
        public CalendarView()
        {
            this.DoubleBuffered = true;
            this.BackColor = AppConfig.ColorBgDark;
            this.Font = AppConfig.FontBody;
            this.Size = new Size(900, 600);
            
            _currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _selectedDate = DateTime.Today;

            InitializeRightPanel();
        }

        public void Initialize(CalendarController controller)
        {
            _controller = controller;
            RefreshCalendar();
        }

        private void InitializeRightPanel()
        {
            // 우측 패널 (320px 넓이, 일정 상세 및 추가 기능)
            _pnlRightSide = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(15)
            };
            
            // 테두리 구분선 그리기 위한 패널 페인트 이벤트
            _pnlRightSide.Paint += (s, e) =>
            {
                using (Pen p = new Pen(AppConfig.ColorBorder, 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, 0, _pnlRightSide.Height);
                }
            };

            // 선택 날짜 헤더
            _lblSelectedDate = new Label
            {
                Dock = DockStyle.Top,
                Height = 35,
                ForeColor = AppConfig.ColorTextLight,
                Font = AppConfig.FontSubtitle,
                Text = "2026-05-31 일정",
                TextAlign = ContentAlignment.MiddleLeft
            };

            // 일정 스크롤 영역
            _flpEvents = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 10)
            };
            // FlowLayoutPanel 스크롤 바 너비 고려하여 자식 컨트롤 가로 핏 맞추기
            _flpEvents.SizeChanged += (s, e) => {
                foreach (Control c in _flpEvents.Controls)
                    c.Width = _flpEvents.Width - 25;
            };

            // 일정 추가 패널 (하단 고정)
            _pnlAddEvent = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 240,
                BackColor = AppConfig.ColorPanelBg,
                Padding = new Padding(5, 10, 5, 0)
            };

            Label lblAddTitle = new Label
            {
                Text = "새 일정 추가",
                ForeColor = AppConfig.ColorTextLight,
                Font = AppConfig.FontBodyBold,
                Location = new Point(5, 5),
                AutoSize = true
            };

            _txtEventTitle = new TextBox
            {
                Location = new Point(5, 30),
                Width = 280,
                PlaceholderText = "일정 제목을 입력하세요",
                BackColor = AppConfig.ColorContentBg,
                ForeColor = AppConfig.ColorTextLight,
                BorderStyle = BorderStyle.FixedSingle
            };

            _txtEventDesc = new TextBox
            {
                Location = new Point(5, 65),
                Width = 280,
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
                Location = new Point(5, 138),
                Width = 60,
                AutoSize = true
            };

            _cmbColor = new ComboBox
            {
                Location = new Point(70, 135),
                Width = 215,
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
                Location = new Point(5, 180),
                Width = 280,
                Height = 40,
                Text = "일정 추가",
                FlatStyle = FlatStyle.Flat,
                BackColor = AppConfig.ColorAccent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnAddEvent.FlatAppearance.BorderSize = 0;
            _btnAddEvent.Click += BtnAddEvent_Click;

            // 호버 시 색상 전환 효과
            _btnAddEvent.MouseEnter += (s, e) => _btnAddEvent.BackColor = AppConfig.ColorAccentHover;
            _btnAddEvent.MouseLeave += (s, e) => _btnAddEvent.BackColor = AppConfig.ColorAccent;

            _pnlAddEvent.Controls.Add(lblAddTitle);
            _pnlAddEvent.Controls.Add(_txtEventTitle);
            _pnlAddEvent.Controls.Add(_txtEventDesc);
            _pnlAddEvent.Controls.Add(lblColor);
            _pnlAddEvent.Controls.Add(_cmbColor);
            _pnlAddEvent.Controls.Add(_btnAddEvent);

            _pnlRightSide.Controls.Add(_flpEvents);
            _pnlRightSide.Controls.Add(_lblSelectedDate);
            _pnlRightSide.Controls.Add(_pnlAddEvent);

            this.Controls.Add(_pnlRightSide);
        }

        private class ColorItem
        {
            public string Name { get; }
            public Color Color { get; }
            public ColorItem(string name, Color color) { Name = name; Color = color; }
            public override string ToString() => Name;
        }

        // 새 일정 등록 처리
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
                EventDate = _selectedDate,
                Title = _txtEventTitle.Text.Trim(),
                Description = _txtEventDesc.Text.Trim(),
                CategoryColor = hexColor
            };

            _controller.AddEvent(newEvent);
            
            _txtEventTitle.Clear();
            _txtEventDesc.Clear();
            
            RefreshCalendar();
        }

        // 현재 상태를 기반으로 UI 리프레시
        public void RefreshCalendar()
        {
            if (_controller == null) return;

            // 1. 헤더 날짜 업데이트
            _lblSelectedDate.Text = $"{_selectedDate:yyyy년 MM월 dd일} 일정";

            // 2. 우측 일정 카드 리스트 로드
            _flpEvents.Controls.Clear();
            var dayEvents = _controller.GetEventsForDate(_selectedDate);

            if (dayEvents.Count == 0)
            {
                Label lblNoEvent = new Label
                {
                    Text = "등록된 일정이 없습니다.",
                    ForeColor = AppConfig.ColorTextMuted,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(_flpEvents.Width - 30, 80),
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

            // 3. 달력 화면 다시 그리기 유도
            this.Invalidate();
        }

        // 아름다운 GDI+ 카드 형태로 일정 표시
        private Control CreateEventCard(CalendarEvent ev)
        {
            Panel card = new Panel
            {
                Width = _flpEvents.Width - 25,
                Height = 85,
                BackColor = AppConfig.ColorContentBg,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(10)
            };

            Color accent = ColorTranslator.FromHtml(ev.CategoryColor);

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // 왼쪽의 두꺼운 컬러 테두리바 그리기
                using (Brush b = new SolidBrush(accent))
                {
                    g.FillRectangle(b, 0, 0, 6, card.Height);
                }

                // 카드 외곽 둥근 선
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
                Height = 45,
                UseCompatibleTextRendering = true
            };

            // 삭제 버튼 (플랫 X)
            Button btnDel = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = AppConfig.ColorTextMuted,
                Size = new Size(25, 25),
                Location = new Point(card.Width - 35, 10),
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
                    RefreshCalendar();
                }
            };

            card.Controls.Add(title);
            card.Controls.Add(desc);
            card.Controls.Add(btnDel);

            return card;
        }

        // GDI+ 달력 페인팅 메인 로직
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int calendarWidth = this.Width - _pnlRightSide.Width;
            int calendarHeight = this.Height;

            // 1. 달력 상단 타이틀 영역 렌더링
            string titleText = $"{_currentMonthStart:yyyy년 MM월}";
            SizeF titleSize = g.MeasureString(titleText, AppConfig.FontTitle);
            
            // 중앙 부근에 제목 출력
            int headerY = 30;
            float titleX = (calendarWidth - titleSize.Width) / 2;
            g.DrawString(titleText, AppConfig.FontTitle, new SolidBrush(AppConfig.ColorTextLight), titleX, headerY);

            // 이전달, 다음달 GDI+ 아이콘/버튼 드로잉 영역 지정
            int btnSize = 32;
            _btnPrevRect = new Rectangle((int)titleX - 60, headerY - 2, btnSize, btnSize);
            _btnNextRect = new Rectangle((int)(titleX + titleSize.Width) + 28, headerY - 2, btnSize, btnSize);

            // 이전달 버튼 그리기
            using (Brush b = new SolidBrush(AppConfig.ColorPanelBg))
            {
                g.FillEllipse(b, _btnPrevRect);
                g.FillEllipse(b, _btnNextRect);
            }
            using (Pen borderPen = new Pen(AppConfig.ColorBorder, 1))
            {
                g.DrawEllipse(borderPen, _btnPrevRect);
                g.DrawEllipse(borderPen, _btnNextRect);
            }
            
            // 화살표 그리기
            using (Brush arrowBrush = new SolidBrush(AppConfig.ColorTextLight))
            {
                g.DrawString("◀", AppConfig.FontSmall, arrowBrush, _btnPrevRect.X + 9, _btnPrevRect.Y + 9);
                g.DrawString("▶", AppConfig.FontSmall, arrowBrush, _btnNextRect.X + 11, _btnNextRect.Y + 9);
            }

            // 2. 요일 헤더 그리기
            string[] daysOfWeek = { "일", "월", "화", "수", "목", "금", "토" };
            int startY = 90;
            int cellWidth = (calendarWidth - 40) / 7;
            int cellHeight = (calendarHeight - startY - 30) / 6;

            for (int i = 0; i < 7; i++)
            {
                Color dayColor = AppConfig.ColorTextMuted;
                if (i == 0) dayColor = AppConfig.ColorPriorityHigh; // 일요일은 빨간빛
                if (i == 6) dayColor = AppConfig.ColorPriorityLow;  // 토요일은 파란빛

                int x = 20 + i * cellWidth;
                Rectangle headerRect = new Rectangle(x, startY, cellWidth, 30);

                using (Brush b = new SolidBrush(dayColor))
                {
                    SizeF size = g.MeasureString(daysOfWeek[i], AppConfig.FontBodyBold);
                    g.DrawString(daysOfWeek[i], AppConfig.FontBodyBold, b, x + (cellWidth - size.Width) / 2, startY + (30 - size.Height) / 2);
                }
            }

            // 3. 날짜 격자(Grid) 영역 및 날짜 그리기
            _dayCellRects.Clear();

            int startDayOfWeek = (int)_currentMonthStart.DayOfWeek;
            int daysInMonth = DateTime.DaysInMonth(_currentMonthStart.Year, _currentMonthStart.Month);
            DateTime prevMonth = _currentMonthStart.AddMonths(-1);
            int prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

            int currentDay = 1;
            int nextMonthDay = 1;

            int gridStartY = startY + 35;

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    int x = 20 + col * cellWidth;
                    int y = gridStartY + row * cellHeight;
                    Rectangle cellRect = new Rectangle(x, y, cellWidth - 5, cellHeight - 5);
                    _dayCellRects.Add(cellRect);

                    int index = row * 7 + col;
                    DateTime cellDate;
                    bool isCurrentMonth = true;

                    if (index < startDayOfWeek)
                    {
                        // 이전달 날짜
                        int d = prevMonthDays - startDayOfWeek + index + 1;
                        cellDate = new DateTime(prevMonth.Year, prevMonth.Month, d);
                        isCurrentMonth = false;
                    }
                    else if (currentDay <= daysInMonth)
                    {
                        // 현재달 날짜
                        cellDate = new DateTime(_currentMonthStart.Year, _currentMonthStart.Month, currentDay);
                        currentDay++;
                    }
                    else
                    {
                        // 다음달 날짜
                        DateTime nextM = _currentMonthStart.AddMonths(1);
                        cellDate = new DateTime(nextM.Year, nextM.Month, nextMonthDay);
                        nextMonthDay++;
                        isCurrentMonth = false;
                    }

                    // 4. 셀 배경 렌더링
                    bool isSelected = (cellDate.Date == _selectedDate.Date);
                    bool isHovered = (index == _hoveredCellIndex);
                    bool isToday = (cellDate.Date == DateTime.Today);

                    if (isSelected)
                    {
                        // 선택된 날짜 배경 (Accent Purple)
                        using (Brush b = new SolidBrush(AppConfig.ColorAccent))
                        {
                            GdiHelper.FillRoundedRectangle(g, b, cellRect, 8);
                        }
                    }
                    else if (isHovered)
                    {
                        // 마우스 호버 배경 (Soft grey)
                        using (Brush b = new SolidBrush(Color.FromArgb(48, 48, 60)))
                        {
                            GdiHelper.FillRoundedRectangle(g, b, cellRect, 8);
                        }
                    }
                    else if (isToday)
                    {
                        // 오늘 날짜 테두리 강조
                        using (Pen p = new Pen(AppConfig.ColorAccent, 1.5F))
                        {
                            GdiHelper.DrawRoundedRectangle(g, p, cellRect, 8);
                        }
                    }
                    else
                    {
                        // 일반 배경
                        using (Brush b = new SolidBrush(AppConfig.ColorContentBg))
                        {
                            GdiHelper.FillRoundedRectangle(g, b, cellRect, 8);
                        }
                    }

                    // 5. 날짜 텍스트 렌더링
                    string dayText = cellDate.Day.ToString();
                    Color numColor = isCurrentMonth ? AppConfig.ColorTextLight : AppConfig.ColorTextMuted;
                    if (isSelected) numColor = Color.White;

                    using (Brush numBrush = new SolidBrush(numColor))
                    {
                        g.DrawString(dayText, AppConfig.FontBodyBold, numBrush, cellRect.X + 8, cellRect.Y + 8);
                    }

                    // 6. 일정이 존재할 경우 하단 표시 렌더링
                    if (_controller != null)
                    {
                        var events = _controller.GetEventsForDate(cellDate);
                        if (events.Count > 0)
                        {
                            int dotSize = 6;
                            int dotSpacing = 4;
                            int totalWidth = events.Count * dotSize + (events.Count - 1) * dotSpacing;
                            int startDotX = cellRect.X + (cellRect.Width - totalWidth) / 2;
                            int dotY = cellRect.Bottom - 12;

                            // 최대 4개까지만 점으로 표시
                            int renderCount = Math.Min(events.Count, 4);
                            for (int d = 0; d < renderCount; d++)
                            {
                                Color dotColor = ColorTranslator.FromHtml(events[d].CategoryColor);
                                if (isSelected) dotColor = Color.White; // 선택 시 가독성을 위해 흰색으로 통합

                                using (Brush b = new SolidBrush(dotColor))
                                {
                                    g.FillEllipse(b, startDotX + d * (dotSize + dotSpacing), dotY, dotSize, dotSize);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 호버 효과 감지
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int newHoveredIndex = -1;

            for (int i = 0; i < _dayCellRects.Count; i++)
            {
                if (_dayCellRects[i].Contains(e.Location))
                {
                    newHoveredIndex = i;
                    break;
                }
            }

            // 상단 이동 버튼 호버 여부에 따라 마우스 커서 손가락 모양으로 변경
            if (_btnPrevRect.Contains(e.Location) || _btnNextRect.Contains(e.Location))
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            if (newHoveredIndex != _hoveredCellIndex)
            {
                _hoveredCellIndex = newHoveredIndex;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoveredCellIndex = -1;
            this.Invalidate();
        }

        // 클릭 이벤트 처리
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // 1. 이전달 버튼 클릭
            if (_btnPrevRect.Contains(e.Location))
            {
                _currentMonthStart = _currentMonthStart.AddMonths(-1);
                _selectedDate = new DateTime(_currentMonthStart.Year, _currentMonthStart.Month, 1);
                RefreshCalendar();
                return;
            }

            // 2. 다음달 버튼 클릭
            if (_btnNextRect.Contains(e.Location))
            {
                _currentMonthStart = _currentMonthStart.AddMonths(1);
                _selectedDate = new DateTime(_currentMonthStart.Year, _currentMonthStart.Month, 1);
                RefreshCalendar();
                return;
            }

            // 3. 날짜 격자 셀 클릭
            for (int i = 0; i < _dayCellRects.Count; i++)
            {
                if (_dayCellRects[i].Contains(e.Location))
                {
                    // 클릭된 날짜 계산
                    int startDayOfWeek = (int)_currentMonthStart.DayOfWeek;
                    DateTime prevMonth = _currentMonthStart.AddMonths(-1);
                    int prevMonthDays = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

                    DateTime clickedDate;
                    if (i < startDayOfWeek)
                    {
                        int d = prevMonthDays - startDayOfWeek + i + 1;
                        clickedDate = new DateTime(prevMonth.Year, prevMonth.Month, d);
                    }
                    else if (i - startDayOfWeek < DateTime.DaysInMonth(_currentMonthStart.Year, _currentMonthStart.Month))
                    {
                        clickedDate = new DateTime(_currentMonthStart.Year, _currentMonthStart.Month, i - startDayOfWeek + 1);
                    }
                    else
                    {
                        DateTime nextMonth = _currentMonthStart.AddMonths(1);
                        int d = i - startDayOfWeek - DateTime.DaysInMonth(_currentMonthStart.Year, _currentMonthStart.Month) + 1;
                        clickedDate = new DateTime(nextMonth.Year, nextMonth.Month, d);
                    }

                    _selectedDate = clickedDate;

                    // 월 이동 필요 시 자동으로 맞춰주기
                    if (clickedDate.Month != _currentMonthStart.Month || clickedDate.Year != _currentMonthStart.Year)
                    {
                        _currentMonthStart = new DateTime(clickedDate.Year, clickedDate.Month, 1);
                    }

                    RefreshCalendar();
                    break;
                }
            }
        }
    }
}
