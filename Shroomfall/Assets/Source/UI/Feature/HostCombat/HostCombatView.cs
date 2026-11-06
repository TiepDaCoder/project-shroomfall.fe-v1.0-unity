using Assets.Source.UI.Component.Button;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HostCombat
{
    public class HostCombatView : MonoBehaviour
    {
        #region Attributes
        [Header("Buttons")]
        [SerializeField] private Button backButton;

        [Header("Selectors")]
        [SerializeField] private Transform selectionRoot;
        [SerializeField] private ImageButton selectionPrefab;

        [Header("Label")]
        [SerializeField] private TMP_Text viewLabel;

        private readonly List<ImageButton> selections = new();
        #endregion

        #region Properties
        public event Action OnBackClicked;
        public event Action<string> OnCreateClicked;
        public event Action OnOpened;
        #endregion

        #region Methods
        private void Awake()
        {
            // Buttons
            backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
        }

        private void OnEnable()
        {
            OnOpened?.Invoke();
        }

        public void SetVisible(
            bool visible)
        {
            gameObject.SetActive(visible);
            if (visible) RefreshLocalizedText();
        }

        public void BindCombatIcons(
            IReadOnlyList<IconModel> models)
        {
            selections.Clear();

            foreach (Transform child in selectionRoot)
                Destroy(child.gameObject);

            foreach (var model in models)
            {
                var selection = Instantiate(selectionPrefab, selectionRoot);

                selection.Bind(model);
                selection.OnClicked += id => OnCreateClicked?.Invoke(id);

                selections.Add(selection);
            }
        }

        public void SetInteractable(
            bool interactable)
        {
            backButton.interactable = interactable;

            foreach (var selection in selections)
            {
                selection.SetInteractable(interactable);
            }
        }

        private void RefreshLocalizedText()
        {
            // Buttons
            viewLabel.SetText(UILocalizationTable.Get("host-combat.label-host"));
        }
        #endregion
    }
}