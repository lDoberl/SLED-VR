using System;
using Systems.Omp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class PickupableItem : MonoBehaviour
    {
        [Header("Item Settings")]
        [SerializeField] protected string itemId;
        [SerializeField] protected string displayName;
        [SerializeField] protected ToolCategory category = ToolCategory.Tools;
        [SerializeField] protected bool isStackable = false;
        [SerializeField] protected int maxStackSize = 1;
        [SerializeField] protected Sprite icon;
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected string description;
        [SerializeField] protected string surfaceName;
        
        [Header("Visual Feedback")]
        [SerializeField] protected bool showPickupPrompt = true;
        [SerializeField] private GameObject pickupPrompt;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material originalMaterial;
        
        [Header("Interaction")]
        [SerializeField] protected float interactionDistance = 2f;
        [SerializeField] private LayerMask playerLayer = -1;
        [SerializeField] private bool requirePlayerProximity = true;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onPickup;
        [SerializeField] private UnityEvent onHighlight;
        [SerializeField] private UnityEvent onUnhighlight;
        
        private Renderer _renderer;
        private bool _isHighlighted = false;
        private bool _isPlayerNearby = false;
        private GameObject _player;
        private DateTime? _timeOfPhoto;
        private Vector3 _initialPosition;
        private Vector3 _lastPosition;
        private bool _movementViolationReported;
        private bool _protocolTrackingDisabled;
        
        public string ItemId => itemId;
        public string DisplayName => displayName;
        public ToolCategory Category => category;
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public string Description => description;
        public string SurfaceName => surfaceName;
        public DateTime? TimeOfPhoto => _timeOfPhoto;

        public void SetItemID(string id) => itemId = id;
        public void SetTimeOfPhoto(DateTime? time) => _timeOfPhoto = time;
        public void SetSurfaceName(string nameOfSurface) => surfaceName = nameOfSurface;
        public void DisableProtocolTracking() => _protocolTrackingDisabled = true;
        
        
        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _renderer = GetComponent<Renderer>();
            if (_renderer && !originalMaterial)
            {
                originalMaterial = _renderer.material;
            }
            
            if (pickupPrompt)
            {
                pickupPrompt.SetActive(false);
            }

            _initialPosition = transform.position;
            _lastPosition = _initialPosition;
            _movementViolationReported = false;
        }
        
        private void Update()
        {
            if (_protocolTrackingDisabled)
            {
                return;
            }

            if (requirePlayerProximity)
            {
                CheckPlayerProximity();
            }

            CheckMovementForProtocolViolation();
        }
        
        private void CheckPlayerProximity()
        {
            if (_player)
            {
                float distance = Vector3.Distance(transform.position, _player.transform.position);
                bool wasNearby = _isPlayerNearby;
                _isPlayerNearby = distance <= interactionDistance;
                
                if (_isPlayerNearby != wasNearby)
                {
                    if (_isPlayerNearby)
                    {
                        OnPlayerEnter();
                    }
                    else
                    {
                        OnPlayerExit();
                    }
                }
            }
        }
        
        private void OnPlayerEnter()
        {
            if (showPickupPrompt && pickupPrompt)
            {
                pickupPrompt.SetActive(true);
            }
            
            Highlight();
        }
        
        private void OnPlayerExit()
        {
            if (showPickupPrompt && pickupPrompt)
            {
                pickupPrompt.SetActive(false);
            }
            
            Unhighlight();
        }
        
        public void Highlight()
        {
            if (_isHighlighted) return;
            
            _isHighlighted = true;
            
            if (_renderer && highlightMaterial)
            {
                _renderer.material = highlightMaterial;
            }
            
            onHighlight?.Invoke();
        }
        
        public void Unhighlight()
        {
            if (!_isHighlighted) return;
            
            _isHighlighted = false;
            
            if (_renderer && originalMaterial)
            {
                _renderer.material = originalMaterial;
            }
            
            onUnhighlight?.Invoke();
        }
        
        public bool CanBePickedUp()
        {
            if (requirePlayerProximity && !_isPlayerNearby)
                return false;
                
            return true;
        }
        
        public void Pickup()
        {
            if (!CanBePickedUp()) return;

            if (_protocolTrackingDisabled)
            {
                onPickup?.Invoke();
                // var inv = FindAnyObjectByType<AdaptiveGridInventory>();
                // if (inv)
                // {
                //     inv.AddItemToInventory(itemId);
                // }
                Destroy(gameObject);
                return;
            }

            // Проверка протокола: перед любым взаимодействием предмет должен быть зафиксирован на фото.
            // Если уже был зафиксирован факт перемещения без фото, второй раз ошибку не шлём.
            if (!_timeOfPhoto.HasValue && !_movementViolationReported)
            {
                var analyzer = OmpActionAnalyzer.Instance;
                if (analyzer)
                {
                    analyzer.RegisterCustomError(
                        penaltyId: $"no-photo-before-pickup-{itemId}",
                        description: $"Предмет \"{displayName}\" взят без предварительной фотофиксации.",
                        points: 1f,
                        relatedActionId: "PICKUP_ITEM",
                        context: itemId
                    );
                }
                else
                {
                    Debug.LogWarning($"{nameof(PickupableItem)}.Pickup: OmpActionAnalyzer not found, штраф не зарегистрирован.");
                }
            }
            
            onPickup?.Invoke();
            
            // var inventory = FindAnyObjectByType<AdaptiveGridInventory>();
            // if (inventory)
            // {
            //     inventory.AddItemToInventory(itemId);
            // }
            
            Destroy(gameObject);
        }
        
        private void CheckMovementForProtocolViolation()
        {
            if (_protocolTrackingDisabled)
                return;

            if (_movementViolationReported)
                return;

            if (_timeOfPhoto.HasValue)
            {
                _lastPosition = transform.position;
                return;
            }

            float distance = Vector3.Distance(transform.position, _lastPosition);
            if (distance <= 0.03f)
                return;

            var analyzer = OmpActionAnalyzer.Instance;
            if (analyzer)
            {
                analyzer.RegisterCustomError(
                    penaltyId: $"no-photo-before-move-{itemId}",
                    description: $"Предмет \"{displayName}\" был перемещён без предварительной фотофиксации.",
                    points: 1f,
                    relatedActionId: "MOVE_ITEM_WITHOUT_PHOTO",
                    context: itemId
                );
                _movementViolationReported = true;
            }

            _lastPosition = transform.position;
        }
        
        public void SetItemId(string newItemId)
        {
            itemId = newItemId;
        }
        
        public void SetDisplayName(string newDisplayName)
        {
            displayName = newDisplayName;
        }
        
        public void SetCategory(ToolCategory newCategory)
        {
            category = newCategory;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (requirePlayerProximity)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, interactionDistance);
            }
        }
    }
} 