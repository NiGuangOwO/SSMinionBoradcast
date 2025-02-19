using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSMinionBoradcast
{
    public static class Boradcast
    {
        public static unsafe void ProcessData(bool send)
        {
            Data.currMacro.Clear();
            if (Data.SSMinion.TryGetValue(GameMain.Instance()->CurrentTerritoryTypeId, out var ssminionlist))
            {
                var mapName = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(GameMain.Instance()->CurrentTerritoryTypeId).PlaceName.Value.Name.ExtractText();
                var instance = GetCharacterForInstanceNumber(UIState.Instance()->PublicInstance.InstanceId);
                var waypoint = new Dictionary<string, string>
        {
            {"<flag1>", $"{mapName}{instance} ( {ssminionlist[0].X:F1}  , {ssminionlist[0].Y:F1} )"},
            {"<flag2>", $"{mapName}{instance} ( {ssminionlist[1].X:F1}  , {ssminionlist[1].Y:F1} )"},
            {"<flag3>", $"{mapName}{instance} ( {ssminionlist[2].X:F1}  , {ssminionlist[2].Y:F1} )"},
            {"<flag4>", $"{mapName}{instance} ( {ssminionlist[3].X:F1}  , {ssminionlist[3].Y:F1} )"},
        };
                foreach (var macro in Plugin.Configuration.Macro)
                {
                    Data.currMacro.Add(ProcessMacro(macro, waypoint));
                }

                if (send)
                {
                    SendMessage(Data.currMacro);
                }
            }
            else
            {
                Svc.Log.Error("获取当前地图SS小怪点位失败！");
            }
        }

        private static string GetCharacterForInstanceNumber(uint instance)
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
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast",
                Content = "开始发送喊话宏",
                Type = NotificationType.Success
            });
        }
    }
}
