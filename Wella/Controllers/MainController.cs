using System.Collections.Generic;
using System.Windows.Forms;
using Wella.Common;
using Wella.Views;

namespace Wella.Controllers
{
    public class MainController
    {
        private readonly MainForm _mainForm;
        private readonly StorageManager _storageManager;
        private readonly List<IToolModule> _modules = new List<IToolModule>();
        private IToolModule _activeModule;

        public MainController(MainForm mainForm)
        {
            _mainForm = mainForm;
            _storageManager = new StorageManager();
            
            // 공통 데이터 폴더 및 초기 상태 보장
            AppConfig.EnsureDataDirectoryExists();
        }

        public void RegisterModule(IToolModule module)
        {
            module.Initialize(_storageManager);
            _modules.Add(module);
        }

        public List<IToolModule> GetModules() => _modules;

        public void Start()
        {
            // 등록된 모듈 목록을 메인 폼에 알림
            _mainForm.InitializeModules(this);
            
            // 첫 번째 모듈(일반적으로 달력)을 기본 활성화
            if (_modules.Count > 0)
            {
                SwitchToModule(_modules[0]);
            }
        }

        public void SwitchToModule(IToolModule module)
        {
            if (module == null) return;
            
            _activeModule = module;
            UserControl view = module.GetView();
            
            _mainForm.ShowModuleView(view, module.ToolName, module.ToolDescription);
        }
    }
}
