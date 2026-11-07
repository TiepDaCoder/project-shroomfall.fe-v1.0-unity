using Assets.Source.Core;
using Assets.Source.Data;
using Assets.Source.Gameplay.Component.Visual;
using Assets.Source.Service;
using Assets.Source.Utility;
using Contract.Enum.EntityDomain;
using Contract.Enum.MetaDomain.Item;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Source.Gameplay.Component.Input
{
    public class ActView : MonoBehaviour
    {
        #region Attributes
        [Header("Tutorial Configuration")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private VisualView visualView;

        [Header("Placement")]
        [SerializeField] private EntityCatalogSO entityCatalog;
        [SerializeField] private SpriteRenderer placementPreviewRenderer;
        [SerializeField] private float pixelsPerUnit = 16f;

        private DefinitionService definitionService;
        private PlayerService playerService;
        private InventoryService inventoryService;
        private UIService uiService;
        private TutorialService tutorialService;

        private TriggerView currentTrigger;
        private float moveSendTimer = 0f;
        private float moveSendRate;
        private Vector2 lastSentDir;
        private bool lastWasMoving = false;
        private bool isPlacementMode;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            definitionService = ServiceProvider.Get<DefinitionService>();
            playerService = ServiceProvider.Get<PlayerService>();
            inventoryService = ServiceProvider.Get<InventoryService>();
            uiService = ServiceProvider.Get<UIService>();
            tutorialService = ServiceProvider.Get<TutorialService>();

            // Inventory Service
            inventoryService.OnSelectedItemChanged += OnSelectedItemChanged;

            placementPreviewRenderer.enabled = false;
        }

        private void Update()
        {
            if (IsTyping())
                return;

            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard == null)
                return;

            // ========================================================
            // 0. HUD TOGGLE & BLOCKER
            // ========================================================
            if (keyboard.eKey.wasPressedThisFrame)
            {
                uiService.ToggleBackpack();
            }

            if (uiService.IsBackpackOpen)
            {
                StopMovement();
                return;
            }

            // ========================================================
            // PLACEMENT MODE
            // ========================================================
            if (isPlacementMode)
            {
                UpdatePlacementPreview();

                if (mouse != null && mouse.rightButton.wasPressedThisFrame)
                {
                    CancelPlacement();
                }

                if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                {
                    Vector2 snappedPosition = placementPreviewRenderer.transform.position;
                    OnItemUsed(snappedPosition);
                    CancelPlacement();
                }
            }

            // ========================================================
            // 1. Gather Raw Input States
            // ========================================================
            Vector2 dir = new Vector2(GetHorizontalInput(keyboard), GetVerticalInput(keyboard)).normalized;
            bool isClicked = mouse != null && mouse.leftButton.wasPressedThisFrame;
            bool isMoving = dir != Vector2.zero;

            bool isInteract = keyboard.spaceKey.wasPressedThisFrame;
            if (isInteract)
            {
                currentTrigger?.Interact();
                return;
            }

            // ========================================================
            // 2. NORMAL ITEM USAGE
            // ========================================================
            if (isClicked)
            {
                Camera mainCamera = Camera.main;

                if (mainCamera == null)
                    return;

                Vector3 screenMouse = mouse.position.ReadValue();
                screenMouse.z = Mathf.Abs(mainCamera.transform.position.z);
                Vector3 worldMouse = mainCamera.ScreenToWorldPoint(screenMouse);
                OnItemUsed((Vector2)worldMouse);
                return;
            }

            // ========================================================
            // 3. HIGH-FREQUENCY UNRELIABLE CHANNEL (WebSockets / Movement)
            // ========================================================
            EntityAction action = isMoving ? EntityAction.RUN : EntityAction.IDLE;

            if (isMoving)
            {
                // Update local visuals instantly for zero-latency feedback
                EntityDirection facing = Mathf.Abs(dir.x) > Mathf.Abs(dir.y)
                    ? (dir.x > 0 ? EntityDirection.RIGHT : EntityDirection.LEFT)
                    : (dir.y > 0 ? EntityDirection.UP : EntityDirection.DOWN);

                if (visualView != null)
                {
                    visualView.SetDirection(facing);
                    visualView.SetAction(action);
                }
            }
            else
            {
                if (visualView != null)
                {
                    visualView.SetAction(EntityAction.IDLE);
                }
            }

            // Network Synchronization (High-frequency channel)
            moveSendTimer += Time.deltaTime;
            bool directionChanged = isMoving && Vector2.SqrMagnitude(dir - lastSentDir) > 0.01f;
            bool stateChanged = isMoving != lastWasMoving;
            bool shouldSend = stateChanged || directionChanged || (isMoving && moveSendTimer >= moveSendRate);

            if (shouldSend)
            {
                moveSendTimer = 0f;
                lastSentDir = dir;
                lastWasMoving = isMoving;
                SyncMovementToServer(dir);
            }
        }

        private void OnDestroy()
        {
            // Inventory Service
            inventoryService.OnSelectedItemChanged -= OnSelectedItemChanged;
        }

        private void FixedUpdate()
        {
            if (uiService.IsBackpackOpen || (tutorialService != null && !tutorialService.IsActive))
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 dir = new Vector2(GetHorizontalInput(Keyboard.current), GetVerticalInput(Keyboard.current)).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }

        private void StopMovement()
        {
            if (lastWasMoving)
            {
                lastWasMoving = false;
                lastSentDir = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
                SyncMovementToServer(Vector2.zero);
            }
        }

        private float GetHorizontalInput(
            Keyboard keyboard)
        {
            float value = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                value -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                value += 1f;

            return value;
        }

        private float GetVerticalInput(
            Keyboard keyboard)
        {
            float value = 0f;

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                value -= 1f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                value += 1f;

            return value;
        }

        private void OnSelectedItemChanged(
            string definitionID)
        {
            CancelPlacement();
            if (string.IsNullOrEmpty(definitionID))
                return;

            var definition = definitionService.GetItemDefinition(definitionID);
            if (definition == null || definition.Type != ItemType.Placeable)
                return;

            if (!entityCatalog.TryGet(definitionID, out var asset))
                return;

            if (asset == null || asset.icon == null)
                return;

            isPlacementMode = true;
            placementPreviewRenderer.sprite = asset.icon;
            placementPreviewRenderer.enabled = true;
        }

        private void UpdatePlacementPreview()
        {
            if (!isPlacementMode || Mouse.current == null)
                return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            Vector3 screenPosition = Mouse.current.position.ReadValue();
            screenPosition.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            placementPreviewRenderer.transform.position = GetSnappedPosition(worldPosition);
        }

        private Vector2 GetSnappedPosition(
            Vector2 position)
        {
            float cellSize = 16f / pixelsPerUnit;
            float xOffset = cellSize * 0.5f;

            return new Vector2(
                Mathf.Round(position.x / cellSize) * cellSize - xOffset,
                Mathf.Round(position.y / cellSize) * cellSize
            );
        }

        private void CancelPlacement()
        {
            isPlacementMode = false;
            placementPreviewRenderer.enabled = false;
        }

        private void SyncMovementToServer(
            Vector2 direction)
        {
            // If running a tutorial locally without server sync, you can branch here
            if (tutorialService != null && tutorialService.IsActive)
            {
                return;
            }

            AsyncHelper.Run(async () =>
            {
                await playerService.MoveAsync(direction);
            });
        }

        private void OnItemUsed(
            Vector2 direction)
        {
            AsyncHelper.Run(async () =>
            {
                await inventoryService.UseItem(direction);
            });
        }

        private bool IsTyping()
        {
            if (EventSystem.current == null) return false;
            var selected = EventSystem.current.currentSelectedGameObject;
            return selected != null && selected.GetComponent<TMP_InputField>() != null;
        }

        private void OnTriggerEnter2D(
            Collider2D other)
        {
            if (other.TryGetComponent(out TriggerView trigger))
            {
                currentTrigger = trigger;
            }
        }

        private void OnTriggerExit2D(
            Collider2D other)
        {
            if (other.TryGetComponent(out TriggerView trigger))
            {
                if (currentTrigger == trigger)
                    currentTrigger = null;
            }
        }
        #endregion
    }
}