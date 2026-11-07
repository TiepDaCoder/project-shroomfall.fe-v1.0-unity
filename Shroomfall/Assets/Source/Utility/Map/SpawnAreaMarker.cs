using Contract.Enum.WorldDomain;
using TMPro;
using UnityEngine;

namespace Assets.Source.Utility.Map
{
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider2D))]
    public class SpawnAreaMarker : MonoBehaviour
    {
        #region Attributes
        [Header("Spawn Rule")]
        public string EntityID;
        public SpawnRuleType RuleType;
        public int MinCount = 1;
        public int MaxCount = 1;
        public float Weight = 1;

        [Header("Visual")]
        [SerializeField]
        private Color fillColor = new Color(0f, 1f, 0f, 0.25f);

        [SerializeField]
        private Color textColor = Color.white;

        [SerializeField]
        private int sortingOrder = 10;

        [SerializeField]
        private float textSize = 2f;

        private BoxCollider2D boxCollider;
        private SpriteRenderer fillRenderer;
        private TextMeshPro label;

        private static Sprite whiteSprite;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void OnEnable()
        {
            Initialize();
            RefreshVisuals();
        }

        private void OnValidate()
        {
            Initialize();
            RefreshVisuals();
        }

        private void Initialize()
        {
            boxCollider = GetComponent<BoxCollider2D>();

            if (whiteSprite == null)
            {
                whiteSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 1, 1),
                    new Vector2(0.5f, 0.5f));
            }

            // Fill
            Transform fillTransform = transform.Find("Fill");

            if (fillTransform == null)
            {
                GameObject fillObject = new GameObject("Fill");
                fillTransform = fillObject.transform;
                fillTransform.SetParent(transform, false);
            }

            fillRenderer = fillTransform.GetComponent<SpriteRenderer>();

            if (fillRenderer == null)
                fillRenderer = fillTransform.gameObject.AddComponent<SpriteRenderer>();

            // Label
            Transform labelTransform = transform.Find("Label");

            if (labelTransform == null)
            {
                GameObject labelObject = new GameObject("Label");
                labelTransform = labelObject.transform;
                labelTransform.SetParent(transform, false);
            }

            label = labelTransform.GetComponent<TextMeshPro>();

            if (label == null)
                label = labelTransform.gameObject.AddComponent<TextMeshPro>();
        }

        private void RefreshVisuals()
        {
            if (boxCollider == null ||
                fillRenderer == null ||
                label == null)
                return;

            Vector2 size = boxCollider.size;
            Vector2 offset = boxCollider.offset;

            // Fill
            fillRenderer.sprite = whiteSprite;
            fillRenderer.color = fillColor;
            fillRenderer.sortingOrder = sortingOrder;

            fillRenderer.transform.localPosition = new Vector3(
                offset.x,
                offset.y,
                0f);

            fillRenderer.transform.localRotation = Quaternion.identity;

            fillRenderer.transform.localScale = new Vector3(
                size.x,
                size.y,
                1f);

            // Label
            label.text = EntityID;
            label.color = textColor;
            label.fontSize = textSize;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            label.sortingOrder = sortingOrder + 1;

            label.rectTransform.localPosition = new Vector3(
                offset.x,
                offset.y,
                -0.1f);

            label.rectTransform.localRotation = Quaternion.identity;
            label.rectTransform.localScale = Vector3.one;

            label.rectTransform.sizeDelta = size;
        }
        #endregion
    }
}