namespace Wella;

using Wella.Controllers;
using Wella.Views;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        // 메인 폼과 컨트롤러 구성
        MainForm mainForm = new MainForm();
        MainController controller = new MainController(mainForm);

        // 1단계: 달력 모듈 등록
        controller.RegisterModule(new CalendarModule());

        // 내비게이션 탭 빌드 및 초기 뷰 활성화
        controller.Start();

        Application.Run(mainForm);
    }    
}