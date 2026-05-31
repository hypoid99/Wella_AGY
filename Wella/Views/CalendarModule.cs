using System.Windows.Forms;
using Wella.Common;
using Wella.Controllers;

namespace Wella.Views
{
    public class CalendarModule : IToolModule
    {
        private CalendarView _view;
        private CalendarController _controller;

        public string ToolName => "달력";
        public string ToolDescription => "월별 일정 및 스케줄 관리";
        public string ToolIconChar => "📅"; // 이모지/아이콘 대체 유니코드 문자

        public UserControl GetView()
        {
            return _view;
        }

        public void Initialize(StorageManager storageManager)
        {
            _controller = new CalendarController(storageManager);
            _view = new CalendarView();
            _view.Initialize(_controller);
        }
    }
}
