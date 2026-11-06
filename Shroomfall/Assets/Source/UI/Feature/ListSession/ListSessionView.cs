using Assets.Source.Data;
using Assets.Source.Utility;
using Assets.Source.UI.Model;
using Contract.DTO.Feature.Connection.Response;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Source.UI.Component.Button;
using Assets.Source.UI.Component.Shared;

namespace Assets.Source.UI.Feature.ListSession
{
    public class SessionEntry : MonoBehaviour
    {
        #region Attributes
        [Header("Preview")]
        [SerializeField] private EntityPreviewView entityPreviewView;

        [Header("Texts")]
        [SerializeField] private TMP_Text sessionIdText;

        [Header("Buttons")]
        [SerializeField] private TextButton loadButton;

        private string sessionId;
        #endregion

        #region Properties
        public event Action<string> OnLoadClicked;
        #endregion

        #region Methods
        private void Awake()
        {
            // Buttons
            loadButton.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(sessionId))
                    OnLoadClicked?.Invoke(sessionId);
            });
        }

        public void Bind(
            SessionEntryModel model,
            int index)
        {
            sessionId = model.SessionId;
            sessionIdText.text = ShortenId(model.SessionId);
            entityPreviewView.Apply(model.Entity, model.SkinColor, EntityDirection.DOWN);
            RefreshLocalizedText();
        }

        public void SetInteractable(
            bool interactable)
        {
            loadButton.interactable = interactable;
        }

        private void RefreshLocalizedText()
        {
            // Buttons
            loadButton.SetText(UILocalizationTable.Get("session-entry.btn-load"));
        }

        private string ShortenId(
            string value,
            int maxLength = 12)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length <= maxLength)
                return value;

            return $"{value[..maxLength]}...";
        }
        #endregion
    }

    public class ListSessionView : MonoBehaviour
    {
        #region Attributes
        [Header("Prefab")]
        [SerializeField] private SessionEntry entryPrefab;

        [Header("Container")]
        [SerializeField] private Transform contentRoot;

        [Header("Button")]
        [SerializeField] private Button backButton;
        [SerializeField] private TextButton createButton;

        [Header("Label")]
        [SerializeField] private TMP_Text viewLabel;

        [Header("Assets")]
        [SerializeField] private EntityCatalogSO entityCatalog;
        #endregion

        #region Properties
        public event Action OnBackClicked;
        public event Action OnCreateClicked;
        public event Action<string> OnLoadClicked;
        #endregion

        #region Methods
        private void Awake()
        {
            // Buttons
            backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
            createButton.onClick.AddListener(() => { OnCreateClicked?.Invoke(); });
        }

        public void SetVisible(
            bool visible)
        {
            gameObject.SetActive(visible);
            if (visible) RefreshLocalizedText();
        }

        public void SetSessions(
            List<ExistedSessionEntryDTO> dto)
        {
            var models = dto.Select(MapToViewModel).ToList();

            // Clear old list
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                DestroyImmediate(contentRoot.GetChild(i).gameObject);

            // Instantiate new items
            for (int i = 0; i < models.Count; i++)
            {
                var entry = Instantiate(entryPrefab, contentRoot);
                entry.OnLoadClicked += (id) => OnLoadClicked?.Invoke(id);

                entry.Bind(models[i], i);
            }
        }

        public void SetInteractable(
            bool interactable)
        {
            for (int i = 0; i < contentRoot.childCount; i++)
            {
                SessionEntry entry = contentRoot.GetChild(i).GetComponent<SessionEntry>();

                if (entry != null)
                    entry.SetInteractable(interactable);
            }
        }

        private SessionEntryModel MapToViewModel(
            ExistedSessionEntryDTO dto)
        {
            var entry = new SessionEntryModel
            {
                SessionId = dto.PlayerInstanceID,
                SkinColor = ColorHelper.ToColor(dto.PlayerAppearance.SkinColor),
            };

            if (entityCatalog.TryGet(dto.PlayerAppearance.SkinID, out var entityAsset))
            {
                entry.Entity = entityAsset;
            }

            return entry;
        }

        private void RefreshLocalizedText()
        {
            // Buttons
            viewLabel.SetText(UILocalizationTable.Get("session-list.label-list"));
            createButton.SetText(UILocalizationTable.Get("session-list.btn-create"));
        }
        #endregion
    }
}