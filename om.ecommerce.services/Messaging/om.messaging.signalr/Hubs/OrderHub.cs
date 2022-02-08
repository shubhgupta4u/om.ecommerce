using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using om.shared.signalr.HubModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace om.messaging.signalr.Hubs
{
    [Authorize]
    public class OrderHub: Hub
    {
        //private readonly ISignalRConnectionRepository signalRConnectionRepository;
        public OrderHub()
        {
            //this.signalRConnectionRepository = signalRConnectionRepository;
        }
        public async Task BroadcastOrderStatusChangedEvent(OrderStatus orderStatus)
        {
            //string buyerConnectionId = await this.GetUserConnectionId(orderStatus.UserId);
            //string sellerConnectionId = await this.GetUserConnectionId(orderStatus.SellerId);
            if (Context.User.Identity.Name == null)
            {
                await Clients.All.SendAsync("OrderStatusChanged", orderStatus);
            }
            else
            {
                if (orderStatus != null && !string.IsNullOrWhiteSpace(orderStatus.UserId))
                {
                    await Clients.User(orderStatus.UserId).SendAsync("OrderStatusChanged", orderStatus);
                }
                if (orderStatus != null && !string.IsNullOrWhiteSpace(orderStatus.SellerId))
                {
                    await Clients.User(orderStatus.SellerId).SendAsync("OrderStatusChanged", orderStatus);
                }
            }
        }

        public override Task OnConnectedAsync()
        {
            //string userId = this.GetUserId();
            //if (!string.IsNullOrWhiteSpace(userId))
            //{
            //    string connectionId = Context.ConnectionId;
            //    this.signalRConnectionRepository.WriteAsync(new shared.caching.Models.SignalRConnection() { UserId = userId, ConnectionId = connectionId }).Wait();
            //}            
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            //string userId = this.GetUserId();
            //if (!string.IsNullOrWhiteSpace(userId))
            //{
            //    //this.signalRConnectionRepository.RemoveAsync(userId).Wait();
            //}
            return base.OnDisconnectedAsync(exception);
        }
        private async Task<string> GetUserConnectionId(string userId)
        {
            //shared.caching.Models.SignalRConnection conn= await this.signalRConnectionRepository.GetAsync(userId);
            //if(conn != null)
            //{
            //    return conn.ConnectionId;
            //}
            return null;
        }
        private string GetUserId()
        {
            return Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
