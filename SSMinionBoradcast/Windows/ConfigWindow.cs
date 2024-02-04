using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSMinionBoradcast.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        public ConfigWindow() : base("SSMinionBoradcast设置") { }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static int SelectedItemIndex = -1;
        private static string EditMacro = string.Empty;
        private static string NewMacro = string.Empty;
        private bool showError = false;

        public override void Draw()
        {
            ImGui.Checkbox("启用自动播报", ref Plugin.Configuration.AutoBoradcast);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("当检测到SS前置触发时，自动发送宏");
            }

            ImGui.Text("播报宏列表");
            if (ImGui.Button("添加模板宏"))
            {
                Plugin.Configuration.Macro.Add("/sh 级恶名精英已触发，小怪请在五分钟内建立仇恨，请在人数足够时再开怪，开怪后请勿拉脱！开怪后请勿拉脱！开怪后请勿拉脱！");
                Plugin.Configuration.Macro.Add("/sh 本图级恶名精英已触发，请前往以下位置=>击杀前置小怪");
                Plugin.Configuration.Macro.Add("/sh ■■■1号■■■<flag1>");
                Plugin.Configuration.Macro.Add("/sh ■■■2号■■■<flag2>");
                Plugin.Configuration.Macro.Add("/sh ■■■3号■■■<flag3>");
                Plugin.Configuration.Macro.Add("/sh ■■■4号■■■<flag4>");
                Plugin.Configuration.Save();
            }
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.BeginListBox("##宏列表"))
            {
                for (var i = 0; i < Plugin.Configuration.Macro.Count; i++)
                {
                    if (SelectedItemIndex != i)
                    {
                        ImGui.Text($"{Plugin.Configuration.Macro[i]}");
                        if (ImGui.IsItemClicked())
                        {
                            SelectedItemIndex = i;
                            EditMacro = Plugin.Configuration.Macro[i];
                        }
                        ImGui.SameLine();
                        if (ImGui.Button($"删除##{i}"))
                        {
                            Plugin.Configuration.Macro.RemoveAt(i);
                        }
                    }
                    else
                    {
                        ImGui.InputText($"##Edit{i}", ref EditMacro, 200);
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
                    }
                }

                ImGui.InputText("##New", ref NewMacro, 100);
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

                ImGui.EndListBox();
            }
            if (ImGui.Button("保存"))
            {
                List<string> allflags = ["<flag1>", "<flag2>", "<flag3>", "<flag4>"];
                var valid = allflags.All(flag => Plugin.Configuration.Macro.Any(macro => macro.Contains(flag)));
                if (valid)
                {
                    showError = false;
                    Plugin.Configuration.Save();
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
#if DEBUG
            if (ImGui.CollapsingHeader("Debug"))
            {
                ImGui.Text($"isBoradcasting:{Data.isBoradcasting}");

                foreach (var item in Data.SSMinion)
                {
                    if (ImGui.CollapsingHeader($"TerritoryType:{item.Key}"))
                    {
                        foreach (var item1 in item.Value)
                        {
                            ImGui.Text($"({item1.Item1}, {item1.Item2})");
                        }
                    }
                }

                if (ImGui.CollapsingHeader($"currSSMinionList"))
                {
                    foreach (var item in Data.currSSMinionList)
                    {
                        ImGui.Text($"({item.Item1}, {item.Item2})");
                    }
                }
            }
#endif
        }
    }
}
