using Assets.Services;
using Assets.Utilities;
using UnityEngine;

namespace Assets.Gameplay.Factory
{
    public class InputBinder
    {
        #region Attributes
        private readonly PlayerService playerService;
        #endregion

        #region Properties
        #endregion

        public InputBinder(
            PlayerService playerService)
        {
            this.playerService = playerService;
        }

        #region Methods
        public void Bind(LocalView view)
        {
            view.OnMoved += OnMoved;
            view.OnItemUsed += OnItemUsed;
        }

        public void Unbind(LocalView view)
        {
            view.OnMoved -= OnMoved;
            view.OnItemUsed -= OnItemUsed;
        }

        public void OnMoved(
            Vector2 direction)
        {
            AsyncHelper.Run(async () =>
            {
                await playerService.MoveAsync(direction);
            });
        }

        public void OnItemUsed(
            Vector2 direction)
        {
            AsyncHelper.Run(async () =>
            {
                await playerService.UseItem(direction);
            });
        }
        #endregion
    }
}