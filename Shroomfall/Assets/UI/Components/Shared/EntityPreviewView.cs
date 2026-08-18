using Assets.Utilities;
using Contract.Enum.EntityDomain;
using UnityEngine;
using UnityEngine.UI;

public class EntityPreviewView : MonoBehaviour
{
    #region Attributes
    [Header("UI Render Target")]
    [SerializeField] private Image image;

    [Header("Hidden World Components")]
    [SerializeField] private SpriteRenderer hiddenRenderer;
    [SerializeField] private Animator previewAnimator;
    #endregion

    #region Properties
    #endregion

    #region Methods
    public void Apply(
        EntityAsset entity,
        Color skinColor,
        EntityDirection direction)
    {
        if (previewAnimator != null && entity != null)
        {
            previewAnimator.runtimeAnimatorController = entity.animatorOverride;

            // Force the action to IDLE
            previewAnimator.SetFloat("Action", (float)EntityAction.IDLE);

            // Translate the direction enum to its Vector2 representation
            Vector2 directionVector = DirectionHelper.DirectionToVector(direction);

            // Set parameters to force the animator into the right state
            previewAnimator.SetFloat("MoveX", directionVector.x);
            previewAnimator.SetFloat("MoveY", directionVector.y);
            previewAnimator.Update(0f);
        }

        // Grab the sprite the animator just evaluated onto the SpriteRenderer
        if (hiddenRenderer != null && image != null)
        {
            image.sprite = hiddenRenderer.sprite;
            image.color = skinColor;
        }
    }
    #endregion
}