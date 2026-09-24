using System;
using CSI.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CSI.UI.FinalReport
{
    /// <summary>
    /// A single selectable answer row (button/radio-list style — chosen over TMP_Dropdown
    /// because dropdowns are unreliable with VR ray interactors).
    /// </summary>
    public class FinalReportOptionView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject selectedIndicator;

        private AnswerOption _option;
        private Action<AnswerOption> _onSelect;

        public void SetData(AnswerOption option, Action<AnswerOption> onSelect)
        {
            _option = option;
            _onSelect = onSelect;

            if (label != null)
                label.text = option.text;

            SetSelected(false);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _onSelect?.Invoke(_option);

            foreach (Transform sibling in transform.parent)
            {
                var view = sibling.GetComponent<FinalReportOptionView>();
                if (view != null)
                    view.SetSelected(view == this);
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null)
                selectedIndicator.SetActive(selected);
        }
    }
}
