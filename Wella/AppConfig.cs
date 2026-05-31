using System.Drawing;
using System.IO;

namespace Wella
{
    public static class AppConfig
    {
        // 글로벌 색상 정의 (Sleek Dark & Pastel Accent 테마)
        public static readonly Color ColorBgDark = Color.FromArgb(20, 20, 26);         // 메인 백그라운드 어두운색
        public static readonly Color ColorPanelBg = Color.FromArgb(28, 28, 36);         // 사이드바/상단바 패널색
        public static readonly Color ColorContentBg = Color.FromArgb(32, 32, 42);       // 메인 콘텐츠 영역 백그라운드
        public static readonly Color ColorAccent = Color.FromArgb(108, 92, 231);       // 포인트 퍼플 색상
        public static readonly Color ColorAccentHover = Color.FromArgb(130, 115, 240);  // 포인트 퍼플 호버 색상
        public static readonly Color ColorTextLight = Color.FromArgb(240, 240, 245);    // 밝은 텍스트
        public static readonly Color ColorTextMuted = Color.FromArgb(140, 140, 155);    // 흐린 텍스트
        public static readonly Color ColorBorder = Color.FromArgb(48, 48, 60);          // 보더 라인 색상
        
        // 중요도별 색상 (달력/할일 등에 사용)
        public static readonly Color ColorPriorityHigh = Color.FromArgb(255, 121, 121); // 높은 우선순위 (Pastel Red)
        public static readonly Color ColorPriorityMedium = Color.FromArgb(249, 202, 36); // 중간 우선순위 (Pastel Yellow)
        public static readonly Color ColorPriorityLow = Color.FromArgb(104, 109, 224);  // 낮은 우선순위 (Pastel Blue)

        // 글로벌 폰트 정의
        public static readonly Font FontTitle = new Font("Malgun Gothic", 16F, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Malgun Gothic", 12F, FontStyle.Bold);
        public static readonly Font FontBody = new Font("Malgun Gothic", 9.5F, FontStyle.Regular);
        public static readonly Font FontBodyBold = new Font("Malgun Gothic", 9.5F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Malgun Gothic", 8F, FontStyle.Regular);

        // 데이터 파일 저장 기본 디렉토리
        public static string DataDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        
        // 각 도구별 데이터 파일명 정의
        public static string CalendarDataPath => Path.Combine(DataDirectory, "calendar_data.txt");
        public static string TodoDataPath => Path.Combine(DataDirectory, "todo_data.txt");
        public static string MemoDataPath => Path.Combine(DataDirectory, "memo_data.txt");
        public static string CalculatorDataPath => Path.Combine(DataDirectory, "calculator_history.txt");

        public static void EnsureDataDirectoryExists()
        {
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
        }
    }
}
