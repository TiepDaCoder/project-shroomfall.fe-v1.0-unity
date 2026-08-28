using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public interface IService
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}