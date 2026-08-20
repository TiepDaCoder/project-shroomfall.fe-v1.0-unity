using Assets.Enums;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BarDrag :
    MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    #region Attributes
    [SerializeField] private HSVChannel channel;
    [SerializeField] private RectTransform bar;
    [SerializeField] private RawImage barImage;
    [SerializeField] private RectTransform target;

    private const int GRADIENT_WIDTH = 18;
    private const int GRADIENT_HEIGHT = 7;
    #endregion

    #region Properties
    public float Value { get; private set; }

    public event Action OnValueChanged;
    #endregion

    #region Methods
    private void Awake()
    {
        UpdateGradient(0f, 1f, 1f);
    }

    public void OnPointerDown(
        PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    private void UpdateDrag(
        PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bar,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        float width = bar.rect.width;
        float x = Mathf.Clamp(localPoint.x, -width / 2f, width / 2f);

        target.localPosition = new Vector3(x, target.localPosition.y, 0f);

        float newValue = (x + width / 2f) / width;

        if (!Mathf.Approximately(Value, newValue))
        {
            Value = newValue;
            OnValueChanged?.Invoke();
        }
    }

    public void UpdateGradient(
        float h, float s, float v)
    {
        barImage.texture = channel switch
        {
            HSVChannel.Hue => GenerateHue(),
            HSVChannel.Saturation => GenerateSaturation(h, v),
            HSVChannel.Value => GenerateValue(h, s),
            _ => barImage.texture
        };
    }

    public void SetValue(
        float value)
    {
        Value = Mathf.Clamp01(value);

        float width = bar.rect.width;
        float x = Mathf.Lerp(-width / 2f, width / 2f, Value);

        target.localPosition = new Vector3(x, target.localPosition.y, 0f);
    }
    #endregion

    #region Private Helpers
    private Texture2D GenerateHue()
    {
        return CreateGradient(t => Color.HSVToRGB(t, 1f, 1f));
    }

    private Texture2D GenerateSaturation(
        float h, float v)
    {
        return CreateGradient(t => Color.HSVToRGB(h, t, v));
    }

    private Texture2D GenerateValue(
        float h, float s)
    {
        return CreateGradient(t => Color.HSVToRGB(h, s, t));
    }

    private Texture2D CreateGradient(
        Func<float, Color> colorFunc)
    {
        var texture = new Texture2D(
            GRADIENT_WIDTH,
            GRADIENT_HEIGHT,
            TextureFormat.RGBA32,
            false);

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < GRADIENT_WIDTH; x++)
        {
            float t = x / (GRADIENT_WIDTH - 1f);
            Color color = colorFunc(t);

            for (int y = 0; y < GRADIENT_HEIGHT; y++)
                texture.SetPixel(x, y, color);
        }

        texture.Apply();
        return texture;
    }
    #endregion
}