using Dalamud.Game.Text;
using Dalamud.Interface.ImGuiNotification;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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
            ExecuteMacroNoFree(toSend);

            var mapName = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(territoryTypeId)
                .PlaceName.Value.Name.ExtractText();
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast 测试",
                Content = $"测试喊话已发送（默语频道）— {mapName}",
                Type = NotificationType.Success,
            });
        }

        /// <summary>单独发送一个点位（pointIndex 0-3）的坐标喊话，使用配置宏中含对应 &lt;flagN&gt; 的行</summary>
        public static unsafe void SendSinglePoint(int pointIndex)
        {
            var tid = GameMain.Instance()->CurrentTerritoryTypeId;
            if (!Data.SSMinion.TryGetValue(tid, out var ssminionlist) || pointIndex >= ssminionlist.Length)
            {
                Svc.Log.Error($"当前地图不存在SS小怪点位{pointIndex + 1}！TerritoryTypeId={tid}");
                return;
            }

            var flag = $"<flag{pointIndex + 1}>";
            var configIdx = Plugin.Configuration.Macro.FindIndex(m => m.Contains(flag));
            if (configIdx < 0)
            {
                Svc.NotificationManager.AddNotification(new Notification()
                {
                    Title = "SSMinionBoradcast",
                    Content = $"宏模板中未找到 {flag}，无法单独发送{pointIndex + 1}号点位",
                    Type = NotificationType.Error,
                });
                return;
            }

            var mapName = Svc.Data.GetExcelSheet<TerritoryType>().GetRow(tid).PlaceName.Value.Name.ExtractText();
            var instance = GetCharacterForInstanceNumber(UIState.Instance()->PublicInstance.InstanceId);
            var coord = ssminionlist[pointIndex];
            var coordText = $"{mapName}{instance} ( {coord.X:F1}  , {coord.Y:F1} )";
            var line = ProcessMacro(Plugin.Configuration.Macro[configIdx],
                new Dictionary<string, string> { { flag, coordText } });

            Chat.SendMessage("/mcancel");
            ExecuteMacroNoFree(["/mlock", line]);

            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast",
                Content = $"已发送{pointIndex + 1}号点位：{coordText}",
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
            ExecuteMacroNoFree(toSend);
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast",
                Content = "开始发送喊话宏",
                Type = NotificationType.Success
            });
        }

        /// <summary>
        /// Execute a fake macro without freeing it. ECommons MacroManager frees the
        /// macro struct and long-line heap buffers immediately after ExecuteMacro
        /// returns, but the shell keeps executing lines across frames (waits span
        /// seconds); the late reads hit freed memory and lines turn into garbled
        /// commands. Leak the block instead (a few KB per broadcast; the game never
        /// frees caller-provided macros — real macros live in RaptureMacroModule).
        /// </summary>
        private static unsafe void ExecuteMacroNoFree(IReadOnlyList<string> commands)
        {
            // ECommons Macro is marked obsolete in favor of the game struct, but the
            // game struct cannot be constructed from managed strings this easily.
#pragma warning disable CS0618
            if (commands.Count > Macro.numLines)
                throw new InvalidOperationException("Macro was more than 15 lines!");
            if (commands.Any(x => x.Length > 180))
                throw new InvalidOperationException("Macro contained lines more than 180 symbols!");

            var macroPtr = Marshal.AllocHGlobal(Macro.size);
            var macro = new Macro(macroPtr, string.Empty, commands);
            Marshal.StructureToPtr(macro, macroPtr, false);
            RaptureShellModule.Instance()->ExecuteMacro((RaptureMacroModule.Macro*)macroPtr);
#pragma warning restore CS0618
            // deliberately no FreeHGlobal / Dispose here — see summary above
        }

        [GeneratedRegex(@"<flag[1-4]>", RegexOptions.Compiled)]
        private static partial Regex MapLinkRegex();
    }
}
