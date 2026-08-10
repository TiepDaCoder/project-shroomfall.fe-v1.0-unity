using System.Threading.Tasks;

namespace Assets.Services
{
    public interface IService
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}