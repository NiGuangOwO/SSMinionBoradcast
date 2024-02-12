using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using System;
using System.Numerics;

namespace SSMinionBoradcast.Windows;

public class MainWindow : Window, IDisposable
{
    public MainWindow() : base("SS前置小怪播报", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Svc.Chat.ChatMessage += ChatGui_OnChatMessage;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Svc.Chat.ChatMessage -= ChatGui_OnChatMessage;
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
    }

    private void ClientState_TerritoryChanged(object? sender, ushort e)
    {
        IsOpen = false;
    }

    private void ChatGui_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if ((int)type == 2105 && message.TextValue.Contains("特殊恶名精英的手下开始了侦察活动"))
        {
            IsOpen = true;
        }
    }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudYellow, "特殊恶名精英的手下开始了侦察活动");

        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.BeginDisabled();
        if (ImGui.Button("全图广播"))
        {
            Boradcast.ProcessData();
        }
        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button("中止喊话"))
        {
            Data.isBoradcasting = false;
            Plugin.TaskManager.Abort();
        }

        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.TextColored(ImGuiColors.HealerGreen, $"当前待发送宏的数量:{Plugin.TaskManager.NumQueuedTasks}");

        ImGui.Separator();
        ImGui.Text("当前宏列表");
        ImGui.BeginChild("");
        foreach (var item in Plugin.Configuration.Macro)
        {
            ImGui.Text($"{item}");
        }
        ImGui.EndChild();
    }
}
