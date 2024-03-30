using Dalamud.Game.Text;
using Dalamud.Interface.Internal.Notifications;
using ECommons.DalamudServices;

namespace SSMinionBoradcast
{
    public static class Events
    {
        public static void Enable()
        {
            Svc.Chat.ChatMessage += Chat_ChatMessage;
            Svc.ClientState.TerritoryChanged += ClientState_TerritoryChanged;

            if (Svc.ClientState.LocalPlayer is not null)
            {
                if (Data.SSMinion.TryGetValue(Svc.ClientState.TerritoryType, out var ssminionlist))
                {
                    Data.currSSMinionList = ssminionlist;
                }
            }
        }

        private static void ClientState_TerritoryChanged(ushort obj)
        {
            Plugin.MainWindow.IsOpen = false;

            if (Data.SSMinion.TryGetValue(Svc.ClientState.TerritoryType, out var ssminionlist))
            {
                Data.currSSMinionList = ssminionlist;
            }
            else
            {
                Data.currSSMinionList.Clear();
            }
        }

        private static void Chat_ChatMessage(XivChatType type, uint senderId, ref Dalamud.Game.Text.SeStringHandling.SeString sender, ref Dalamud.Game.Text.SeStringHandling.SeString message, ref bool isHandled)
        {
            if ((int)type == 2105 && message.TextValue.Contains("特殊恶名精英的手下开始了侦察活动"))
            {
                Svc.PluginInterface.UiBuilder.AddNotification("特殊恶名精英的手下开始了侦察活动", "SSMinionBoradcast", NotificationType.Warning);
                Plugin.MainWindow.IsOpen = true;

                if (Plugin.Configuration.AutoBoradcast)
                {
                    Boradcast.ProcessData();
                }
            }

            if (type == XivChatType.Echo && message.TextValue == "test" && Svc.ClientState.TerritoryType == 1055)
            {
                Svc.PluginInterface.UiBuilder.AddNotification("特殊恶名精英的手下开始了侦察活动", "SSMinionBoradcast", NotificationType.Warning);
                Plugin.MainWindow.IsOpen = true;

                if (Plugin.Configuration.AutoBoradcast)
                {
                    Boradcast.ProcessData();
                }
            }
        }

        public static void Disable()
        {
            Svc.Chat.ChatMessage -= Chat_ChatMessage;
            Svc.ClientState.TerritoryChanged -= ClientState_TerritoryChanged;
        }
    }
}
