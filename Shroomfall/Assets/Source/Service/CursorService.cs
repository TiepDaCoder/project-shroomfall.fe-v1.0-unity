using Assets.Source.Data;
using Assets.Source.Enum;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Source.Service
{
    public class CursorService : IService
    {
        #region Attributes 
        private CursorCatalogSO cursorCatalog;
        #endregion

        #region Properties
        public bool IsInitialized { get; private set; } = false;
        public CursorType CurrentCursor { get; private set; }
        #endregion 

        public CursorService(
            CursorCatalogSO cursorCatalog)
        {
            this.cursorCatalog = cursorCatalog;
            SetCursor(CursorType.Default);
        }

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

        public void SetCursor(
            CursorType type)
        {
            Texture2D texture = cursorCatalog.Get(type);
            if (texture == null)
                return;

            CurrentCursor = type;
            Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
        }

        #region Senders
        #endregion

        #region Receivers
        #endregion
        #endregion
    }
}