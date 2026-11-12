using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Enum;
using Assets.Source.Service;
using Assets.Source.UI.Component.Customize;
using Assets.Source.UI.Component.Customize.HSVCollector;
using Assets.Source.UI.Component.Shared;
using Assets.Source.UI.Model;
using Assets.Source.Utility;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class CustomizeView : MonoBehaviour, IHUDView
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Buttons")]
        [SerializeField] private Button submitButton;
        [SerializeField] private Button skinToLeftButton;
        [SerializeField] private Button skinToRightButton;

        [Header("Scrolls")]
        [SerializeField] private Scroll skinScroll;

        [Header("Colors")]
        [SerializeField] private HSVCollector skinColorCollector;

        [Header("Text Fields")]
        [SerializeField] private TMP_InputField nameTextField;
        [SerializeField] private TMP_InputField dobDayTextField;
        [SerializeField] private TMP_InputField dobMonthTextField;
        [SerializeField] private TMP_InputField dobYearTextField;

        [Header("Preview")]
        [SerializeField] private EntityPreviewView preview;

        [Header("Data")]
        [SerializeField] private EntityCatalogSO entityCatalog;

        private UIService uiService;
        private PlayerService playerService;
        private AuthService authService;
        private DefinitionService definitionService;

        private EntityDirection direction = EntityDirection.DOWN;
        private CharacterAppearanceModel currentAppearance;

        private string profileName;
        private string dobDayStr;
        private string dobMonthStr;
        private string dobYearStr;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            uiService = ServiceProvider.Get<UIService>();
            playerService = ServiceProvider.Get<PlayerService>();
            authService = ServiceProvider.Get<AuthService>();
            definitionService = ServiceProvider.Get<DefinitionService>();

            // UI Service
            uiService.OnGlobalInteractableChanged += SetInteractable;

            InitializeSkinValues();

            // Buttons
            submitButton.onClick.AddListener(OnSubmitClicked);
            skinToLeftButton.onClick.AddListener(OnSkinToLeftClicked);
            skinToRightButton.onClick.AddListener(OnSkinToRightClicked);

            // Scrolls
            skinScroll.OnValueChanged += OnSkinChanged;

            // Colors
            skinColorCollector.OnColorChanged += OnSkinColorChanged;

            // Text Fields
            nameTextField.onValueChanged.AddListener(OnNameChanged);
            dobDayTextField.onValueChanged.AddListener(OnDobDayChanged);
            dobMonthTextField.onValueChanged.AddListener(OnDobMonthChanged);
            dobYearTextField.onValueChanged.AddListener(OnDobYearChanged);

            root.SetActive(false);
        }

        private void OnDestroy()
        {
            // UI Service
            uiService.OnGlobalInteractableChanged -= SetInteractable;

            // Buttons
            submitButton.onClick.RemoveListener(OnSubmitClicked);
            skinToLeftButton.onClick.RemoveListener(OnSkinToLeftClicked);
            skinToRightButton.onClick.RemoveListener(OnSkinToRightClicked);

            // Scrolls
            skinScroll.OnValueChanged -= OnSkinChanged;

            // Colors
            skinColorCollector.OnColorChanged -= OnSkinColorChanged;

            // Text Fields
            nameTextField.onValueChanged.RemoveListener(OnNameChanged);
            dobDayTextField.onValueChanged.RemoveListener(OnDobDayChanged);
            dobMonthTextField.onValueChanged.RemoveListener(OnDobMonthChanged);
            dobYearTextField.onValueChanged.RemoveListener(OnDobYearChanged);
        }

        #region HUD Action
        public void Show()
        {
            root.SetActive(true);
            CurrentAppearanceSync();
        }

        public void Hide()
        {
            root.SetActive(false);
        }
        #endregion

        #region Initialization
        private void InitializeSkinValues()
        {
            var values = new List<ScrollValue>();

            foreach (var entity in entityCatalog.GetAll())
            {
                var entityDef = definitionService.GetEntityDefinition(entity.id);
                if (entityDef == null)
                    continue;

                if (entityDef.Type != EntityType.Player)
                    continue;

                values.Add(new ScrollValue
                {
                    ID = entity.id,
                    Name = entity.id
                });
            }

            skinScroll.SetValues(values);
        }
        #endregion

        public void SetInteractable(
            bool interactable)
        {
            submitButton.interactable = interactable;
        }

        private void OnSubmitClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                Task updateProfileTask = Task.CompletedTask;
                if (!string.IsNullOrWhiteSpace(profileName) && TryGetValidDateOfBirth(out DateTime dob))
                    updateProfileTask = authService.UpdateProfile(profileName, dob);

                Task updateAppearanceTask = playerService.UpdateAppearanceAsync(currentAppearance.SkinID, currentAppearance.SkinColor);
                await Task.WhenAll(updateProfileTask, updateAppearanceTask);
                uiService.ShowToast(ToastType.Information, "Character updated successfully!");
            });
        }

        #region Appearance
        private void OnSkinToLeftClicked()
        {
            RotateDirection(-1);
        }

        private void OnSkinToRightClicked()
        {
            RotateDirection(+1);
        }

        private void OnSkinChanged(
            string id)
        {
            if (currentAppearance == null)
                return;

            currentAppearance.SkinID = id;
            RefreshPreview();
        }

        private void OnSkinColorChanged(
            Color color)
        {
            if (currentAppearance == null)
                return;

            currentAppearance.SkinColor = color;
            RefreshPreview();
        }

        private void RotateDirection(
            int delta)
        {
            const int directionCount = 4;

            int value = ((int)direction + delta) % directionCount;
            if (value < 0)
                value += directionCount;

            direction = (EntityDirection)value;

            RefreshPreview();
        }

        private void CurrentAppearanceSync()
        {
            var appearance = playerService.Runtime?.Components.OfType<AppearanceInstanceDTO>().FirstOrDefault();
            if (appearance == null)
                return;

            currentAppearance = new CharacterAppearanceModel
            {
                SkinID = appearance.SkinID,
                SkinColor = ColorHelper.ToColor(appearance.SkinColor)
            };
            skinScroll.SetCurrentByID(currentAppearance.SkinID);
            skinColorCollector.SetCurrentByColor(currentAppearance.SkinColor);

            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (currentAppearance == null)
                return;

            if (entityCatalog.TryGet(currentAppearance.SkinID, out var entity))
                preview.Apply(entity, currentAppearance.SkinColor, direction);
        }
        #endregion

        #region Profile
        private void OnNameChanged(
            string value)
        {
            profileName = value;
        }

        private void OnDobDayChanged(
            string value)
        {
            dobDayStr = value;
        }

        private void OnDobMonthChanged(
            string value)
        {
            dobMonthStr = value;
        }

        private void OnDobYearChanged(
            string value)
        {
            dobYearStr = value;
        }

        private bool TryGetValidDateOfBirth(
            out DateTime result)
        {
            result = default;

            if (!int.TryParse(dobDayStr, out int day) ||
                !int.TryParse(dobMonthStr, out int month) ||
                !int.TryParse(dobYearStr, out int year))
            {
                return false;
            }

            try
            {
                result = new DateTime(year, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
        #endregion
        #endregion
    }
}