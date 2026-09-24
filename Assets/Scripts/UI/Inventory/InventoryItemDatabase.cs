using System;
using System.Collections.Generic;
using System.IO;
using Data;
using Items;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemDatabase", menuName = "Inventory/Item Database")]
    public class InventoryItemDatabase : ScriptableObject
    {
        [SerializeField] private List<InventoryItem> allItems = new List<InventoryItem>();
        
        private Dictionary<string, InventoryItem> _itemLookup = new Dictionary<string, InventoryItem>();
        private Dictionary<ToolCategory, List<InventoryItem>> _categoryLookup = new Dictionary<ToolCategory, List<InventoryItem>>();
        
        private void OnEnable()
        {
            BuildLookupTables();
        }
        
        private void BuildLookupTables()
        {
            _itemLookup.Clear();
            _categoryLookup.Clear();
            
            foreach (var item in allItems)
            {
                if (!item) continue;
                if (!string.IsNullOrEmpty(item.itemId))
                {
                    _itemLookup[item.itemId] = item; // индексер перезаписывает дубликаты, не бросает
                }

                if (!_categoryLookup.ContainsKey(item.category))
                {
                    _categoryLookup[item.category] = new List<InventoryItem>();
                }
                if (!_categoryLookup[item.category].Contains(item))
                {
                    _categoryLookup[item.category].Add(item);
                }
            }
        }
        
        public InventoryItem GetItemById(string itemId)
        {
            return _itemLookup.TryGetValue(itemId, out var item) ? item : null;
        }
        
        public List<InventoryItem> GetItemsByCategory(ToolCategory category)
        {
            return _categoryLookup.TryGetValue(category, out var items) ? items : new List<InventoryItem>();
        }
        
        public List<InventoryItem> GetAllItems()
        {
            return new List<InventoryItem>(allItems);
        }
        
        public void AddItem(InventoryItem item)
        {
            if (item && !allItems.Contains(item))
            {
                allItems.Add(item);
                BuildLookupTables();
            }
        }
        
        public void RemoveItem(InventoryItem item)
        {
            if (allItems.Remove(item))
            {
                BuildLookupTables();
            }
        }
        
        public void RemoveItemById(string itemId)
        {
            var item = GetItemById(itemId);
            if (item)
            {
                RemoveItem(item);
            }
        }
        
        public InventoryItem CreateItem(string itemId, string displayName, Sprite icon, GameObject prefab, ToolCategory category, bool isStackable, int maxStackSize, string description = "", string surfaceName = "", DateTime? timeOfPhoto = null)
        {
            var item = CreateInstance<InventoryItem>();
            
            item.itemId = itemId;
            item.displayName = displayName;
            item.icon = icon;
            item.prefab = prefab;
            item.category = category;
            item.description = description;
            item.isStackable = isStackable;
            item.maxStackSize = maxStackSize;
            item.surfaceName = surfaceName;
            item.timeOfPhoto = timeOfPhoto;
            
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(item, $"Assets/Resources/Data/Items/{itemId}Item.asset");
            AssetDatabase.SaveAssets();
#endif
            
            ItemsDatabase.Instance.AddNewItemToDatabase(item);
            
            AddItem(item);
            return item;
        }
        
        public void SaveToPlayerPrefs()
        {
            // Это можно расширить для сохранения прогресса
        }
        
        public void LoadFromPlayerPrefs()
        {
            // Загружаем состояние инвентаря
        }
    }
} 