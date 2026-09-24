using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CSI.UI.Inventory
{
    /// <summary>
    /// A single clickable inventory slot: icon + short name, with a selection highlight.
    /// Clicking it reports its full item data so the panel can show a detail view; hovering
    /// (ray enters/exits the slot, no click needed) does the same so VR players can preview an
    /// item before committing to an action. An optional quantity badge and delete button support
    /// limited-stock tools and a deletable photo gallery respectively.
    /// </summary>
    public class InventorySlotView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject quantityBadge;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button deleteButton;

        [SerializeField] private Color normalColor = new Color(0.16f, 0.16f, 0.17f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.79f, 0.68f, 0.5f, 1f);
        [SerializeField] private Color depletedColor = new Color(0.09f, 0.09f, 0.1f, 1f);

        public InventoryItemRuntime Item { get; private set; }
        public event Action<InventorySlotView> OnClicked;
        public event Action<InventorySlotView> OnHoverEnter;
        public event Action<InventorySlotView> OnHoverExit;
        public event Action<InventorySlotView> OnDeleteRequested;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(() => OnClicked?.Invoke(this));
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => OnDeleteRequested?.Invoke(this));
                deleteButton.gameObject.SetActive(false);
            }
        }

        public void SetData(InventoryItemRuntime item)
        {
            Item = item;

            if (icon != null)
            {
                icon.sprite = item.Icon;
                icon.enabled = item.Icon != null;
            }

            if (nameText != null)
                nameText.text = item.DisplayName;

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
                background.color = selected ? selectedColor : normalColor;
        }

        /// <summary>Shows/updates the quantity badge. Pass a negative count to hide it entirely
        /// (used for items that don't have a limited stock, e.g. traces/photos).</summary>
        public void SetQuantity(int count)
        {
            if (quantityBadge != null)
                quantityBadge.SetActive(count >= 0);

            if (quantityText != null)
                quantityText.text = count.ToString();

            bool depleted = count == 0;
            if (button != null)
                button.interactable = !depleted;
            if (background != null)
                background.color = depleted ? depletedColor : normalColor;
        }

        public void EnableDeleteButton()
        {
            if (deleteButton != null)
                deleteButton.gameObject.SetActive(true);
        }

        public void OnPointerEnter(PointerEventData eventData) => OnHoverEnter?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => OnHoverExit?.Invoke(this);
    }
}
