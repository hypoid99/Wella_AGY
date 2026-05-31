using System;

namespace Wella.Models
{
    public class CalendarEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime EventDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryColor { get; set; } = "#6C5CE7"; // 기본 테마 색상 (Hex 포맷)
        public bool IsAlarmEnabled { get; set; } = false;
    }
}
