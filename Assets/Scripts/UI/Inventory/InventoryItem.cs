using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Inventory
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ItemsDatabase", menuName = "Game/New Item")]
    public class InventoryItem : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public GameObject prefab;
        public ToolCategory category;
        public Vector3 spawnOffset = Vector3.zero;
        public Quaternion spawnRotation = Quaternion.identity;
        public bool isStackable = false;
        public int maxStackSize = 1;
        [Header("Special Options")]
        public bool isInfinite = false;
        public string surfaceName;
        public DateTime? timeOfPhoto;
        public string hiddenDisplayName;
        
        [TextArea(2, 4)]
        public string description;
        
        [TextArea(2, 4)]
        public string hiddenDescription;
        
        [Header("Available Actions")]
        [SerializeField]
        public List<ItemActionType> availableActions = new List<ItemActionType>();
        
        public InventoryItem CreateCopy()
        {
            var copy = CreateInstance<InventoryItem>();
            copy.itemId = this.itemId;
            copy.displayName = this.displayName;
            copy.icon = this.icon;
            copy.prefab = this.prefab;
            copy.category = this.category;
            copy.spawnOffset = this.spawnOffset;
            copy.spawnRotation = this.spawnRotation;
            copy.isStackable = this.isStackable;
            copy.maxStackSize = this.maxStackSize;
            copy.isInfinite = this.isInfinite;
            copy.surfaceName = this.surfaceName;
            copy.timeOfPhoto = this.timeOfPhoto;
            copy.hiddenDisplayName = this.hiddenDisplayName;
            copy.description = this.description;
            copy.hiddenDescription = this.hiddenDescription;
            copy.availableActions = new List<ItemActionType>(this.availableActions);
            return copy;
        }
        
        public void RevealHiddenData()
        {
            if (hiddenDisplayName != null) displayName = hiddenDisplayName;
            if (hiddenDescription != null) description = hiddenDescription;
            availableActions.Clear();
        }
    }
    
    public enum ToolCategory
    {
        Tools,      // Инструменты
        Traces,      // Следы
        Photos // Фото
    }
    
    [System.Serializable]
    public enum ItemActionType
    {
        Take,
        Analysis,
        Discover,
        OpenAdditionalInfo,
        OpenFinalReport
    }
} 