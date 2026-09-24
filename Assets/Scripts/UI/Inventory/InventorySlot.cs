using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI.Inventory
{
    public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("UI Components")]
        public Image IconImage;
        public Image BackgroundImage;
        public TextMeshProUGUI CountText;
        [SerializeField] private GameObject contextMenuPrefab;
        
        [Header("Visual Settings")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color filledColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.8f, 0.8f, 1f, 0.8f);
        
        [Header("Input")]
        [SerializeField] private InputActionProperty selectAction;

        private Transform _spawnpoint;
        public AdaptiveGridInventory Inventory;
        private InventoryItem _item;
        private int _itemCount = 0;
        private GameObject _contextMenu;
        private bool _isHovered = false;

        // Текущий слот с открытым контекстным меню
        private static InventorySlot _currentContextSlot;
        
        public InventoryItem Item => _item;
        public int ItemCount => _itemCount;
        public bool IsEmpty => !_item;
        
        private void Start()
        {
            Inventory = GetComponentInParent<AdaptiveGridInventory>();
            UpdateVisuals();
        }
        
        private void Update()
        {
            if (_isHovered && selectAction.action.triggered)
            {
                ShowContextMenu();
            }
        }
        
        public void SetItem(InventoryItem item, Transform spawnpoint, int count = 1)
        {
            _spawnpoint = spawnpoint;
            _item = item;
            _itemCount = count;
            UpdateVisuals();
        }
        
        public void ClearSlot()
        {
            Inventory.RemoveEmptySlot(_item.itemId);
            var database = Resources.Load<InventoryItemDatabase>("InventoryItemDatabase");
            database.RemoveItem(_item);
        }
        
        public void AddToStack(int amount = 1)
        {
            if (_item && _item.isStackable)
            {
                _itemCount = Mathf.Min(_itemCount + amount, _item.maxStackSize);
                UpdateVisuals();
            }
        }
        
        public bool RemoveFromStack(int amount = 1)
        {
            if (!_item || _itemCount < amount) return false;
            
            _itemCount -= amount;
            if (_itemCount <= 0)
            {
                ClearSlot();
            }
            else
            {
                UpdateVisuals();
            }
            return true;
        }
        
        private void UpdateVisuals()
        {
            if (_item)
            {
                IconImage.sprite = _item.icon;
                IconImage.color = filledColor;
                BackgroundImage.color = filledColor;
                
                if (_item.isStackable && _itemCount > 1)
                {
                    CountText.text = _itemCount.ToString();
                    CountText.gameObject.SetActive(true);
                }
                else
                {
                    CountText.gameObject.SetActive(false);
                }
            }
            else
            {
                IconImage.sprite = null;
                IconImage.color = emptyColor;
                BackgroundImage.color = emptyColor;
                CountText.gameObject.SetActive(false);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (!IsEmpty)
            {
                BackgroundImage.color = hoverColor;
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            UpdateVisuals();
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsEmpty)
            {
                ShowContextMenu();
            }
        }
        
        private void ShowContextMenu()
        {
            // Если у другого слота уже открыто контекстное меню — закрываем его
            if (_currentContextSlot && _currentContextSlot != this)
            {
                _currentContextSlot.HideContextMenu();
            }

            if (_contextMenu)
            {
                return;
            }
            
            _contextMenu = Instantiate(contextMenuPrefab, transform);
            var contextMenu = _contextMenu.GetComponent<InventoryContextMenu>();
            contextMenu.Initialize(_item, this, _spawnpoint);
            
            RectTransform rectTransform = _contextMenu.GetComponent<RectTransform>();
            RectTransform temp = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.rect.width + 17 - temp.anchoredPosition.x, 0);

            _currentContextSlot = this;
        }
        
        private void HideContextMenu()
        {
            if (_contextMenu)
            {
                Destroy(_contextMenu);
                _contextMenu = null;
            }

            if (_currentContextSlot == this)
            {
                _currentContextSlot = null;
            }
        }
    }
} 