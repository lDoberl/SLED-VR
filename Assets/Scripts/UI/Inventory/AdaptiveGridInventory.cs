using System.Collections.Generic;
using Data;
using TMPro;
using UI.Convert;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UI.Inventory
{
    public class AdaptiveGridInventory : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private List<GameObject> gridContainers;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button instrumentButton;
        [SerializeField] private Button evidenceButton;
        [SerializeField] private Button notebookButton;
        [SerializeField] private GameObject header;
        
        
        [Header("Input")]
        [SerializeField]
        public InputActionProperty ToggleInventoryAction;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        
        [Header("Item Return")]
        [SerializeField] private LayerMask itemLayerMask = -1;
        [SerializeField] private float pickupDistance = 2f;
        [SerializeField] private InputActionProperty pickupAction;
        
        [Header("Raycast Sources")]
        [SerializeField] private Transform raycastOrigin1;
        [SerializeField] private Transform raycastOrigin2;
        
        [SerializeField] private List<GameObject> categoryTabs;
        private ToolCategory _currentCategory;
        private bool _isInventoryOpen;
        private List<InventorySlot> _slots = new List<InventorySlot>();
        
        private static readonly HashSet<string> Fabula1DefaultToolIds = new HashSet<string>
        {
            "3DCamera",  
            "Brush",      
            "NotebookUI", 
            "FinalReport",
            "Convert",    
            "scotch"      
        };
        
        public GameObject adaptiveGridInventory;
        
        private void Start()
        {
            returnButton.onClick.AddListener(ReturnToMainMenu);
            instrumentButton.onClick.AddListener(() => OnCategoryTabClicked(ToolCategory.Tools));
            evidenceButton.onClick.AddListener(() => OnCategoryTabClicked(ToolCategory.Traces));
            notebookButton.onClick.AddListener(() => OnCategoryTabClicked(ToolCategory.Photos));
            LoadInitialItems();
            adaptiveGridInventory.SetActive(false);
        }
        
        private void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (pickupAction.action.triggered)
            {
                TryPickupItem();
            }
        }
        
        private void OnCategoryTabClicked(ToolCategory category)
        {
            EnterCategory(category);
        }
        
        private void EnterCategory(ToolCategory category)
        {
            foreach (var tab in categoryTabs)
            {
                tab.SetActive(false);
            }

            foreach (var container in gridContainers)
            {
                if (container.name.Contains(category.ToString()))
                {
                    container.SetActive(true);
                }
            }
            
            ShowReturnButton();
            HideHeader();
            
            Debug.Log($"Вошли в категорию: {category}");
        }
        
        private void ReturnToMainMenu()
        {
            ShowAllCategories();
            
            HideReturnButton();
            
            ShowHeader();
            
            HideGridContainers();
        }
        
        private void ShowAllCategories()
        {
            foreach (var tab in categoryTabs)
            {
                tab.SetActive(true);
            }
        }
        
        private void ShowReturnButton()
        {
            returnButton.gameObject.SetActive(true);
        }
        
        private void HideReturnButton()
        {
            returnButton.gameObject.SetActive(false);
        }

        private void ShowHeader()
        {
            header.SetActive(true);
        }

        private void HideHeader()
        {
            header.SetActive(false);
        }
        
        private void HideGridContainers()
        {
            foreach (var container in gridContainers)
            {
                container.SetActive(false);
            }
        }
        
        private void LoadInitialItems()
        {
            ShowAllCategories();
            
            var itemDatabase = Resources.Load<ItemsDatabase>("ItemsDatabase");
            var inventoryItemDatabase = Resources.Load<InventoryItemDatabase>("InventoryItemDatabase");
            if (!inventoryItemDatabase)
            {
                Debug.LogError("Failed to load InventoryItemDatabase from Resources!");
                return;
            }
            if (!itemDatabase)
            {
                Debug.LogError("Failed to load ItemDatabase from Resources!");
                return;
            }

            foreach (var item in inventoryItemDatabase.GetAllItems())
            {
                inventoryItemDatabase.RemoveItem(item);
            }
            
            foreach (var toolId in Fabula1DefaultToolIds)
            {
                var item = itemDatabase.GetItemById<InventoryItem>(toolId);
                if (item)
                {
                    AddItem(item);
                }
            }
        }
        
        public void AddItem(InventoryItem item)
        {
            if (item.isStackable)
            {
                InventorySlot existingSlot = FindSlotWithItem(item.itemId);
                if (existingSlot)
                {
                    existingSlot.AddToStack();
                    return;
                }
            }
            
            AddItemToSlot(item);
        }
        
        private void AddItemToSlot(InventoryItem item)
        {
            foreach (var container in gridContainers)
            {
                if (container.name.Contains(item.category.ToString()))
                {
                    InventorySlot slot = Instantiate(slotPrefab, container.transform).GetComponent<InventorySlot>();
                    slot.SetItem(item, spawnPoint);
                    _slots.Add(slot);
                    Resources.Load<InventoryItemDatabase>("InventoryItemDatabase").AddItem(item);
                    if (item.timeOfPhoto.HasValue)
                    {
                        Debug.Log(item.timeOfPhoto.Value);
                    }
                    break;
                }
            }
        }
        
        private InventorySlot FindSlotWithItem(string itemId)
        {
            foreach (var slot in _slots)
            {
                if (slot.Item && slot.Item.itemId == itemId)
                    return slot;
            }
            
            return null;
        }
        
        public void ToggleInventory()
        {
            _isInventoryOpen = !_isInventoryOpen;
            
            adaptiveGridInventory.SetActive(_isInventoryOpen);
            
            if (_isInventoryOpen)
            {
                ReturnToMainMenu();
            }
        }
        
        private void TryPickupItem()
        {
            if (TryRaycastFromAssigned(raycastOrigin1)) return;
            TryRaycastFromAssigned(raycastOrigin2);
        }
        
        private bool TryRaycastFromAssigned(Transform origin)
        {
            if (!origin) return false;
            return TryRaycastFrom(origin.position, origin.forward);
        }
        
        private bool TryRaycastFrom(Vector3 origin, Vector3 direction)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, pickupDistance, itemLayerMask))
            {
                GameObject itemObject = hit.collider.gameObject;
                var pickupableItem = itemObject.GetComponent<PickupableItem>();
                if (itemObject.CompareTag("Convert"))
                {
                    Debug.Log("Поднят конверт");
                    PickupItem(itemObject.GetComponent<ConvertConfig>().ConvertFingerprint());
                    return true;
                }
                if (pickupableItem)
                {
                    Debug.Log("Поднят предмет");
                    PickupItem(pickupableItem);
                    return true;
                }
            }
            return false;
        }
        
        private void PickupItem(PickupableItem pickupableItem)
        {
            if (!pickupableItem) return;
            
            pickupableItem.Pickup(); // Проверка на ошибку подбора
            
            var inventoryItemDatabase = Resources.Load<InventoryItemDatabase>("InventoryItemDatabase");
            if (inventoryItemDatabase)
            {
                var item = inventoryItemDatabase.GetItemById(pickupableItem.ItemId);
                if (item)
                {
                    AddItem(item);
                    Destroy(pickupableItem.gameObject);
                }
                else
                {
                    var temp = Resources.Load<ItemsDatabase>("ItemsDatabase").GetItemById<InventoryItem>(pickupableItem.ItemId);
                    if (temp)
                    {
                        temp.displayName = pickupableItem.DisplayName;
                        temp.timeOfPhoto = pickupableItem.TimeOfPhoto;
                        Debug.Log("Создан существующий");
                        AddItem(temp);
                    }
                    else
                    {
                        InventoryItem newItem = inventoryItemDatabase.CreateItem(pickupableItem.ItemId, pickupableItem.DisplayName, pickupableItem.Icon,
                            pickupableItem.Prefab, pickupableItem.Category, pickupableItem.IsStackable, pickupableItem.MaxStackSize, pickupableItem.Description, pickupableItem.SurfaceName, pickupableItem.TimeOfPhoto);
                        AddItem(newItem);
                        Debug.Log("Создан из префаба");
                    }
                    Destroy(pickupableItem.gameObject);
                }
            }
        }
        
        public void AddItemToInventory(string itemId)
        {
            var itemDatabase = Resources.Load<InventoryItemDatabase>("InventoryItemDatabase");
            if (itemDatabase)
            {
                var item = itemDatabase.GetItemById(itemId);
                if (item)
                {
                    AddItem(item);
                }
            }
        }
        
        public bool HasItem(string itemId)
        {
            return FindSlotWithItem(itemId);
        }
        
        public void RemoveItem(string itemId)
        {
            var slot = FindSlotWithItem(itemId);
            if (slot)
            {
                slot.RemoveFromStack();
            }
        }
        
        public void RemoveEmptySlot(string itemId)
        {
            var slot = FindSlotWithItem(itemId);
            if (slot)
            {
                _slots.Remove(slot);
                Destroy(slot.gameObject);
            }
        }
    }
    
    [System.Serializable]
    public class InventoryCategory
    {
        public ToolCategory category;
        public string displayName;
        public Sprite categoryIcon;
    }
} 