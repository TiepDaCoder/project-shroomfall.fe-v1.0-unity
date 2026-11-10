using Assets.Source.Core;
using Assets.Source.Service;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Shared
{
    public class DiscreteSlider :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Image[] segments;
        [SerializeField] private RectTransform selectionBar;
        [SerializeField] private RectTransform segmentContainer;

        [Header("Appearance")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Value")]
        [SerializeField, Range(0, 1)] private float value = 1f;

        [Header("Audio")]
        [SerializeField] private AudioClip changeSound;

        private AudioService audioService;
        #endregion

        #region Properties
        public float Value => value;
        public event Action<float> OnValueChanged;
        #endregion

        #region Methods
        private void Awake()
        {
            audioService = ServiceProvider.Get<AudioService>();
        }

        private void Start()
        {
            UpdateSelection(value);
        }

        public void OnPointerDown(
            PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        public void OnDrag(
            PointerEventData eventData)
        {
            UpdateFromPointer(eventData);
        }

        private void UpdateFromPointer(
            PointerEventData eventData)
        {
            if (segments == null || segments.Length == 0)
                return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(segmentContainer, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
                return;

            float normalized = Mathf.InverseLerp(segmentContainer.rect.xMin, segmentContainer.rect.xMax, localPoint.x);
            int index = Mathf.RoundToInt(normalized * (segments.Length - 1));
            SetValue(index / (float)(segments.Length - 1));
        }

        public void SetValue(
            float newValue)
        {
            SetValueInternal(newValue, true);
        }

        public void SetValueWithoutNotify(
            float newValue)
        {
            SetValueInternal(newValue, false);
        }

        private void SetValueInternal(
            float newValue,
            bool notify)
        {
            newValue = Mathf.Clamp01(newValue);
            if (segments == null || segments.Length == 0)
                return;

            int index = segments.Length == 1 ? 0 : Mathf.RoundToInt(newValue * (segments.Length - 1));
            float snappedValue = segments.Length == 1 ? 1f : index / (float)(segments.Length - 1);
            bool changed = !Mathf.Approximately(value, snappedValue);
            value = snappedValue;
            UpdateSelection(value);

            if (changed && notify)
            {
                audioService?.PlayUI(changeSound);
                OnValueChanged?.Invoke(value);
            }
        }


        private void UpdateSelection(
            float normalizedValue)
        {
            if (segments == null || segments.Length == 0)
                return;

            int selectedIndex = segments.Length == 1 ? 0 : Mathf.RoundToInt(normalizedValue * (segments.Length - 1));
            for (int i = 0; i < segments.Length; i++)
                if (segments[i] != null)
                    segments[i].color = i <= selectedIndex ? activeColor : inactiveColor;

            if (selectionBar != null && segments[selectedIndex] != null)
                selectionBar.position = segments[selectedIndex].rectTransform.position;
        }
        #endregion
    }
}