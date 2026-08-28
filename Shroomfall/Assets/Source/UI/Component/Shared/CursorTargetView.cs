using Assets.Enum;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorTarget : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    #region Attributes
    [Header("Cursor Hover Type")]
    [SerializeField] private CursorType hoverType = CursorType.Hover;

    private Action<CursorType> requestCursor;
    #endregion

    #region Properties
    public static event Action<CursorTarget> OnTargetEnabled;
    public static event Action<CursorTarget> OnTargetDisabled;
    #endregion

    #region Methods
    void OnEnable()
    {
        OnTargetEnabled?.Invoke(this);
    }

    void OnDisable()
    {
        OnTargetDisabled?.Invoke(this);
    }

    public void Bind(
        Action<CursorType> request)
    {
        requestCursor = request;
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        requestCursor?.Invoke(hoverType);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        requestCursor?.Invoke(CursorType.Default);
    }

    public void OnPointerUp(
        PointerEventData eventData)
    {
        requestCursor?.Invoke(hoverType);
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        requestCursor?.Invoke(CursorType.Drag);
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        requestCursor?.Invoke(CursorType.Default);
    }
    #endregion
}