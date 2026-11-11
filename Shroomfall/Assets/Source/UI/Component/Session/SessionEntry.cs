using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Shared;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using Contract.Enum.EntityDomain;
using TMPro;
using UnityEngine;

namespace Assets.Source.UI.Component.Session
{
    public class SessionEntry : MonoBehaviour
    {
        #region Attributes
        [Header("Preview")]
        [SerializeField] private EntityPreviewView entityPreviewView;

        [Header("Texts")]
        [SerializeField] private TMP_Text sessionIdText;

        [Header("Buttons")]
        [SerializeField] private UnityEngine.UI.Button loadButton;

        private UIService uiService;
        private SessionService sessionService;

        private string sessionId;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            sessionService = ServiceProvider.Get<SessionService>();

            // Buttons
            loadButton.onClick.AddListener(OnLoadClicked);
        }

        private void OnLoadClicked()
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            AsyncHelper.Run(uiService, async () =>
            {
                await sessionService.LoadSession(sessionId);
            });
        }

        public void Bind(
            SessionEntryModel model)
        {
            sessionId = model.SessionId;
            sessionIdText.text = ShortenId(model.SessionId);
            entityPreviewView.Apply(model.Entity, model.SkinColor, EntityDirection.DOWN);
        }

        public void SetInteractable(
            bool interactable)
        {
            loadButton.interactable = interactable;
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
}