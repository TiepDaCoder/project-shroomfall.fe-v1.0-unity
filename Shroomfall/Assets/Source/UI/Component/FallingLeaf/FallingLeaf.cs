using UnityEngine;

namespace Assets.Source.UI.Component.FallingLeaf
{
    public class FallingLeaf : MonoBehaviour
    {
        #region Attributes
        private Vector2 startPos;
        private Vector2 endPos;
        private float moveSpeed;
        private float progress;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Update()
        {
            // Linearly calculate precise path position behind the scenes
            float distance = Vector2.Distance(startPos, endPos);
            progress += (moveSpeed / distance) * Time.deltaTime;

            Vector2 exactPosition = Vector2.Lerp(startPos, endPos, progress);

            // SNAP MOVE: Remove decimal data by rounding to absolute whole pixels
            float pixelSnappedX = Mathf.Round(exactPosition.x);
            float pixelSnappedY = Mathf.Round(exactPosition.y);

            transform.localPosition = new Vector3(pixelSnappedX, pixelSnappedY, 0f);

            // Self-destruct when bounds clear completely
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        public void Initialize(
            Vector2 start,
            Vector2 end,
            float speed)
        {
            startPos = start;
            endPos = end;
            moveSpeed = speed;

            progress = 0f;

            // Snap the starting position instantly to whole numbers
            transform.localPosition = new Vector3(Mathf.Round(startPos.x), Mathf.Round(startPos.y), 0f);
            transform.localRotation = Quaternion.identity;
        }
        #endregion
    }
}