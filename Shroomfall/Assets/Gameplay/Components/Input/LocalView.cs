using Assets.Gameplay.Component.Input;
using System;
using UnityEngine;

public class LocalView : EntityView
{
    #region Attributes
    [Header("Sub Views")]
    [SerializeField] private PlayerActView playerActView;
    [SerializeField] private Camera mainCamera;

    private TriggerPlateView currentTrigger;
    #endregion

    #region Properties
    public event Action<Vector2> OnMoved;
    public event Action<Vector2> OnItemUsed;
    #endregion

    #region Methods
    private void Awake()
    {
        gameObject.SetActive(false);
        playerActView.OnMoved += dir => OnMoved?.Invoke(dir);
        playerActView.OnItemUsed += dir => OnItemUsed?.Invoke(dir);
    }

    public override void ApplyPosition(
        Vector2 pos)
    {
        Vector3 targetPos = new Vector3(pos.x, pos.y, 0);
        transform.position = targetPos;

        FollowCamera(targetPos);
    }

    private void FollowCamera(
        Vector3 playerPos)
    {
        if (mainCamera == null) return;

        float camHeight = mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;

        Vector3 clamped = new Vector3(playerPos.x, playerPos.y, mainCamera.transform.position.z);

        // Immediate stop at boundary
        mainCamera.transform.position = clamped;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out TriggerPlateView trigger))
        {
            currentTrigger = trigger;

            playerActView.OnInteract += trigger.Interact;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out TriggerPlateView trigger))
        {
            playerActView.OnInteract -= trigger.Interact;

            if (currentTrigger == trigger)
                currentTrigger = null;
        }
    }
    #endregion
}