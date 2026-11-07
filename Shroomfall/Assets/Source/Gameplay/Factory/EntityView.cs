using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Visual;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.EntityDomain;
using UnityEngine;

namespace Assets.Source.Gameplay.Factory
{
    public class EntityView : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private VisualView visualView;

        [Header("Collision Debug")]
        [SerializeField] private bool showCollision = true;
        [SerializeField] private Color blockingColor = Color.red;
        [SerializeField] private Color nonBlockingColor = Color.yellow;

        private CollisionInstanceDTO collisionInstance;
        #endregion

        #region Properties
        public string ID { get; private set; }
        #endregion

        #region Methods
        public void Initialize(
            string id,
            CollisionInstanceDTO collision,
            Vector2 startPosition,
            EntityDirection dir,
            EntityAction action,
            EntityAsset entity,
            Color color,
            string name = null,
            Vector2? direction = null)
        {
            ID = id;
            collisionInstance = collision;

            ApplyPosition(startPosition);
            ApplyAppearance(entity, color, name);
            SetDirection(dir);
            SetAction(action);

            if (direction.HasValue && direction.Value != Vector2.zero)
                ApplyRotation(direction.Value);
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

        private void OnDrawGizmos()
        {
            if (!showCollision || collisionInstance == null)
                return;

            Gizmos.color = collisionInstance.IsBlocking ? blockingColor : nonBlockingColor;
            Vector3 offset = new Vector3(collisionInstance.CollisionOffset.X, collisionInstance.CollisionOffset.Y, 0f);
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.rotation * offset, transform.rotation, Vector3.one);

            switch (collisionInstance.ShapeType)
            {
                case CollisionShapeType.Point:
                    Gizmos.DrawWireSphere(Vector3.zero, 0.05f);
                    break;

                case CollisionShapeType.Box:
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(collisionInstance.Width, collisionInstance.Height, 0.01f));
                    break;

                case CollisionShapeType.Circle:
                    Gizmos.DrawWireSphere(Vector3.zero, collisionInstance.Radius);
                    break;
            }

            Gizmos.matrix = previousMatrix;
        }
        #endregion
    }
}