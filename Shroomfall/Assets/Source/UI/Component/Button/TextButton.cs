using TMPro;

namespace Assets.Source.UI.Component.Button
{
    public class TextButton : UnityEngine.UI.Button
    {
        #region Attributes
        private TMP_Text label;
        #endregion

        #region Properties
        #endregion

        #region Methods
        protected override void Awake()
        {
            base.Awake();

            label = GetComponentInChildren<TMP_Text>();
        }

        public void SetText(
            string value)
        {
            if (label == null) return;
            label.text = value;
        }
        #endregion
    }
}