using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSMinionBoradcast.Windows
{
    public class ConfigWindow : Window
    {
        public ConfigWindow() : base("SSMinionBoradcast设置")
        {
            var count = 0;
            foreach (var item in Enum.GetValues(typeof(SeIconChar)))
            {
                SeIconChar += $"{((SeIconChar)(int)item).ToIconChar()}";
                count++;

                if (count % 20 == 0)
                {
                    SeIconChar += Environment.NewLine;
                }
            }
        }

        private static int SelectedItemIndex = -1;
        private static string EditMacro = string.Empty;
        private static string NewMacro = string.Empty;
        private bool showError = false;
        private static string SeIconChar = string.Empty;

        public override void Draw()
        {
            ImGui.Checkbox("启用自动播报", ref Plugin.Configuration.AutoBoradcast);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("当检测到SS前置触发时，自动发送宏");
            }

            ImGui.Text("播报宏列表");
            ImGui.SameLine();
            if (ImGui.Button("添加模板宏") && Plugin.Configuration.Macro.Count < 8)
            {
                AddTemplateMacro();
            }

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.BeginListBox("##宏列表"))
            {
                for (var i = 0; i < Plugin.Configuration.Macro.Count; i++)
                {
                    if (SelectedItemIndex != i)
                    {
                        if (ImGui.Button($"删除##{i}"))
                        {
                            Plugin.Configuration.Macro.RemoveAt(i);
                            continue;
                        }
                        ImGui.SameLine();

                        ImGui.Text($"{Plugin.Configuration.Macro[i]}");
                        if (ImGui.IsItemClicked())
                        {
                            SelectedItemIndex = i;
                            EditMacro = Plugin.Configuration.Macro[i];
                        }
                    }
                    else
                    {
                        ImGui.InputText($"##Edit{i}", ref EditMacro, 1000);
                        ImGui.SameLine();

                        if (i > 0)
                        {
                            if (ImGui.Button($"△##{i}"))
                            {
                                Plugin.Configuration.Macro[i] = EditMacro;
                                (Plugin.Configuration.Macro[i - 1], Plugin.Configuration.Macro[i]) = (Plugin.Configuration.Macro[i], Plugin.Configuration.Macro[i - 1]);
                                SelectedItemIndex = -1;
                            }
                            ImGui.SameLine();
                        }

                        if (i < Plugin.Configuration.Macro.Count - 1)
                        {
                            if (ImGui.Button($"▽##{i}"))
                            {
                                Plugin.Configuration.Macro[i] = EditMacro;
                                (Plugin.Configuration.Macro[i + 1], Plugin.Configuration.Macro[i]) = (Plugin.Configuration.Macro[i], Plugin.Configuration.Macro[i + 1]);
                                SelectedItemIndex = -1;
                            }
                        }

                        ImGui.SameLine();

                        if (ImGui.Button($"保存##{i}"))
                        {
                            Plugin.Configuration.Macro[i] = EditMacro;
                            SelectedItemIndex = -1;
                            EditMacro = "";
                        }

                        ImGui.SameLine();

                        ImGui.Text($"{EditMacro.Length}");
                    }
                }
                if (Plugin.Configuration.Macro.Count < 14)
                {
                    ImGui.InputText("##New", ref NewMacro, 1000);
                    ImGui.SameLine();

                    if (NewMacro.IsNullOrEmpty())
                        ImGui.BeginDisabled();
                    if (ImGui.Button("添加"))
                    {
                        Plugin.Configuration.Macro.Add(NewMacro);
                        NewMacro = "";
                    }
                    if (NewMacro.IsNullOrEmpty())
                        ImGui.EndDisabled();
                }
                else
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, "宏数量已经到达上限");
                }

                ImGui.EndListBox();
            }

            ImGui.TextColored(ImGuiColors.DalamudYellow, "↓修改完记得点击保存！");

            if (ImGui.Button("保存"))
            {
                List<string> allflags = ["<flag1>", "<flag2>", "<flag3>", "<flag4>"];
                var valid = allflags.All(flag => Plugin.Configuration.Macro.Any(macro => macro.Contains(flag)));
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
            ImGui.InputTextMultiline("", ref SeIconChar, SeIconChar.Length, new System.Numerics.Vector2(-1, -1), flags: ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.CallbackResize);
#if DEBUG
            if (ImGui.CollapsingHeader("Debug"))
            {
                foreach (var item in Data.SSMinion)
                {
                    if (ImGui.CollapsingHeader($"TerritoryType:{item.Key}"))
                    {
                        foreach (var item1 in item.Value)
                        {
                            ImGui.Text($"({item1.X}, {item1.Y})");
                        }
                    }
                }
            }
#endif
        }

        public static void AddTemplateMacro()
        {
            Plugin.Configuration.Macro.Add("/sh 级恶名精英已触发，小怪请在五分钟内建立仇恨，请在人数足够时再开怪，开怪后请勿拉脱！开怪后请勿拉脱！开怪后请勿拉脱！<wait.2>");
            Plugin.Configuration.Macro.Add("/sh 本图级恶名精英已触发，请前往以下位置=>击杀前置小怪 <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■1号■■■<flag1> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■2号■■■<flag2> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■3号■■■<flag3> <wait.2>");
            Plugin.Configuration.Macro.Add("/sh ■■■4号■■■<flag4> <wait.2>");
            Plugin.Configuration.Save();
        }
    }
}
