#if DEBUG
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;
#endif
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ECommons.DalamudServices;
using System.Diagnostics;
using System.Numerics;

namespace SSMinionBoradcast.Windows;

public class MainWindow : Window
{
    public MainWindow() : base("SS前置小怪播报", flags: ImGuiWindowFlags.NoCollapse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        // 构建地图选择列表（仅 Debug 模式）
        BuildMapList();
    }

    private readonly Stopwatch cooldownWatch = new();
    private readonly List<(uint Id, string Name)> mapList = [];
    private int selectedTestMapIndex = 0;

    private void BuildMapList()
    {
        mapList.Clear();
        var sheet = Svc.Data.GetExcelSheet<TerritoryType>();
        foreach (var kv in Data.SSMinion.OrderBy(kv => kv.Key))
        {
            var name = sheet.GetRow(kv.Key).PlaceName.Value.Name.ExtractText();
            mapList.Add((kv.Key, string.IsNullOrEmpty(name) ? $"Unknown ({kv.Key})" : name));
        }
    }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudYellow, "特殊恶名精英的手下开始了侦察活动");
        ImGui.Text($"自动播报状态：{(Plugin.Configuration.AutoBoradcast ? "启用" : "禁用")}");

        // ── 正式广播按钮（带冷却） ──
        var onCooldown = cooldownWatch.IsRunning && cooldownWatch.Elapsed.TotalSeconds < 3;
        if (onCooldown)
            ImGui.BeginDisabled();
        if (ImGui.Button("全图广播"))
        {
            Boradcast.ProcessData(true);
            cooldownWatch.Restart();
        }
        if (onCooldown)
            ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button("中止喊话"))
        {
            Chat.SendMessage("/mcancel");
            Svc.NotificationManager.AddNotification(new Notification()
            {
                Title = "SSMinionBoradcast",
                Content = "已停止执行喊话宏",
                Type = NotificationType.Warning
            });
        }

        // ── 测试功能 ──
        ImGui.Separator();
        ImGui.TextColored(ImGuiColors.DalamudYellow, "地图喊话测试（/e 默语频道）");

        // 地图下拉选择
        if (mapList.Count == 0)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "没有可用的地图数据");
        }
        else
        {
            var preview = mapList[selectedTestMapIndex].Name;
            if (ImGui.BeginCombo("##测试地图选择", preview))
            {
                for (var i = 0; i < mapList.Count; i++)
                {
                    var isSelected = selectedTestMapIndex == i;
                    if (ImGui.Selectable($"{mapList[i].Name}  (ID:{mapList[i].Id})", isSelected))
                    {
                        selectedTestMapIndex = i;
                    }
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.SameLine();
            if (ImGui.Button("测试喊话"))
            {
                var tid = mapList[selectedTestMapIndex].Id;
                Boradcast.TestSend(tid);
            }

            // 如果当前地图在列表中，显示快捷按钮
            var currentTid = Svc.ClientState.TerritoryType;
            var currentIdx = mapList.FindIndex(m => m.Id == currentTid);
            if (currentIdx >= 0 && currentIdx != selectedTestMapIndex)
            {
                ImGui.SameLine();
                if (ImGui.Button($"使用当前地图"))
                {
                    selectedTestMapIndex = currentIdx;
                }
            }
        }

        // ── 当前宏列表预览 ──
        ImGui.Separator();
        ImGui.Text("当前宏列表");
        ImGui.BeginChild("##MainWindow当前宏列表");
        foreach (var item in Data.currMacro)
        {
            ImGui.Text($"{item}");
        }
        ImGui.EndChild();
    }
}
