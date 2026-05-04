using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SSMinionBoradcast
{
    public static partial class Boradcast
    {
        private static readonly Regex FlagRegex = MapLinkRegex();

        /// <summary>处理喊话数据，send=true 时立即发送，可指定地图（null=当前地图）</summary>
        public static unsafe void ProcessData(bool send, uint? overrideTerritoryId = null)
        {
            Data.currMacro.Clear();
            var tid = overrideTerritoryId ?? GameMain.Instance()->CurrentTerritoryTypeId;

            if (!Data.SSMinion.TryGetValue(tid, out var ssminionlist))
            {
                Svc.Log.Error($"地图上不存在SS小怪点位！TerritoryTypeId={tid}");
                return;
            }

            if (ssminionlist.Length < 4)
            {
                Svc.Log.Error($"地图上SS小怪点位不足4个（实际：{ssminionlist.Length}），无法生成播报宏！");
                return;
            }

            var mapName = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(tid).PlaceName.Value.Name.ExtractText();
            var instance = GetCharacterForInstanceNumber(UIState.Instance()->PublicInstance.InstanceId);
            var waypoint = BuildWaypoint(mapName, instance, ssminionlist);

            foreach (var macro in Plugin.Configuration.Macro)
            {
                Data.currMacro.Add(ProcessMacro(macro, waypoint));
            }

            if (send)
            {
                SendMessage(Data.currMacro);
            }
        }

        /// <summary>以 /e（默语）频道测试喊话，不影响其他玩家</summary>
        public static void TestSend(uint territoryTypeId)
        {
            ProcessData(false, territoryTypeId);

            if (Data.currMacro.Count == 0)
            {
                Svc.NotificationManager.AddNotification(new Notification()
                {
                    Title = "SSMinionBoradcast",
                    Content = "没有可发送的宏，请检查配置",
                    Type = NotificationType.Error,
                });
                return;
            }

            Chat.SendMessage("/mcancel");

            var toSend = new List<string>();
            foreach (var macro in Data.currMacro)
            {
                toSend.Add(ReplaceChannelWithEcho(macro));
            }
            toSend.Insert(0, "/mlock");
            MacroManager.Execute(toSend);

            var mapName = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(territoryTypeId)
                .PlaceName.Value.Name.ExtractText();
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast 测试",
                Content = $"测试喊话已发送（默语频道）— {mapName}",
                Type = NotificationType.Success,
            });
        }

        /// <summary>将宏前缀替换为 /e（默语），保留 /mlock、/mcancel 等控制命令</summary>
        private static string ReplaceChannelWithEcho(string macro)
        {
            if (macro.StartsWith("/mlock") || macro.StartsWith("/mcancel"))
                return macro;

            string[] channelPrefixes = ["/sh ", "/shout ", "/y ", "/yell "];
            foreach (var prefix in channelPrefixes)
            {
                if (macro.StartsWith(prefix))
                    return "/e " + macro[prefix.Length..];
            }

            // 无已知前缀时直接加 /e
            return "/e " + macro;
        }

        private static Dictionary<string, string> BuildWaypoint(string mapName, string instance, Vector2[] ssminionlist)
        {
            return new Dictionary<string, string>
            {
                { "<flag1>", $"{mapName}{instance} ( {ssminionlist[0].X:F1}  , {ssminionlist[0].Y:F1} )" },
                { "<flag2>", $"{mapName}{instance} ( {ssminionlist[1].X:F1}  , {ssminionlist[1].Y:F1} )" },
                { "<flag3>", $"{mapName}{instance} ( {ssminionlist[2].X:F1}  , {ssminionlist[2].Y:F1} )" },
                { "<flag4>", $"{mapName}{instance} ( {ssminionlist[3].X:F1}  , {ssminionlist[3].Y:F1} )" },
            };
        }

        private static string GetCharacterForInstanceNumber(uint instance)
        {
            if (instance == 0)
                return string.Empty;
            return $"{((SeIconChar)((int)SeIconChar.Instance1 + (instance - 1))).ToIconChar()}";
        }

        private static string ProcessMacro(string macro, Dictionary<string, string> waypoint)
        {
            return FlagRegex.Replace(macro, match =>
                waypoint.TryGetValue(match.Value, out var val) ? val : match.Value);
        }

        public static void SendMessage(List<string> macro)
        {
            Chat.SendMessage("/mcancel");

            var toSend = new List<string>(macro);
            toSend.Insert(0, "/mlock");
            MacroManager.Execute(toSend);
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast",
                Content = "开始发送喊话宏",
                Type = NotificationType.Success
            });
        }

        [GeneratedRegex(@"<flag[1-4]>", RegexOptions.Compiled)]
        private static partial Regex MapLinkRegex();
    }
}
