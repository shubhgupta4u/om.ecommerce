using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace om.shared.signalr
{
    public abstract class BaseHubClient: IHubClient
    {
        private readonly HubConnection connection;
        public BaseHubClient(SignalRSetting setting, string hubEndPoint)
        {
            this.connection = new HubConnectionBuilder()
                .WithUrl(string.Format("{0}/{1}", setting.BaseUrl, hubEndPoint), options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(setting.JwtBearerToken);
                })
                .Build();
        }

        public async Task SendMessage(string methodName, object message)
        {
            try
            {
                await this.connection.StartAsync();

                await this.connection.InvokeAsync(methodName, message);

                await this.connection.StopAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        
        }

        public abstract Task SendMessage<T>(T orderStatus) where T:class;
    }
}
