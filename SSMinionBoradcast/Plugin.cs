using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.Automation;
using ECommons.DalamudServices;
using SSMinionBoradcast.Windows;

namespace SSMinionBoradcast
{
    public sealed class SSMinionBoradcast : IDalamudPlugin
    {
        public string Name => "SSMinionBoradcast";
        public static SSMinionBoradcast Plugin;
        public Configuration Configuration;
        public WindowSystem WindowSystem = new("SSMinionBoradcast");

        private ConfigWindow ConfigWindow;
        private MainWindow MainWindow;
        public TaskManager TaskManager;
        public CoordsToMapLink CoordsToMapLink;
        public Boradcast Boradcast;
        public SSMinionBoradcast(DalamudPluginInterface pluginInterface)
        {
            Plugin = this;
            ECommonsMain.Init(pluginInterface, this);
            Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(Svc.PluginInterface);

            TaskManager = new();

            ConfigWindow = new();
            MainWindow = new();
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            Boradcast = new();
            CoordsToMapLink = new();
            CoordsToMapLink.Enable();

            Svc.PluginInterface.UiBuilder.Draw += DrawUI;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= DrawUI;
            Svc.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

            TaskManager.Abort();
            WindowSystem.RemoveAllWindows();
            MainWindow.Dispose();

            Boradcast.Dispose();
            CoordsToMapLink.Dispose();

            ECommonsMain.Dispose();
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
