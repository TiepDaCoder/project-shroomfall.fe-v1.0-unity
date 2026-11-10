using System;
using UnityEngine;

namespace Assets.Source.UI.Component.Shared
{
    public class SlideToggle : MonoBehaviour
    {
        #region Attributes
        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button onButton;
        [SerializeField] private UnityEngine.UI.Button offButton;

        [Header("Sprites")]
        [SerializeField] private Sprite onPressedSprite;
        [SerializeField] private Sprite offPressedSprite;
        private Sprite onNormalSprite;
        private Sprite offNormalSprite;

        private bool isOn;
        #endregion

        #region Properties
        public event Action<bool> OnValueChanged;
        public bool IsOn => isOn;
        #endregion

        #region Methods
        private void Awake()
        {
            onNormalSprite = onButton.image.sprite;
            offNormalSprite = offButton.image.sprite;

            onButton.onClick.AddListener(() => SetValue(true));
            offButton.onClick.AddListener(() => SetValue(false));

            UpdateButtonSprites();
        }

        public void SetValueWithoutNotify(
            bool value)
        {
            isOn = value;
            UpdateButtonSprites();
        }

        public void SetValue(
            bool value)
        {
            if (isOn == value)
                return;

            isOn = value;
            UpdateButtonSprites();

            OnValueChanged?.Invoke(isOn);
        }

        private void UpdateButtonSprites()
        {
            onButton.image.sprite = isOn ? onPressedSprite : onNormalSprite;
            offButton.image.sprite = isOn ? offNormalSprite : offPressedSprite;
        }
        #endregion
    }
}