using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSMinionBoradcast
{
    public partial class Boradcast : IDisposable
    {
        public Boradcast()
        {
            Svc.Chat.ChatMessage += ChatGui_OnChatMessage;
        }

        public static void ChatGui_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (type == XivChatType.Echo && message.TextValue == "特殊恶名精英的手下开始了侦察活动......")
            {
                ProcessData();
            }
        }

        public static unsafe void ProcessData(bool manual = false)
        {
            Data.currSSMinionList.Clear();
            Data.currMacro.Clear();

            if (Data.SSMinion.TryGetValue(Svc.ClientState.TerritoryType, out var ssminionlist))
            {
                Data.currSSMinionList = ssminionlist;
                if (Data.currSSMinionList != null && Data.currSSMinionList.Count == 4)
                {
                    var mapName = Svc.Data.GetExcelSheet<TerritoryType>()!.GetRow(Svc.ClientState.TerritoryType)!.PlaceName.Value!.Name.RawString;
                    var instance = GetCharacterForInstanceNumber(UIState.Instance()->AreaInstance.Instance);
                    var waypoint = new Dictionary<string, string>
        {
            {"<flag1>", $"{mapName}{instance} ( {ssminionlist[0].Item1}  , {ssminionlist[0].Item2} )"},
            {"<flag2>", $"{mapName}{instance} ( {ssminionlist[1].Item1}  , {ssminionlist[1].Item2} )"},
            {"<flag3>", $"{mapName}{instance} ( {ssminionlist[2].Item1}  , {ssminionlist[2].Item2} )"},
            {"<flag4>", $"{mapName}{instance} ( {ssminionlist[3].Item1}  , {ssminionlist[3].Item2} )"},
        };
                    foreach (var macro in Plugin.Configuration.Macro)
                    {
                        Data.currMacro.Add(ProcessMacro(macro, waypoint));
                    }
                    if (Plugin.Configuration.AutoBoradcast || manual)
                    {
                        SendMessage(Data.currMacro);
                    }
                }
            }
        }

        public static async void SendMessage(List<string> macro)
        {
            if (Data.isBoradcasting)
                return;
            Data.isBoradcasting = true;
            foreach (var item in macro)
            {
                Plugin.TaskManager.Enqueue(() => Chat.Instance.SendMessage(item));
                if (item != macro.Last())
                {
                    Plugin.TaskManager.DelayNext(2500);
                }
                else
                {
                    Plugin.TaskManager.Enqueue(() => { Data.isBoradcasting = false; });
                }
            }
        }

        private static string GetCharacterForInstanceNumber(int instance)
        {
            if (instance == 0)
                return string.Empty;
            return $" {((SeIconChar)((int)SeIconChar.Instance1 + (instance - 1))).ToIconChar()}";
        }

        private static string ProcessMacro(string macro, Dictionary<string, string> waypoint)
        {
            var pattern = string.Join("|", waypoint.Keys.Select(Regex.Escape));
            var regex = new Regex(pattern);
            var result = regex.Replace(macro, match => waypoint[match.Value]);
            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Svc.Chat.ChatMessage -= ChatGui_OnChatMessage;
        }
    }
}
