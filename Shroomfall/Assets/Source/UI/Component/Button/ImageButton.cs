using Assets.UI.Models;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    #region Attributes
    [SerializeField] private Image icon;
    [SerializeField] private ButtonHoverFX hoverFX;

    private bool interactable = true;
    #endregion

    #region Properties
    public string Id { get; private set; }

    public event Action<string> OnClicked;
    #endregion

    #region Methods
    public void Bind(IconModel model)
    {
        Id = model.Id;
        icon.sprite = model.Icon;

        if (icon.sprite != null)
            icon.sprite.texture.filterMode = FilterMode.Point;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable)
            return;

        hoverFX.Hover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverFX.Hover(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
            return;

        OnClicked?.Invoke(Id);
    }

    public void SetInteractable(bool value)
    {
        interactable = value;
    }
    #endregion
}