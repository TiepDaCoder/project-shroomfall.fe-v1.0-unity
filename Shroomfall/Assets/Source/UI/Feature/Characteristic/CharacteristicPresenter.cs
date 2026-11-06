using Assets.Source.Service;
using Contract.DTO.Runtime.EntityDomain.Component;
using Contract.Enum.MetaDomain.Effect;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Source.UI.Feature.Characteristic
{
    public class CharacteristicPresenter
    {
        #region Attributes
        private readonly PlayerService playerService;
        private readonly CharacteristicView characteristicView;

        private bool disposed;
        #endregion

        #region Properties
        #endregion

        public CharacteristicPresenter(
            PlayerService playerService,
            CharacteristicView characteristicView)
        {
            this.playerService = playerService;
            this.characteristicView = characteristicView;

            Bind();
            OnCharacteristicSyncHandler();
        }

        #region Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            // Outbound
            playerService.OnCharacteristicSync -= OnCharacteristicSyncHandler;
        }

        private void Bind()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(CharacteristicPresenter));

            // Outbound
            playerService.OnCharacteristicSync += OnCharacteristicSyncHandler;
        }

        private void OnCharacteristicSyncHandler(CharacteristicInstanceDTO dto = null)
        {
            var characteristicComp = dto ?? playerService.Runtime?.Components
                .OfType<CharacteristicInstanceDTO>()
                .FirstOrDefault();

            if (characteristicComp == null)
                return;

            var attributeMap = new Dictionary<AttributeType, float>();

            if (characteristicComp.Cores != null)
            {
                foreach (var attr in characteristicComp.Cores)
                {
                    attributeMap[attr.AttributeType] = attr.Value;
                }
            }

            if (characteristicComp.Vitals != null)
            {
                foreach (var attr in characteristicComp.Vitals)
                {
                    attributeMap[attr.AttributeType] = attr.Value;
                }
            }

            characteristicView.UpdateAllAttributes(attributeMap);
        }
        #endregion
    }
}