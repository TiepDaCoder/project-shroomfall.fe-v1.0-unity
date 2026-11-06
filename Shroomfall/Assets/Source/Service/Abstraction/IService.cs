using System.Threading.Tasks;

namespace Assets.Source.Service.Abstraction
{
    public interface IService
    {
        bool IsInitialized { get; }
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}