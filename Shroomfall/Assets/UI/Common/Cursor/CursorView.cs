using Assets.Enums;
using UnityEngine;

public class CursorView : MonoBehaviour
{
    #region Attributes
    [Header("Cursor Textures")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D clickCursor;
    [SerializeField] private Texture2D dragCursor;
    [SerializeField] private Texture2D textCursor;

    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private static CursorView instance;
    #endregion

    #region Properties
    #endregion

    #region Methods
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Apply(
        CursorType type)
    {
        Texture2D texture = type switch
        {
            CursorType.Hover => hoverCursor,
            CursorType.Click => clickCursor,
            CursorType.Drag => dragCursor,
            CursorType.Text => textCursor,
            _ => defaultCursor
        };

        Cursor.SetCursor(texture, hotSpot, CursorMode.Auto);
    }
    #endregion
}