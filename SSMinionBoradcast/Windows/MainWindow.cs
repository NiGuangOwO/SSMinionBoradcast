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
    }

    private readonly Stopwatch cooldownWatch = new();

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudYellow, "特殊恶名精英的手下开始了侦察活动");
        ImGui.Text($"自动播报状态：{(Plugin.Configuration.AutoBoradcast ? "启用" : "禁用")}");

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
