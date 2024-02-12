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
            Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
            if (Svc.ClientState.LocalPlayer is not null)
            {
                if (Data.SSMinion.TryGetValue(Svc.ClientState.TerritoryType, out var ssminionlist))
                {
                    Data.currSSMinionList = ssminionlist;
                }
            }
        }

        private void ClientState_TerritoryChanged(object? sender, ushort e)
        {
            if (Data.SSMinion.TryGetValue(Svc.ClientState.TerritoryType, out var ssminionlist))
            {
                Data.currSSMinionList = ssminionlist;
            }
            else
            {
                Data.currSSMinionList.Clear();
            }
        }

        public static void ChatGui_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if ((int)type == 2105 && message.TextValue.Contains("特殊恶名精英的手下开始了侦察活动"))
            {
                if (Plugin.Configuration.AutoBoradcast)
                {
                    ProcessData();
                }
            }
            //if (type == XivChatType.Echo && message.TextValue == "test")
            //{
            //    ProcessData(true);
            //}
        }

        public static unsafe void ProcessData()
        {
            Data.currMacro.Clear();
            if (Data.currSSMinionList != null && Data.currSSMinionList.Count != 0)
            {
                var mapName = Svc.Data.GetExcelSheet<TerritoryType>()!.GetRow(Svc.ClientState.TerritoryType)!.PlaceName.Value!.Name.RawString;
                var instance = GetCharacterForInstanceNumber(UIState.Instance()->AreaInstance.Instance);
                var waypoint = new Dictionary<string, string>
        {
            {"<flag1>", $"{mapName}{instance} ( {Data.currSSMinionList[0].Item1:F1}  , {Data.currSSMinionList[0].Item2:F1} )"},
            {"<flag2>", $"{mapName}{instance} ( {Data.currSSMinionList[1].Item1:F1}  , {Data.currSSMinionList[1].Item2:F1} )"},
            {"<flag3>", $"{mapName}{instance} ( {Data.currSSMinionList[2].Item1:F1}  , {Data.currSSMinionList[2].Item2:F1} )"},
            {"<flag4>", $"{mapName}{instance} ( {Data.currSSMinionList[3].Item1:F1}  , {Data.currSSMinionList[3].Item2:F1} )"},
        };
                foreach (var macro in Plugin.Configuration.Macro)
                {
                    Data.currMacro.Add(ProcessMacro(macro, waypoint));
                }
                SendMessage(Data.currMacro);
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
            return $"{((SeIconChar)((int)SeIconChar.Instance1 + (instance - 1))).ToIconChar()}";
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
            Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        }
    }
}
