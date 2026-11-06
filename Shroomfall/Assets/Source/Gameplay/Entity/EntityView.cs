using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Visual;
using Contract.Enum.EntityDomain;
using UnityEngine;

namespace Assets.Source.Gameplay.Entity
{
    public class EntityView : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private VisualView visualView;
        #endregion

        #region Properties
        public string ID { get; private set; }
        #endregion

        #region Methods
        public void Initialize(
            string id,
            Vector2 startPosition,
            EntityDirection dir,
            EntityAction action,
            EntityAsset entity,
            Color color,
            string name = null,
            Vector2? direction = null)
        {
            ID = id;

            ApplyPosition(startPosition);

            ApplyAppearance(entity, color, name);

            SetDirection(dir);
            SetAction(action);

            if (direction.HasValue && direction.Value != Vector2.zero)
            {
                ApplyRotation(direction.Value);
            }
        }

        public void SetDirection(
            EntityDirection dir)
        {
            visualView.SetDirection(dir);
        }

        public void SetAction(
            EntityAction action)
        {
            visualView.SetAction(action);
        }

        public void HoldItem(
            ItemAsset item)
        {
            visualView.HoldItem(item);
        }

        public virtual void ApplyPosition(
            Vector2 pos)
        {
            Vector3 targetPos = new Vector3(pos.x, pos.y, 0);
            transform.position = targetPos;
        }

        public void ApplyAppearance(
            EntityAsset entity,
            Color color,
            string name)
        {
            visualView.ApplyAppearance(entity, color, name);
        }

        private void ApplyRotation(
            Vector2 dir)
        {
            float angleInDegrees = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angleInDegrees);
        }
        #endregion
    }
}