using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System;
using System.Linq;

namespace SSMinionBoradcast.Windows
{
    public class ConfigWindow : Window
    {
        public ConfigWindow() : base("SSMinionBoradcast设置")
        {
            var count = 0;
            foreach (var item in Enum.GetValues<SeIconChar>())
            {
                SeIconChar += $"{((SeIconChar)(int)item).ToIconChar()}";
                count++;

                if (count % 20 == 0)
                {
                    SeIconChar += Environment.NewLine;
                }
            }
        }

        private static string NewMacro = string.Empty;
        private static string SeIconChar = string.Empty;
        private bool showError = false;

        /// <summary>假宏上限 15 行，减去自动插入的 /mlock 后配置宏最多 14 行</summary>
        private const int MaxMacroLines = 14;

        /// <summary>单行宏的长度上限，超过后 ExecuteMacroNoFree 会拒绝发送</summary>
        private const int MaxLineLength = 180;

        public override void Draw()
        {
#if DEBUG
            if (ImGui.Button("打开主窗口"))
            {
                Plugin.MainWindow.IsOpen = true;
            }
#endif

            ImGui.Checkbox("启用自动播报", ref Plugin.Configuration.AutoBoradcast);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("当检测到SS前置触发时，自动发送宏");
            }

            ImGui.Text("播报宏列表");
            ImGui.SameLine();
            if (ImGui.Button("使用模板宏"))
            {
                AddTemplateMacro();
            }

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.BeginListBox("##宏列表"))
            {
                var macros = Plugin.Configuration.Macro;
                for (var i = 0; i < macros.Count; i++)
                {
                    // 直接编辑：每行常驻输入框，改动即时写回配置，无需进入编辑态/点保存
                    var line = macros[i];

                    if (i > 0)
                    {
                        if (ImGui.Button($"△##{i}"))
                            (macros[i - 1], macros[i]) = (macros[i], macros[i - 1]);
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        ImGui.Button($"△##{i}");
                        ImGui.EndDisabled();
                    }

                    ImGui.SameLine();
                    if (i < macros.Count - 1)
                    {
                        if (ImGui.Button($"▽##{i}"))
                            (macros[i + 1], macros[i]) = (macros[i], macros[i + 1]);
                    }
                    else
                    {
                        ImGui.BeginDisabled();
                        ImGui.Button($"▽##{i}");
                        ImGui.EndDisabled();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button($"删##{i}"))
                    {
                        macros.RemoveAt(i);
                        continue;
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - 45f);
                    if (ImGui.InputText($"##宏{i}", ref line, 500))
                    {
                        macros[i] = line;
                    }

                    ImGui.SameLine();
                    if (line.Length > MaxLineLength)
                    {
                        ImGui.TextColored(ImGuiColors.DalamudRed, $"{line.Length}");
                        if (ImGui.IsItemHovered())
                            ImGui.SetTooltip($"超过单行 {MaxLineLength} 字符，发送时会被拒绝");
                    }
                    else
                    {
                        ImGui.TextDisabled($"{line.Length}");
                    }
                }

                if (macros.Count < MaxMacroLines)
                {
                    var submitted = ImGui.InputText("##New", ref NewMacro, 500, ImGuiInputTextFlags.EnterReturnsTrue);
                    ImGui.SameLine();

                    var canAdd = !NewMacro.IsNullOrEmpty();
                    if (!canAdd)
                        ImGui.BeginDisabled();
                    if ((ImGui.Button("添加") && canAdd) || (submitted && canAdd))
                    {
                        macros.Add(NewMacro);
                        NewMacro = "";
                    }
                    if (!canAdd)
                        ImGui.EndDisabled();
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed,
                        $"宏数量已达到上限（{MaxMacroLines} 行 + 自动插入的 /mlock = 假宏 15 行上限）");
                }

                ImGui.EndListBox();
            }

            // ── 占位符覆盖状态（实时显示，不用点保存才发现缺） ──
            ImGui.Text("占位符检查：");
            for (var i = 1; i <= 4; i++)
            {
                ImGui.SameLine();
                var has = Plugin.Configuration.Macro.Any(m => m.Contains($"<flag{i}>"));
                ImGui.TextColored(has ? ImGuiColors.HealerGreen : ImGuiColors.DalamudRed,
                    $"<flag{i}> {(has ? "✓" : "X缺失")}");
            }

            ImGui.TextColored(ImGuiColors.DalamudYellow, "↓修改完记得点击保存！");
            if (ImGui.Button("保存"))
            {
                var valid = Enumerable.Range(1, 4)
                    .All(i => Plugin.Configuration.Macro.Any(m => m.Contains($"<flag{i}>")));
                if (valid)
                {
                    showError = false;
                    Plugin.Configuration.Save();
                    Svc.NotificationManager.AddNotification(new Notification()
                    {
                        Content = "配置已保存",
                        Title = "SSMinionBoradcast",
                        Type = NotificationType.Success
                    });
                }
                else
                {
                    showError = true;
                }
            }

            if (showError)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "宏必须包含<flag1-4>四个占位符！");
            }

            ImGui.Separator();
            ImGui.Text("游戏内特殊标志（可复制）");
            ImGui.InputTextMultiline("", ref SeIconChar, SeIconChar.Length, new System.Numerics.Vector2(-1, 200f.Scale()), flags: ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.CallbackResize);
        }

        public static void AddTemplateMacro()
        {
            Plugin.Configuration.Macro.Clear();
            Plugin.Configuration.Macro.Add("/sh 级恶名精英已触发，前置小怪请在周围玩家足够时再开怪，开怪后请勿拉脱！开怪后请勿拉脱！开怪后请勿拉脱！");
            Plugin.Configuration.Macro.Add("/y 级恶名精英已触发，前置小怪请在周围玩家足够时再开怪，开怪后请勿拉脱！开怪后请勿拉脱！开怪后请勿拉脱！<wait.2>");
            Plugin.Configuration.Macro.Add("/sh 本图级恶名精英已触发，请前往以下位置=>击杀前置小怪");
            Plugin.Configuration.Macro.Add("/y 本图级恶名精英已触发，请前往以下位置=>击杀前置小怪 <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■1号■■■<flag1> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■2号■■■<flag2> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■3号■■■<flag3> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■4号■■■<flag4> <wait.2>");
            Plugin.Configuration.Save();
        }
    }
}
