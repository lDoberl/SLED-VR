using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CSI.Data;
using CSI.Interaction;
using CSI.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CSI.UI.Inventory
{
    /// <summary>
    /// Wrist-mounted inventory panel with three always-visible tabs:
    ///  - Tools: standing equipment (camera, brush, tape...), populated once at start — not tied
    ///    to "discovery", it's the kit you always carry.
    ///  - Traces: evidence actually found in the world, populated as EvidenceManager reports
    ///    collection.
    ///  - Photos: a real photo gallery — every shutter press adds an actual captured thumbnail;
    ///    clicking one opens it full-size in the photo viewer overlay.
    /// Opens attached to whichever hand's button triggered it (Y = left, B = right) rather than
    /// being pinned to one wrist permanently — pressing the other hand's button while open
    /// relocates it there instead of closing it.
    /// </summary>
    public class InventoryController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button closeButton;

        [Header("Hands")]
        [SerializeField] private Transform leftHandAttach;
        [SerializeField] private Transform rightHandAttach;
        [SerializeField] private Vector3 panelLocalPosition = new Vector3(0f, 0.1f, 0.05f);
        [SerializeField] private Vector3 panelLocalEulerAngles = new Vector3(55f, 0f, 0f);

        [Header("Tabs (always visible)")]
        [SerializeField] private Button toolsTabButton;
        [SerializeField] private Button tracesTabButton;
        [SerializeField] private Button photosTabButton;
        [SerializeField] private Image toolsTabBackground;
        [SerializeField] private Image tracesTabBackground;
        [SerializeField] private Image photosTabBackground;
        [SerializeField] private TextMeshProUGUI toolsTabLabel;
        [SerializeField] private TextMeshProUGUI tracesTabLabel;
        [SerializeField] private TextMeshProUGUI photosTabLabel;
        [SerializeField] private Color tabInactiveColor = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Color tabActiveColor = new Color(0.79f, 0.68f, 0.5f, 1f);
        [SerializeField] private Color tabInactiveTextColor = Color.white;
        [SerializeField] private Color tabActiveTextColor = new Color(0.25f, 0.18f, 0.08f, 1f);

        [Header("Grid content (only one active at a time)")]
        [SerializeField] private GameObject toolsGridContainer;
        [SerializeField] private GameObject tracesGridContainer;
        [SerializeField] private GameObject photosGridContainer;
        [SerializeField] private InventorySlotView slotPrefab;

        [Header("Detail view")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private GameObject detailEmptyState;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailMetaText;
        [SerializeField] private Image detailIcon;

        [Header("Photo viewer (full-size overlay)")]
        [SerializeField] private GameObject photoViewerPanel;
        [SerializeField] private Image photoViewerImage;
        [SerializeField] private TextMeshProUGUI photoViewerTimeText;
        [SerializeField] private Button photoViewerCloseButton;

        [Header("Final report (Traces tab only)")]
        [SerializeField] private GameObject reportButton;
        [SerializeField] private CSI.UI.FinalReport.FinalReportController finalReportController;

        public event Action<EvidenceCategory> OnCategoryChanged;
        public event Action OnOpened;

        private bool _isOpen;
        private Transform _currentHand;
        private EvidenceCategory _activeCategory = EvidenceCategory.Tool;
        private InventorySlotView _selectedSlot;
        private int _photoCounter;

        private InputAction _openLeftAction;
        private InputAction _openRightAction;

        private readonly List<InventorySlotView> _toolSlots = new List<InventorySlotView>();
        private readonly List<InventorySlotView> _traceSlots = new List<InventorySlotView>();
        private readonly List<InventorySlotView> _photoSlots = new List<InventorySlotView>();
        private readonly Dictionary<string, int> _toolRemaining = new Dictionary<string, int>();
        private readonly Dictionary<string, InventorySlotView> _toolSlotsById = new Dictionary<string, InventorySlotView>();

        private void Start()
        {
            _openLeftAction = new InputAction("OpenInventoryLeft", InputActionType.Button);
            _openLeftAction.AddBinding("<XRController>{LeftHand}/{SecondaryButton}");
            _openLeftAction.Enable();

            _openRightAction = new InputAction("OpenInventoryRight", InputActionType.Button);
            _openRightAction.AddBinding("<XRController>{RightHand}/{SecondaryButton}");
            _openRightAction.Enable();

            if (toolsTabButton != null) toolsTabButton.onClick.AddListener(() => SetActiveCategory(EvidenceCategory.Tool));
            if (tracesTabButton != null) tracesTabButton.onClick.AddListener(() => SetActiveCategory(EvidenceCategory.Trace));
            if (photosTabButton != null) photosTabButton.onClick.AddListener(() => SetActiveCategory(EvidenceCategory.Photo));
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (photoViewerCloseButton != null) photoViewerCloseButton.onClick.AddListener(ClosePhotoViewer);
            if (reportButton != null) reportButton.GetComponentInChildren<Button>().onClick.AddListener(OpenFinalReport);

            CameraTool.OnAnyPhotoCaptured += HandlePhotoCaptured;
            ReturnableTool.OnToolReturned += HandleToolReturned;

            if (panelRoot != null)
                panelRoot.SetActive(false);
            if (photoViewerPanel != null)
                photoViewerPanel.SetActive(false);

            SetActiveCategory(EvidenceCategory.Tool);
            ShowDetail(null);

            StartCoroutine(PopulateStartingToolsNextFrame());
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.OnCollected -= HandleCollected;

            CameraTool.OnAnyPhotoCaptured -= HandlePhotoCaptured;
            ReturnableTool.OnToolReturned -= HandleToolReturned;

            _openLeftAction?.Disable();
            _openLeftAction?.Dispose();
            _openRightAction?.Disable();
            _openRightAction?.Dispose();
        }

        private void Update()
        {
            if (_openLeftAction != null && _openLeftAction.triggered)
                HandleHandButton(leftHandAttach);

            if (_openRightAction != null && _openRightAction.triggered)
                HandleHandButton(rightHandAttach);
        }

        private void HandleHandButton(Transform hand)
        {
            if (hand == null) return;

            if (_isOpen && _currentHand == hand)
                Close();
            else
                OpenOnHand(hand);
        }

        private void OpenOnHand(Transform hand)
        {
            _currentHand = hand;
            _isOpen = true;

            if (panelRoot != null)
            {
                panelRoot.transform.SetParent(hand, false);
                panelRoot.transform.localPosition = panelLocalPosition;
                panelRoot.transform.localRotation = Quaternion.Euler(panelLocalEulerAngles);
                panelRoot.SetActive(true);
            }

            OnOpened?.Invoke();
        }

        public void Close()
        {
            _isOpen = false;
            _currentHand = null;
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void OpenFinalReport()
        {
            if (finalReportController != null)
                finalReportController.Open();
            Close();
        }

        public void SetActiveCategory(EvidenceCategory category)
        {
            _activeCategory = category;

            // Each grid container sits inside its own ScrollView/Viewport wrapper (added for
            // scrolling support). The wrapper's Viewport carries a raycastable Image so drag-to-
            // scroll works; leaving inactive tabs' wrappers active would leave their invisible
            // Viewports overlapping the active tab and stealing hover/click raycasts from its
            // slots. So the whole wrapper — not just the innermost content — must be toggled.
            SetGridWrapperActive(toolsGridContainer, category == EvidenceCategory.Tool);
            SetGridWrapperActive(tracesGridContainer, category == EvidenceCategory.Trace);
            SetGridWrapperActive(photosGridContainer, category == EvidenceCategory.Photo);
            // Tab buttons are deliberately left untouched here — they stay visible/clickable
            // no matter which category is active.

            if (toolsTabBackground != null) toolsTabBackground.color = category == EvidenceCategory.Tool ? tabActiveColor : tabInactiveColor;
            if (tracesTabBackground != null) tracesTabBackground.color = category == EvidenceCategory.Trace ? tabActiveColor : tabInactiveColor;
            if (photosTabBackground != null) photosTabBackground.color = category == EvidenceCategory.Photo ? tabActiveColor : tabInactiveColor;

            if (toolsTabLabel != null) toolsTabLabel.color = category == EvidenceCategory.Tool ? tabActiveTextColor : tabInactiveTextColor;
            if (tracesTabLabel != null) tracesTabLabel.color = category == EvidenceCategory.Trace ? tabActiveTextColor : tabInactiveTextColor;
            if (photosTabLabel != null) photosTabLabel.color = category == EvidenceCategory.Photo ? tabActiveTextColor : tabInactiveTextColor;

            // The final report is filed against the evidence you've gathered, so it only makes
            // sense to offer it from the Traces tab, not Tools or Photos.
            if (reportButton != null) reportButton.SetActive(category == EvidenceCategory.Trace);

            ShowDetail(null);
            OnCategoryChanged?.Invoke(category);
        }

        private static void SetGridWrapperActive(GameObject gridContainer, bool active)
        {
            if (gridContainer == null) return;

            // gridContainer (Content) -> Viewport -> ScrollView; fall back to the container itself
            // if it isn't wrapped for some reason, so this stays safe either way.
            var wrapper = gridContainer.transform.parent != null && gridContainer.transform.parent.parent != null
                ? gridContainer.transform.parent.parent.gameObject
                : gridContainer;

            wrapper.SetActive(active);
            gridContainer.SetActive(active);
        }

        private void ShowDetail(InventorySlotView slot)
        {
            if (_selectedSlot != null)
                _selectedSlot.SetSelected(false);

            _selectedSlot = slot;

            if (slot != null)
                slot.SetSelected(true);

            if (detailPanel != null)
                detailPanel.SetActive(slot != null);
            // The "select an item" placeholder only makes sense for Tools (a description-driven
            // equipment list). Photos use the full-screen viewer instead, and Traces reads fine
            // as a plain list with no idle hint needed.
            if (detailEmptyState != null)
                detailEmptyState.SetActive(slot == null && _activeCategory == EvidenceCategory.Tool);

            if (slot == null)
                return;

            var item = slot.Item;
            if (detailNameText != null)
                detailNameText.text = item.DisplayName;
            if (detailDescriptionText != null)
                detailDescriptionText.text = item.Description;
            if (detailMetaText != null)
                detailMetaText.text = BuildMetaText(item);
            if (detailIcon != null)
            {
                detailIcon.sprite = item.Icon;
                detailIcon.enabled = item.Icon != null;
            }
        }

        private void HandleToolClicked(InventorySlotView slot)
        {
            string id = slot.Item.EvidenceId;
            if (slot.Item.WorldPrefab == null)
            {
                ShowDetail(slot);
                return;
            }

            if (!_toolRemaining.TryGetValue(id, out int remaining) || remaining <= 0)
                return; // out of stock — button is already non-interactable, but guard anyway

            remaining--;
            _toolRemaining[id] = remaining;
            slot.SetQuantity(remaining);

            SpawnToolIntoHand(slot.Item.WorldPrefab, id);
            Close();
        }

        private void HandleToolReturned(string evidenceId)
        {
            if (!_toolRemaining.TryGetValue(evidenceId, out int remaining))
                return; // not a stock-tracked tool (shouldn't happen, but keep this safe)

            remaining++;
            _toolRemaining[evidenceId] = remaining;

            if (_toolSlotsById.TryGetValue(evidenceId, out var slot))
                slot.SetQuantity(remaining);
        }

        private Transform GetFreeHand()
        {
            if (_currentHand == leftHandAttach) return rightHandAttach;
            if (_currentHand == rightHandAttach) return leftHandAttach;
            return rightHandAttach;
        }

        private void SpawnToolIntoHand(GameObject prefab, string evidenceId)
        {
            var hand = GetFreeHand();
            GameObject instance;
            if (hand == null)
            {
                instance = Instantiate(prefab);
            }
            else
            {
                Vector3 spawnPos = hand.position + hand.forward * 0.15f + hand.up * 0.03f;
                instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            }

            var returnable = instance.GetComponent<ReturnableTool>();
            if (returnable != null)
                returnable.Initialize(evidenceId);
        }

        private void OpenPhotoViewer(InventorySlotView slot)
        {
            if (photoViewerPanel == null || slot.Item == null) return;

            photoViewerPanel.SetActive(true);
            if (photoViewerImage != null)
            {
                photoViewerImage.sprite = slot.Item.Icon;
                photoViewerImage.enabled = slot.Item.Icon != null;
                photoViewerImage.preserveAspect = true;
            }
            if (photoViewerTimeText != null)
                photoViewerTimeText.text = BuildMetaText(slot.Item);
        }

        private void ClosePhotoViewer()
        {
            if (photoViewerPanel != null)
                photoViewerPanel.SetActive(false);
        }

        private void HandleDeletePhoto(InventorySlotView slot)
        {
            _photoSlots.Remove(slot);
            if (_selectedSlot == slot)
                ShowDetail(null);
            Destroy(slot.gameObject);
        }

        private static string BuildMetaText(InventoryItemRuntime item)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(item.SurfaceName))
                parts.Add($"Найдено: {item.SurfaceName}");

            if (!string.IsNullOrEmpty(item.PhotoTimestampUtc) &&
                DateTime.TryParse(item.PhotoTimestampUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
            {
                parts.Add($"Сфотографировано: {dt.ToLocalTime():HH:mm:ss}");
            }

            return string.Join("   |   ", parts);
        }

        private void HandleCollected(string evidenceId)
        {
            var mgr = EvidenceManager.Instance;
            var piece = mgr.GetPiece(evidenceId);
            if (piece == null || slotPrefab == null) return;

            // Tools are pre-populated at start (see PopulateStartingToolsNextFrame); only
            // Traces come through the discovery pipeline here.
            if (piece.category != EvidenceCategory.Trace || tracesGridContainer == null) return;

            var slot = Instantiate(slotPrefab, tracesGridContainer.transform);
            slot.SetData(BuildRuntimeItem(piece, mgr));
            slot.SetQuantity(-1); // traces aren't limited-stock — hide the badge
            slot.OnClicked += ShowDetail;
            slot.OnHoverEnter += ShowDetail;
            slot.OnHoverExit += _ => ShowDetail(null);
            _traceSlots.Add(slot);
        }

        private void HandlePhotoCaptured(CapturedPhoto photo)
        {
            if (photosGridContainer == null || slotPrefab == null || photo.Thumbnail == null) return;

            _photoCounter++;
            var slot = Instantiate(slotPrefab, photosGridContainer.transform);
            slot.SetData(new InventoryItemRuntime
            {
                EvidenceId = $"Photo_{_photoCounter}",
                DisplayName = $"Фото {_photoCounter}",
                Description = string.Empty,
                Icon = photo.Thumbnail,
                Category = EvidenceCategory.Photo,
                PhotoTimestampUtc = photo.TimestampUtc
            });
            slot.OnClicked += OpenPhotoViewer;
            slot.SetQuantity(-1); // photos aren't limited-stock — hide the badge
            slot.EnableDeleteButton();
            slot.OnDeleteRequested += HandleDeletePhoto;
            _photoSlots.Add(slot);
        }

        private IEnumerator PopulateStartingToolsNextFrame()
        {
            // Wait one frame so EvidenceManager.Start() has had a chance to load the active case
            // before we read it here. This also covers TrainingScene, where TutorialManager adds
            // EvidenceManager dynamically in its own Start() — Unity doesn't guarantee Start() call
            // order across objects within the same frame, so subscribing immediately in our own
            // Start() could silently miss EvidenceManager.Instance still being null at that instant
            // (no error, no retry — collected evidence would just never reach the Traces tab).
            yield return null;

            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.OnCollected += HandleCollected;

            var activeCase = EvidenceManager.Instance != null ? EvidenceManager.Instance.ActiveCase : null;
            if (activeCase == null || slotPrefab == null || toolsGridContainer == null)
                yield break;

            foreach (var piece in activeCase.startingTools)
            {
                if (piece == null) continue;

                var slot = Instantiate(slotPrefab, toolsGridContainer.transform);
                slot.SetData(new InventoryItemRuntime
                {
                    EvidenceId = piece.evidenceId,
                    DisplayName = piece.visibleDisplayName,
                    Description = piece.visibleDescription,
                    Icon = piece.icon,
                    Category = EvidenceCategory.Tool,
                    WorldPrefab = piece.worldPrefab
                });
                slot.OnClicked += HandleToolClicked;
                slot.OnHoverEnter += ShowDetail;
                slot.OnHoverExit += _ => ShowDetail(null);

                if (piece.worldPrefab != null)
                {
                    int startingQty = Mathf.Max(1, piece.startingQuantity);
                    _toolRemaining[piece.evidenceId] = startingQty;
                    _toolSlotsById[piece.evidenceId] = slot;
                    slot.SetQuantity(startingQty);
                }
                else
                {
                    slot.SetQuantity(-1); // no world prefab to spawn — this is a description-only tool, no stock to track
                }

                _toolSlots.Add(slot);
            }
        }

        private static InventoryItemRuntime BuildRuntimeItem(EvidencePiece piece, EvidenceManager mgr)
        {
            var state = mgr.GetState(piece.evidenceId);
            bool revealHidden = state != null && state.IsCollected;

            return new InventoryItemRuntime
            {
                EvidenceId = piece.evidenceId,
                DisplayName = revealHidden && !string.IsNullOrEmpty(piece.hiddenDisplayName) ? piece.hiddenDisplayName : piece.visibleDisplayName,
                Description = revealHidden && !string.IsNullOrEmpty(piece.hiddenDescription) ? piece.hiddenDescription : piece.visibleDescription,
                Icon = piece.icon,
                Category = piece.category,
                SurfaceName = state != null ? state.SurfaceName : null,
                PhotoTimestampUtc = state != null ? state.PhotoTimestampUtc : null
            };
        }
    }
}
