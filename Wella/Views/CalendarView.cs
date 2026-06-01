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
        
        // 우측 관리 패널용 컨트롤들 (제거됨)
        
        public CalendarView()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true; // 창 크기 변경 시 전체 영역을 즉시 갱신하도록 설정
            this.BackColor = AppConfig.ColorBgDark;
            this.Font = AppConfig.FontBody;
            this.Size = new Size(900, 600);
            
            _currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _selectedDate = DateTime.Today;
        }

        public void Initialize(CalendarController controller)
        {
            _controller = controller;
            RefreshCalendar();
        }

        // 현재 상태를 기반으로 UI 리프레시
        public void RefreshCalendar()
        {
            if (_controller == null) return;
            this.Invalidate();
        }

        // GDI+ 달력 페인팅 메인 로직
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int calendarWidth = this.Width;
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

                    // 5. 날짜 텍스트 및 공휴일 렌더링
                    string dayText = cellDate.Day.ToString();
                    string holidayName = HolidayHelper.GetHolidayName(cellDate);
                    bool isHoliday = !string.IsNullOrEmpty(holidayName);

                    Color numColor;
                    if (isSelected)
                    {
                        numColor = Color.White;
                    }
                    else if (isHoliday)
                    {
                        numColor = AppConfig.ColorPriorityHigh; // 공휴일은 붉은색 (Pastel Red)
                    }
                    else
                    {
                        numColor = isCurrentMonth ? AppConfig.ColorTextLight : AppConfig.ColorTextMuted;
                    }

                    using (Brush numBrush = new SolidBrush(numColor))
                    {
                        g.DrawString(dayText, AppConfig.FontBodyBold, numBrush, cellRect.X + 8, cellRect.Y + 8);

                        // 공휴일 명칭 그리기
                        if (isHoliday)
                        {
                            SizeF numSize = g.MeasureString(dayText, AppConfig.FontBodyBold);
                            Color holidayTextColor = isSelected ? Color.White : AppConfig.ColorPriorityHigh;
                            using (Brush holidayBrush = new SolidBrush(holidayTextColor))
                            {
                                g.DrawString(holidayName, AppConfig.FontSmall, holidayBrush, cellRect.X + 8 + numSize.Width + 4, cellRect.Y + 10);
                            }
                        }
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

                    // 다이얼로그 팝업 호출
                    using (var dlg = new EventDialog(_controller, clickedDate, () => RefreshCalendar()))
                    {
                        dlg.ShowDialog(this);
                    }

                    break;
                }
            }
        }
    }
}
