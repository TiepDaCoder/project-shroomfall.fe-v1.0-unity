using UnityEngine;

namespace Assets.Source.Gameplay.Component.Visual
{
    [ExecuteAlways]
    public class SnapToGrid : MonoBehaviour
    {
        [SerializeField]
        private float gridSize = 1f;

        private void Update()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            Snap();
#endif
        }

        private void Snap()
        {
            Vector3 position = transform.position;

            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;

            if (transform.position != position)
            {
                transform.position = position;
            }
        }
    }
}