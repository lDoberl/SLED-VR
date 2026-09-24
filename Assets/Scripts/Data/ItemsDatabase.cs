using System.Collections.Generic;
using System.Linq;
using Items;
using UI.Inventory;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ItemsDatabase", menuName = "Game/Items Database")]
    public class ItemsDatabase : ScriptableObject
    {
        public List<InventoryItem> AllItems = new List<InventoryItem>();
        
        private static ItemsDatabase _instance;
        
        public static ItemsDatabase Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.Load<ItemsDatabase>("ItemsDatabase");
                }
                return _instance;
            }
        }

        public void AddNewItemToDatabase(InventoryItem item)
        {
            AllItems.Add(item);
        }

        public T GetItemById<T>(string id) where T : InventoryItem
        {
            return AllItems.Find(i => i.itemId == id) as T;
        }

        public List<T> GetAllItemsOfType<T>() where T : InventoryItem
        {
            return AllItems.OfType<T>().ToList();
        }
    }
}
