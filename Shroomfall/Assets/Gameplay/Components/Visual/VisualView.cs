using Assets.Utilities;
using Contract.Enum.EntityDomain;
using System.Collections;
using TMPro;
using UnityEngine;

public class VisualView : MonoBehaviour
{
    #region Attributes
    [Header("Renderers")]
    [SerializeField] private SpriteRenderer skinRenderer;
    [SerializeField] private SpriteRenderer itemRenderer;

    [Header("Animators")]
    [SerializeField] private Animator skinAnimator;
    [SerializeField] private Animator itemAnimator;

    [SerializeField] private TMP_Text nameText;

    [Header("Retro Flash Durations")]
    [Tooltip("Time spent solid White")][SerializeField] private float whiteStageDuration = 0.05f;
    [Tooltip("Time spent solid Red")][SerializeField] private float redStageDuration = 0.05f;
    [Tooltip("Time spent solid Black")][SerializeField] private float blackStageDuration = 0.05f;

    private Coroutine damageRoutine;

    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

    private MaterialPropertyBlock skinBlock;
    private MaterialPropertyBlock itemBlock;

    private EntityAction currentAction = EntityAction.IDLE;
    private EntityDirection currentDirection = EntityDirection.DOWN;
    #endregion

    #region Methods
    private void Awake()
    {
        skinBlock = new MaterialPropertyBlock();
        itemBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
    }
    public void ApplyAppearance(
        EntityAsset entity,
        Color skinColor,
        string name)
    {
        if (entity != null && skinAnimator != null)
            skinAnimator.runtimeAnimatorController = entity.animatorOverride;

        if (skinRenderer != null)
            skinRenderer.color = skinColor;

        if (nameText != null)
            nameText.text = name;
    }

    public void HoldItem(
        ItemAsset item)
    {
        if (itemAnimator != null)
            itemAnimator.runtimeAnimatorController = item != null ? item.animatorOverride : null;
    }

    public void SetAction(
        EntityAction action)
    {
        if (action == EntityAction.DAMAGED)
        {
            if (damageRoutine != null)
                StopCoroutine(damageRoutine);

            damageRoutine = StartCoroutine(DamageFlashSequence());
            return;
        }

        if (currentAction == action)
            return;
        currentAction = action;

        SetAnimatorAction(skinAnimator, action);
        SetAnimatorAction(itemAnimator, action);
    }

    public void SetDirection(
        EntityDirection direction)
    {
        if (currentDirection == direction)
            return;

        currentDirection = direction;
        Vector2 value = DirectionHelper.DirectionToVector(direction);
        SetAnimatorDirection(skinAnimator, value);
        SetAnimatorDirection(itemAnimator, value);
    }

    private void SetAnimatorAction(
        Animator animator,
        EntityAction action)
    {
        if (animator == null)
            return;

        int actionInt = (int)action;

        animator.SetFloat("Action", actionInt);
    }

    private void SetAnimatorDirection(
        Animator animator,
        Vector2 direction)
    {
        if (animator == null)
            return;

        animator.SetFloat("MoveX", direction.x);
        animator.SetFloat("MoveY", direction.y);
    }

    /// <summary>
    /// Hard-stepped retro pixel color sequence: White -> Red -> Black -> Normal
    /// </summary>
    private IEnumerator DamageFlashSequence()
    {
        // 1. STAGE WHITE (Full Intensity)
        SetFlash(skinRenderer, skinBlock, 1f, Color.white);
        SetFlash(itemRenderer, itemBlock, 1f, Color.white);
        yield return new WaitForSeconds(whiteStageDuration);

        // 2. STAGE RED
        SetFlash(skinRenderer, skinBlock, 1f, Color.red);
        SetFlash(itemRenderer, itemBlock, 1f, Color.red);
        yield return new WaitForSeconds(redStageDuration);

        // 3. STAGE BLACK
        SetFlash(skinRenderer, skinBlock, 1f, Color.black);
        SetFlash(itemRenderer, itemBlock, 1f, Color.black);
        yield return new WaitForSeconds(blackStageDuration);

        // 4. RETURN TO NORMAL (Clear Flash Effect)
        SetFlash(skinRenderer, skinBlock, 0f, Color.clear);
        SetFlash(itemRenderer, itemBlock, 0f, Color.clear);

        damageRoutine = null;
    }

    private void SetFlash(
        SpriteRenderer renderer,
        MaterialPropertyBlock block,
        float amount,
        Color flashColor)
    {
        if (renderer == null)
            return;

        renderer.GetPropertyBlock(block);

        block.SetFloat(FlashAmountId, amount);
        block.SetColor(FlashColorId, flashColor);

        renderer.SetPropertyBlock(block);
    }
    #endregion
}