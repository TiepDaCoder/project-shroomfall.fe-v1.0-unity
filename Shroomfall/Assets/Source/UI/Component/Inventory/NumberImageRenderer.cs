using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Inventory
{
    public class NumberImageRenderer : MonoBehaviour
    {
        #region Attributes
        [Header("Digit Sprites (0-9)")]
        [SerializeField] private Sprite[] digitSprites = new Sprite[10];

        [Header("Digit Images")]
        [SerializeField] private Image hundredsImage;
        [SerializeField] private Image tensImage;
        [SerializeField] private Image onesImage;
        #endregion

        #region Properties
        #endregion

        #region Methods
        public void SetNumber(
            int number)
        {
            if (number < 0 || number > 999)
            {
                Clear();
                return;
            }

            // Extract digits
            int hundreds = number / 100;
            int tens = (number / 10) % 10;
            int ones = number % 10;

            // Render digits
            SetDigit(hundredsImage, hundreds, number >= 100);
            SetDigit(tensImage, tens, number >= 10);
            SetDigit(onesImage, ones, true);
        }

        private void SetDigit(
            Image image,
            int digit,
            bool visible)
        {
            if (image == null)
                return;

            image.gameObject.SetActive(visible);
            if (!visible)
                return;

            if (digitSprites == null || digitSprites.Length < 10 || digitSprites[digit] == null)
            {
                image.sprite = null;
                return;
            }

            image.sprite = digitSprites[digit];
        }

        public void Clear()
        {
            if (hundredsImage != null)
                hundredsImage.gameObject.SetActive(false);

            if (tensImage != null)
                tensImage.gameObject.SetActive(false);

            if (onesImage != null)
                onesImage.gameObject.SetActive(false);
        }
        #endregion
    }
}