using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Component.Scroll
{
    [Serializable]
    public struct ScrollValue
    {
        public string ID;
        public string Name;
    }

    public class ScrollCollector : MonoBehaviour
    {
        #region Attributes
        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button leftButton;
        [SerializeField] private UnityEngine.UI.Button rightButton;

        [Header("Display")]
        [SerializeField] private TMP_Text valueText;

        [Header("Values")]
        [SerializeField] private List<ScrollValue> values = new();

        private bool suppressEvents;
        private int currentIndex = 0;
        #endregion

        #region Properties
        public event Action<string> OnValueChanged;
        #endregion

        #region Methods
        void Awake()
        {
            leftButton.onClick.AddListener(ScrollLeft);
            rightButton.onClick.AddListener(ScrollRight);
        }

        public void SetCurrentByID(string id)
        {
            if (values == null || values.Count == 0) return;

            int index = values.FindIndex(v => v.ID == id);
            if (index < 0) index = 0;

            suppressEvents = true;
            currentIndex = index;
            valueText.text = values[currentIndex].Name;
            suppressEvents = false;
        }

        public void SetValues(List<ScrollValue> newValues, int startIndex = 0)
        {
            suppressEvents = true;

            values = newValues;
            currentIndex = Mathf.Clamp(startIndex, 0, values.Count - 1);
            if (values == null || values.Count == 0) return;

            var entry = values[currentIndex];
            valueText.text = entry.Name;

            suppressEvents = false;
        }

        public string GetCurrentID()
        {
            return values != null && values.Count > 0
                ? values[currentIndex].ID
                : string.Empty;
        }

        public void SetInteractable(
            bool interactable)
        {
            leftButton.interactable = interactable;
            rightButton.interactable = interactable;
        }

        private void ScrollLeft()
        {
            if (values == null || values.Count == 0) return;

            currentIndex = (currentIndex - 1 + values.Count) % values.Count;
            UpdateDisplay();
        }

        private void ScrollRight()
        {
            if (values == null || values.Count == 0) return;

            currentIndex = (currentIndex + 1) % values.Count;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (values == null || values.Count == 0)
            {
                valueText.text = string.Empty;
                return;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, values.Count - 1);
            var entry = values[currentIndex];
            valueText.text = entry.Name;

            if (!suppressEvents)
                OnValueChanged?.Invoke(entry.ID);
        }
        #endregion
    }
}