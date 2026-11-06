using Assets.Source.Utility;
using System;
using UnityEngine;

namespace Assets.Source.UI.Component.HSV
{
    public class HSVCollector : MonoBehaviour
    {
        #region Attributes
        [SerializeField] private BarDrag H;
        [SerializeField] private BarDrag S;
        [SerializeField] private BarDrag V;

        private bool suppressEvents;
        #endregion

        #region Properties
        public event Action<Color> OnColorChanged;
        #endregion

        #region Methods
        void Awake()
        {
            H.OnValueChanged += UpdateAll;
            S.OnValueChanged += UpdateAll;
            V.OnValueChanged += UpdateAll;
        }

        void Start()
        {

        }

        public void SetCurrentByColor(
            Color color)
        {
            suppressEvents = true;

            var dto = ColorHelper.ToHSV(color);
            H.SetValue(dto.H);
            S.SetValue(dto.S);
            V.SetValue(dto.V);

            UpdateGradientsOnly();

            suppressEvents = false;

            EmitColor(); // single, controlled emit
        }

        void UpdateAll()
        {
            if (suppressEvents) return;
            EmitColor();
        }

        private void EmitColor()
        {
            Color color = Color.HSVToRGB(H.Value, S.Value, V.Value);
            UpdateGradientsOnly();
            OnColorChanged?.Invoke(color);
        }

        private void UpdateGradientsOnly()
        {
            H.UpdateGradient(H.Value, S.Value, V.Value);
            S.UpdateGradient(H.Value, S.Value, V.Value);
            V.UpdateGradient(H.Value, S.Value, V.Value);
        }
        #endregion
    }
}