using Assets.Source.Data;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Runtime.MetaDomain;
using Contract.Enum.MetaDomain.Item;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Inventory
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SlotItem : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
    {
        #region Attributes
        [SerializeField] private Image itemImage;
        [SerializeField] private Image qualityImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Image durabilityBar;
        [SerializeField] private GameObject durabilityContainer;

        [SerializeField] private ItemCatalogSO itemCatalog;
        [SerializeField] private Sprite lowQuality;
        [SerializeField] private Sprite mediumQuality;
        [SerializeField] private Sprite highQuality;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Transform originalParent;
        #endregion

        #region Properties
        public ItemInstanceDTO Item { get; private set; }
        public int SlotIndex { get; private set; }
        #endregion

        #region Methods
        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        public void Bind(
            ItemInstanceDTO item,
            ItemDefinitionDTO definition,
            int slotIndex)
        {
            Item = item;
            SlotIndex = slotIndex;

            if (item == null)
                return;

            if (itemCatalog.TryGet(item.DefinitionID, out var asset))
            {
                itemImage.sprite = asset.icon;

                switch (item.Quality)
                {
                    case ItemQuality.Low: qualityImage.sprite = lowQuality; break;
                    case ItemQuality.Medium: qualityImage.sprite = mediumQuality; break;
                    case ItemQuality.High: qualityImage.sprite = highQuality; break;
                }

                quantityText.text = item.Amount >= 1 ? item.Amount.ToString() : "";

                if (durabilityBar == null) return;

                if (!item.Durability.HasValue ||
                    definition == null ||
                    !definition.MaxDurability.HasValue ||
                    definition.MaxDurability.Value <= 0)
                {
                    SetDurabilityActive(false);
                    return;
                }

                float currentDurability = item.Durability.Value;
                float maxDurability = definition.MaxDurability.Value;

                SetDurabilityActive(true);
                durabilityBar.type = Image.Type.Filled;
                durabilityBar.fillMethod = Image.FillMethod.Horizontal;
                durabilityBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                durabilityBar.fillAmount = Mathf.Clamp01(currentDurability / maxDurability);
            }
        }

        private void SetDurabilityActive(bool active)
        {
            if (durabilityContainer != null)
                durabilityContainer.SetActive(active);
            else if (durabilityBar != null)
                durabilityBar.gameObject.SetActive(active);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                transform.SetParent(canvas.transform, true);
            }
            else
            {
                transform.SetParent(transform.root, true);
            }

            SetPivotWithoutMoving(new Vector2(0.5f, 0.5f));

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (this == null) return;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            // We ALWAYS return to our original physical slot hierarchy.
            // If a valid drop occurred, the Presenter will have already called Bind() 
            // to swap this GameObject's visual data with the target slot's item.
            ReturnToOriginalParent();

            SetPivotWithoutMoving(new Vector2(0f, 0f));
        }

        private void ReturnToOriginalParent()
        {
            if (originalParent != null)
            {
                var slot = originalParent.GetComponentInParent<Slot>();
                if (slot != null)
                {
                    slot.SetItemView(this);
                }
            }
        }

        private void SetPivotWithoutMoving(Vector2 newPivot)
        {
            Vector3 worldPos = rectTransform.position;
            rectTransform.pivot = newPivot;
            rectTransform.position = worldPos;
        }
        #endregion
    }
}