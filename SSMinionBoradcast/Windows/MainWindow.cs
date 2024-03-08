using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
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

    private void ClientState_TerritoryChanged(ushort e)
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

        if (ImGui.Button("全图广播"))
        {
            Svc.PluginInterface.UiBuilder.AddNotification("开始发送喊话宏", "SSMinionBoradcast", NotificationType.Success);
            Svc.Chat.Print("[SSMinionBoradcast] 开始发送喊话宏");
            Boradcast.ProcessData();
        }

        ImGui.SameLine();
        if (ImGui.Button("中止喊话"))
        {
            Chat.Instance.SendMessage("/mcancel");
            Svc.PluginInterface.UiBuilder.AddNotification("已停止执行喊话宏", "SSMinionBoradcast", NotificationType.Warning);
            Svc.Chat.Print("[SSMinionBoradcast] 已停止执行喊话宏");
        }

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
