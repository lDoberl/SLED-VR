using System;
using UnityEngine;

namespace UI.Inventory.Additional_UI
{
    public class UIDestroyer : MonoBehaviour
    {
        private void OnDisable()
        {
            DestroyButton();
        }

        public void DestroyButton()
        {
            Destroy(this.gameObject);
        }
    }
}
