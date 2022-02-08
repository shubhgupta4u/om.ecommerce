using System.Threading.Tasks;

namespace om.shared.signalr
{
    public interface IHubClient
    {
        Task SendMessage<T>(T orderStatus) where T : class;
    }
}
