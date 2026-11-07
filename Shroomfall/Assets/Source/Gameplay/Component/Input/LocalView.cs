using Assets.Source.Gameplay.Factory;
using UnityEngine;

namespace Assets.Source.Gameplay.Component.Input
{
    public class LocalView : EntityView
    {
        #region Attributes
        private Camera mainCamera;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            FollowCamera(transform.position);
        }

        public override void ApplyPosition(
            Vector2 pos)
        {
            Vector3 targetPos = new Vector3(pos.x, pos.y, 0);
            transform.position = targetPos;
        }

        private void FollowCamera(
            Vector3 playerPos)
        {
            if (mainCamera == null) return;

            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;

            Vector3 clamped = new Vector3(playerPos.x, playerPos.y, mainCamera.transform.position.z);

            // Immediate stop at boundary
            mainCamera.transform.position = clamped;
        }
        #endregion
    }
}