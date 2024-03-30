using Dalamud.Game.Text;
using Dalamud.Interface.Internal.Notifications;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSMinionBoradcast
{
    public static class Boradcast
    {
        public static unsafe void ProcessData()
        {
            Svc.Log.Info("ProcessData...");
            Data.currMacro.Clear();
            if (Data.currSSMinionList.Count != 0)
            {
                var mapName = Svc.Data.GetExcelSheet<TerritoryType>()!.GetRow(Svc.ClientState.TerritoryType)!.PlaceName.Value!.Name.RawString;
                var instance = GetCharacterForInstanceNumber(UIState.Instance()->AreaInstance.Instance);
                var waypoint = new Dictionary<string, string>
        {
            {"<flag1>", $"{mapName}{instance} ( {Data.currSSMinionList[0].X:F1}  , {Data.currSSMinionList[0].Y:F1} )"},
            {"<flag2>", $"{mapName}{instance} ( {Data.currSSMinionList[1].X:F1}  , {Data.currSSMinionList[1].Y:F1} )"},
            {"<flag3>", $"{mapName}{instance} ( {Data.currSSMinionList[2].X:F1}  , {Data.currSSMinionList[2].Y:F1} )"},
            {"<flag4>", $"{mapName}{instance} ( {Data.currSSMinionList[3].X:F1}  , {Data.currSSMinionList[3].Y:F1} )"},
        };
                foreach (var macro in Plugin.Configuration.Macro)
                {
                    Data.currMacro.Add(ProcessMacro(macro, waypoint));
                }

                Svc.Log.Info("SendMessage...");
                SendMessage(Data.currMacro);
            }
        }

        private static string GetCharacterForInstanceNumber(int instance)
        {
            if (instance == 0)
                return string.Empty;
            return $"{((SeIconChar)((int)SeIconChar.Instance1 + (instance - 1))).ToIconChar()}";
        }

        private static string ProcessMacro(string macro, Dictionary<string, string> waypoint)
        {
            var pattern = string.Join("|", waypoint.Keys.Select(Regex.Escape));
            var regex = new Regex(pattern);
            var result = regex.Replace(macro, match => waypoint[match.Value]);
            return result;
        }

        public static void SendMessage(List<string> macro)
        {
            Chat.Instance.SendMessage("/mcancel");

            macro.Insert(0, "/mlock");
            MacroManager.Execute(macro);

            Svc.Log.Info("ExecuteMacro...");
            Svc.PluginInterface.UiBuilder.AddNotification("开始发送喊话宏", "SSMinionBoradcast", NotificationType.Success);
        }
    }
}
