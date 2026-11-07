using System.Threading.Tasks;

namespace Assets.Source.Service
{
    public class TutorialService : IService
    {
        #region Attributes
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public bool IsActive { get; private set; }
        #endregion

        public TutorialService() { }

        #region Methods
        public Task InitializeAsync()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void StartTutorial()
        {
            IsActive = true;
        }

        public void StopTutorial()
        {
            IsActive = false;
        }
        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}