using System.Windows.Forms;

namespace Wella.Common
{
    public interface IToolModule
    {
        string ToolName { get; }
        string ToolDescription { get; }
        
        // 메인 쉘 상단 또는 사이드바에 표시될 버튼을 렌더링하기 위한 모듈 아이콘
        // (필요 시 직접 그리거나 문자로 대체 가능하도록 지원)
        string ToolIconChar { get; } 

        UserControl GetView();
        void Initialize(StorageManager storageManager);
    }
}
