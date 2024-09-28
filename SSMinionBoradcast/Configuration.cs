using Dalamud.Configuration;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;

namespace SSMinionBoradcast
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool AutoBoradcast = false;
        public List<string> Macro = [];

        public void Save()
        {
            Svc.PluginInterface.SavePluginConfig(this);
        }
    }
}
