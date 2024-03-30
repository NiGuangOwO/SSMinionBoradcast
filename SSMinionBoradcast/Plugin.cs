using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ECommons;
using ECommons.DalamudServices;
using SSMinionBoradcast.Windows;

namespace SSMinionBoradcast
{
    public sealed class SSMinionBoradcast : IDalamudPlugin
    {
        public static string Name => "SSMinionBoradcast";
        public static SSMinionBoradcast Plugin;
        public Configuration Configuration;
        public WindowSystem WindowSystem = new("SSMinionBoradcast");

        private ConfigWindow ConfigWindow;
        internal MainWindow MainWindow;
        public CoordsToMapLink CoordsToMapLink;

        public SSMinionBoradcast(DalamudPluginInterface pluginInterface)
        {
            Plugin = this;
            ECommonsMain.Init(pluginInterface, this);
            Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(Svc.PluginInterface);
            ConfigWindow = new();
            MainWindow = new();
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            CoordsToMapLink = new();
            CoordsToMapLink.Enable();
            Events.Enable();
            Svc.PluginInterface.UiBuilder.Draw += DrawUI;
            Svc.PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;
            Svc.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            if (Configuration.Macro.Count == 0)
            {
                ConfigWindow.AddTemplateMacro();
            }
        }

        public void Dispose()
        {
            Svc.PluginInterface.UiBuilder.Draw -= DrawUI;
            Svc.PluginInterface.UiBuilder.OpenMainUi -= DrawConfigUI;
            Svc.PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
            WindowSystem.RemoveAllWindows();
            CoordsToMapLink.Dispose();
            Events.Disable();
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
