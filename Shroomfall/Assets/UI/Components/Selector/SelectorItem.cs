using Assets.UI.Models;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SelectorItem : MonoBehaviour
{
    #region Attributes
    [Header("Button Components")]
    [SerializeField] private Image icon;
    [SerializeField] private Button button;

    [Header("Visual States")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    #endregion

    #region Properties
    public event Action OnSelect;
    public bool IsSelected { get; private set; }
    public IconModel Model { get; private set; }
    #endregion

    #region Methods
    public void Bind(
        IconModel model)
    {
        Model = model;
        icon.sprite = model.Icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSelect?.Invoke());

        SetSelected(false);
    }

    public void SetSelected(
        bool state)
    {
        IsSelected = state;
        if (button.image != null)
        {
            button.image.sprite = state ? selectedSprite : normalSprite;
        }
    }
    #endregion
}