using System;
using Data;
using UI.Inventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Items
{
    public class Scotch : MonoBehaviour
    {
        private AdaptiveGridInventory _inventory;
        private bool _isActive;

        private void Start()
        {
            _inventory = FindAnyObjectByType<AdaptiveGridInventory>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fingerprint") && !_isActive)
            {
                _isActive = true;
                var itemsDatabase = Resources.Load<ItemsDatabase>("ItemsDatabase");
                if (itemsDatabase)
                {
                    var item = itemsDatabase.GetItemById<InventoryItem>(other.GetComponent<Fingerprint>().FingerprintId);
                    other.GetComponent<Fingerprint>().DeActivate(item, _inventory);
                    Debug.Log("Добавлен отпечаток");
                }

                _isActive = false;
            }
        }
    }
}
