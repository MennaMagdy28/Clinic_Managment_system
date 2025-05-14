using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Clinic_Sys.Models;
using System.Text.Json;

namespace Clinic_Sys.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinChatSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }

        public async Task LeaveChatSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        }

        public async Task SendMessage(ChatMessage message)
        {
            // The message will be sent to all clients in the chat session group
            await Clients.Group(message.SessionId.ToString()).SendAsync("ReceiveMessage", message);
        }

        public async Task MessageSeen(string messageId)
        {
            await Clients.Group(messageId).SendAsync("MessageSeen", messageId);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}