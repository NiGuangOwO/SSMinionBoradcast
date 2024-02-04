using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using System;

namespace SSMinionBoradcast.Windows;

public class MainWindow : Window, IDisposable
{
    public MainWindow() : base("SS前置小怪播报", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        Svc.Chat.ChatMessage += ChatGui_OnChatMessage;
        Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;
    }

    private void ClientState_TerritoryChanged(object? sender, ushort e)
    {
        IsOpen = false;
    }

    private void ChatGui_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (type == XivChatType.SystemMessage && message.TextValue == "特殊恶名精英的手下开始了侦察活动......")
        {
            IsOpen = true;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Svc.Chat.ChatMessage -= ChatGui_OnChatMessage;
        Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
    }

    public override void Draw()
    {
        ImGui.TextColored(ImGuiColors.DalamudYellow, "特殊恶名精英的手下开始了侦察活动");

        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.BeginDisabled();
        if (ImGui.Button("全图广播"))
        {
            Boradcast.ProcessData(true);
        }
        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.EndDisabled();

        if (ImGui.Button("中止喊话"))
        {
            Data.isBoradcasting = false;
            Plugin.TaskManager.Abort();
        }
        if (Data.isBoradcasting || Plugin.TaskManager.IsBusy)
            ImGui.TextColored(ImGuiColors.HealerGreen, $"当前待发送宏的数量:{Plugin.TaskManager.NumQueuedTasks}");

        ImGui.Text("当前宏列表");
        foreach (var item in Plugin.Configuration.Macro)
        {
            ImGui.Text($"{item}");
        }
    }
}
