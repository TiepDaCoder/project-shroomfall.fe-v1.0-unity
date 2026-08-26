using Assets.Enums;
using Assets.Services;
using Assets.Utilities;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.EntityDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.UI.Features.Customize
{
    public class CharacterAppearancePreview
    {
        public string SkinID;
        public Color SkinColor;

        public CharacterAppearancePreview Clone()
        {
            return (CharacterAppearancePreview)MemberwiseClone();
        }
    }

    public class CustomizePresenter
    {
        #region Attributes
        private readonly UIService uiService;
        private readonly PlayerService playerService;
        private readonly AuthService authService;
        private readonly CustomizeView customizeView;

        private readonly EntityCatalogSO entityCatalog;

        private EntityDirection direction = EntityDirection.DOWN;
        private CharacterAppearancePreview preview;
        private CharacterAppearancePreview original;
        private string profileName;
        private string dobDayStr;
        private string dobMonthStr;
        private string dobYearStr;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public CustomizePresenter(
            UIService uiService,
            PlayerService playerService,
            AuthService authService,
            CustomizeView customizeView,
            EntityCatalogSO entityCatalog)
        {
            this.uiService = uiService;
            this.playerService = playerService;
            this.authService = authService;
            this.customizeView = customizeView;
            this.entityCatalog = entityCatalog;

            Bind();

            customizeView.SetSkinValues(BuildScrollValues(entityCatalog));
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            uiService.OnGlobalInteractableChanged -= customizeView.SetInteractable;

            // Inbound
            customizeView.OnSubmitClicked -= OnSubmitClicked;
            customizeView.OnViewShown -= OnViewShown;
            customizeView.OnSkinToLeftClicked -= OnSkinToLeftClicked;
            customizeView.OnSkinToRightClicked -= OnSkinToRightClicked;

            customizeView.OnSkinChanged -= OnSkinChanged;
            customizeView.OnSkinColorChanged -= OnSkinColorChanged;

            // Profile Input Events
            customizeView.OnNameChanged -= OnNameChanged;
            customizeView.OnDobDayChanged -= OnDobDayChanged;
            customizeView.OnDobMonthChanged -= OnDobMonthChanged;
            customizeView.OnDobYearChanged -= OnDobYearChanged;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CustomizePresenter));

            // Outbound
            uiService.OnGlobalInteractableChanged += customizeView.SetInteractable;

            // Inbound
            customizeView.OnSubmitClicked += OnSubmitClicked;
            customizeView.OnSkinToLeftClicked += OnSkinToLeftClicked;
            customizeView.OnSkinToRightClicked += OnSkinToRightClicked;

            customizeView.OnSkinChanged += OnSkinChanged;

            customizeView.OnSkinColorChanged += OnSkinColorChanged;

            customizeView.OnNameChanged += OnNameChanged;
            customizeView.OnDobDayChanged += OnDobDayChanged;
            customizeView.OnDobMonthChanged += OnDobMonthChanged;
            customizeView.OnDobYearChanged += OnDobYearChanged;

            customizeView.OnViewShown += OnViewShown;
        }

        private void OnSubmitClicked()
        {
            AsyncHelper.Run(uiService, async () =>
            {
                // Default to a completed task if profile data is empty/invalid
                Task updateProfileTask = Task.CompletedTask;

                // Update information if valid profile data is provided
                if (!string.IsNullOrWhiteSpace(profileName) && TryGetValidDateOfBirth(out DateTime dob))
                {
                    updateProfileTask = authService.UpdateProfile(profileName, dob);
                }

                Task updateAppearanceTask = playerService.UpdateAppearanceAsync(preview.SkinID, preview.SkinColor);

                await Task.WhenAll(updateProfileTask, updateAppearanceTask);

                uiService.ShowToast(ToastType.Information, "Character updated successfully!");
            });
        }

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
            if (preview == null) return;
            preview.SkinID = id;
            RefreshPreview();
        }

        private void OnSkinColorChanged(
            Color color)
        {
            if (preview == null) return;
            preview.SkinColor = color;
            RefreshPreview();
        }

        private void OnNameChanged(
            string name)
        {
            profileName = name;
        }

        private void OnDobDayChanged(
            string day)
        {
            dobDayStr = day;
        }

        private void OnDobMonthChanged(
            string month)
        {
            dobMonthStr = month;
        }

        private void OnDobYearChanged(
            string year)
        {
            dobYearStr = year;
        }

        private void OnViewShown()
        {
            var appearance = playerService.Runtime.Components
                .OfType<AppearanceInstanceDTO>()
                .FirstOrDefault();

            original = FromAppearance(appearance);
            preview = original.Clone();

            customizeView.ApplyCurrentSelection(
                appearance.SkinID,
                ColorHelper.ToColor(appearance.SkinColor));

            RefreshPreview();
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

        private void RotateDirection(
            int delta)
        {
            int count = 4;
            int value = ((int)direction + delta) % count;
            if (value < 0) value += count;

            direction = (EntityDirection)value;
            RefreshPreview();
        }

        private List<ScrollValue> BuildScrollValues(
            EntityCatalogSO catalog)
        {
            var list = new List<ScrollValue>();

            foreach (var part in catalog.GetAll())
            {
                list.Add(new ScrollValue
                {
                    ID = part.id,
                    Name = part.id,
                });
            }

            return list;
        }

        private CharacterAppearancePreview FromAppearance(
            AppearanceInstanceDTO appearance)
        {
            return new CharacterAppearancePreview
            {
                SkinID = appearance.SkinID,
                SkinColor = ColorHelper.ToColor(appearance.SkinColor)
            };
        }

        private void RefreshPreview()
        {
            if (preview == null) return;

            if (entityCatalog.TryGet(preview.SkinID, out var entity))
            {
                customizeView.SetSkinPreview(entity, preview.SkinColor, direction);
            }
        }
        #endregion
    }
}