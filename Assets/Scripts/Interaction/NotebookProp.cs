using CSI.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CSI.Interaction
{
    /// <summary>
    /// Handheld notebook (XRGrabInteractable). Shows the active case's investigative lead text
    /// directly on the physical prop's cover, so reading it is just "look at what you're holding"
    /// instead of opening a separate floating panel.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class NotebookProp : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI leadText;

        /// <summary>Static so the tutorial can react without needing a fixed scene reference — the
        /// notebook is spawned from the inventory, not a fixed prop.</summary>
        public static event System.Action OnGrabbedByPlayer;

        private XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            _grabInteractable.selectEntered.AddListener(HandleSelectEntered);
        }

        private void OnDisable()
        {
            _grabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
        }

        private void Start()
        {
            var activeCase = ActiveCaseContext.Instance != null ? ActiveCaseContext.Instance.CurrentCase : null;
            if (leadText != null && activeCase != null)
                leadText.text = activeCase.notebookLeadText;
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            OnGrabbedByPlayer?.Invoke();
        }
    }
}
