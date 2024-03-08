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

        private void ClientState_TerritoryChanged(ushort e)
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
#if DEBUG
            if (type == XivChatType.Echo && message.TextValue == "test")
            {
                ProcessData();
            }
#endif
        }

        public static unsafe void ProcessData()
        {
            Data.currMacro.Clear();
            if (Data.currSSMinionList.Any())
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
                SendMessage(Data.currMacro);
            }
        }

        public static void SendMessage(List<string> macro)
        {
            macro.Insert(0, "/mlock");
            MacroManager.Execute(macro);
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
