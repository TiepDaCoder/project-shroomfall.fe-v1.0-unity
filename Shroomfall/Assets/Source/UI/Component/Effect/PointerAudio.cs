using Assets.Source.Core;
using Assets.Source.Service;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Source.UI.Component.Effect
{
    public class PointerAudio : MonoBehaviour,
        IPointerEnterHandler,
        IPointerDownHandler
    {
        #region Attributes
        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip pressedSound;

        private AudioService audioService;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            audioService = ServiceProvider.Get<AudioService>();
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            audioService.PlayUI(hoverSound);
        }

        public void OnPointerDown(
            PointerEventData eventData)
        {
            audioService.PlayUI(pressedSound);
        }
        #endregion
    }
}