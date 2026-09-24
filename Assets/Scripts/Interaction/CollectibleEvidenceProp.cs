using CSI.Runtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CSI.Interaction
{
    /// <summary>
    /// A directly grabbable piece of physical evidence (syringe, cash, contract, hair, shot glass, etc).
    /// Visible/known from scene start (auto-discovered); grabbing it collects it. If it hasn't been
    /// photographed first, the grab still succeeds but is flagged for ProtocolScoringSystem to penalize
    /// (checked via EvidenceManager.IsPhotographed at the moment OnCollected fires).
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class CollectibleEvidenceProp : MonoBehaviour, IPhotographable
    {
        [SerializeField] private string evidenceId;
        [SerializeField] private string surfaceName;

        public string EvidenceId => evidenceId;
        public Bounds WorldBounds
        {
            get
            {
                var col = GetComponent<Collider>();
                return col != null ? col.bounds : new Bounds(transform.position, Vector3.one * 0.1f);
            }
        }

        private XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            _grabInteractable.selectEntered.AddListener(HandleSelectEntered);
            PhotographableRegistry.Register(this);
        }

        private void OnDisable()
        {
            _grabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
            PhotographableRegistry.Unregister(this);
        }

        private void Start()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.SetSurfaceName(evidenceId, surfaceName);
                EvidenceManager.Instance.DiscoverEvidence(evidenceId);
            }
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            if (EvidenceManager.Instance == null) return;
            if (EvidenceManager.Instance.IsCollected(evidenceId)) return;

            if (!EvidenceManager.Instance.IsPhotographed(evidenceId))
                EvidenceManager.Instance.MarkMovedBeforePhoto(evidenceId);

            EvidenceManager.Instance.CollectEvidence(evidenceId);
        }
    }
}
