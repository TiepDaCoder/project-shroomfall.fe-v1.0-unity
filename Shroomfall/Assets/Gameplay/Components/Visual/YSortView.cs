using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(SortingGroup))]
public class YSortView : MonoBehaviour
{
    #region Attributes

    private SortingGroup group;
    private SpriteRenderer spriteRenderer;
    private const float SORT_STEP = 0.5f;
    private const int SORT_MULTIPLIER = 100;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        group = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (group == null || spriteRenderer == null)
            return;

        float bottomY = spriteRenderer.bounds.min.y;

        // Round to nearest 0.5 unit
        float snappedBottomY = Mathf.Round(bottomY / SORT_STEP) * SORT_STEP;

        group.sortingOrder = Mathf.RoundToInt(-snappedBottomY * SORT_MULTIPLIER);
    }
    #endregion
}