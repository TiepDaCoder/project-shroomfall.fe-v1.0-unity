using Contract.Enum.EntityDomain;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerActView : MonoBehaviour
{
    #region Attributes
    [Header("UI References")]
    [SerializeField] private HUDUtilsView hudUtilsView;

    [Header("Movement Settings")]
    private float moveSendTimer = 0f;
    private float moveSendRate;
    private Vector2 lastSentDir;
    private bool lastWasMoving = false;
    #endregion

    #region Properties
    // HIGH-FREQUENCY ROUTE: Handled by your WebSocket/Real-time Movement System
    public event Action<Vector2> OnMoved;

    // LOW-FREQUENCY ROUTE: Handled by your Inventory/Combat System via HTTP
    public event Action<Vector2> OnItemUsed;

    public event Action OnInteract;
    #endregion

    #region Methods
    void Update()
    {
        if (IsTyping())
            return;

        // ========================================================
        // 0. HUD TOGGLE & BLOCKER
        // ========================================================
        if (Input.GetKeyDown(KeyCode.E) && hudUtilsView != null)
        {
            hudUtilsView.ToggleHUD();
        }

        // If the HUD is open, block all interactions/movement
        if (hudUtilsView != null && hudUtilsView.IsHUDOpen)
        {
            if (lastWasMoving)
            {
                lastWasMoving = false;
                lastSentDir = Vector2.zero;
                OnMoved?.Invoke(Vector2.zero);
            }
            return;
        }

        // ========================================================
        // 1. Gather Raw Input States
        // ========================================================
        Vector2 dir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        bool isClicked = Input.GetMouseButtonDown(0);
        bool isMoving = dir != Vector2.zero;
        bool isInteract = Input.GetKeyDown(KeyCode.Space);

        if (isInteract)
        {
            OnInteract?.Invoke();
            return;
        }

        // ========================================================
        // 2. RELIABLE TRANSACTION CHANNEL (HTTP / Click Actions)
        // ========================================================
        if (isClicked)
        {
            Vector3 screenMouse = Input.mousePosition;
            screenMouse.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldMouse = Camera.main.ScreenToWorldPoint(screenMouse);

            OnItemUsed?.Invoke((Vector2)worldMouse);
            return;
        }

        // ========================================================
        // 3. HIGH-FREQUENCY UNRELIABLE CHANNEL (WebSockets / Movement)
        // ========================================================
        EntityAction action = isMoving ? EntityAction.RUN : EntityAction.IDLE;
        moveSendTimer += Time.deltaTime;

        // Triggers instantly if the player changes direction (e.g. Up -> UpRight)
        bool directionChanged = isMoving && Vector2.SqrMagnitude(dir - lastSentDir) > 0.01f;
        bool stateChanged = isMoving != lastWasMoving;

        bool shouldSend = stateChanged || directionChanged || (isMoving && moveSendTimer >= moveSendRate);

        if (shouldSend)
        {
            moveSendTimer = 0f;
            lastSentDir = dir;
            lastWasMoving = isMoving;

            // Fire movement update (sends normalized direction while moving, Vector2.zero ONCE when stopping)
            OnMoved?.Invoke(dir);
        }
    }

    private bool IsTyping()
    {
        if (EventSystem.current == null) return false;
        var selected = EventSystem.current.currentSelectedGameObject;
        return selected != null && selected.GetComponent<TMP_InputField>() != null;
    }
    #endregion
}