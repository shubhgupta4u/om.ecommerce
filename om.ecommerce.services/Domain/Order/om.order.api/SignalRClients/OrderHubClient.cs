using Microsoft.Extensions.Options;
using om.shared.signalr;
using System.Threading.Tasks;

namespace om.order.api.SignalRClients
{
    public class OrderHubClient: BaseHubClient
    {
        public OrderHubClient(IOptions<SignalRSetting> options) : base(options.Value, "order")
        {
        }
        public override async Task SendMessage<T>(T orderStatus) where T : class
        {
            await base.SendMessage("BroadcastOrderStatusChangedEvent", orderStatus);
        }
    }
}
