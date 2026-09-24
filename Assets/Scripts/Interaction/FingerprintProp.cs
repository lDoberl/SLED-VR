using System.Collections;
using CSI.Runtime;
using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Sits on a fingerprint prop instance (e.g. on a safe handle or a glass).
    /// Hidden until dusted by DustingBrush, then must be photographed before TapeLifter can collect it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FingerprintProp : MonoBehaviour, IPhotographable
    {
        [SerializeField] private string evidenceId;
        [SerializeField] private string surfaceName;
        [Tooltip("Visual shown once dusted (decal/mesh). Left inactive until discovered.")]
        [SerializeField] private GameObject revealVisual;

        public string EvidenceId => evidenceId;
        public Bounds WorldBounds
        {
            get
            {
                var col = GetComponent<Collider>();
                return col != null ? col.bounds : new Bounds(transform.position, Vector3.one * 0.1f);
            }
        }

        private void Start()
        {
            if (revealVisual != null)
                revealVisual.SetActive(false);

            StartCoroutine(RegisterNextFrame());
        }

        private IEnumerator RegisterNextFrame()
        {
            // EvidenceManager's own Start() populates its per-evidence state on the same frame;
            // Unity doesn't guarantee Start() call order across objects, so wait one frame before
            // touching that state (same fix already used for InventoryController's startup race).
            yield return null;

            if (EvidenceManager.Instance == null)
                yield break;

            EvidenceManager.Instance.SetSurfaceName(evidenceId, surfaceName);
            EvidenceManager.Instance.OnDiscovered += HandleDiscovered;
            EvidenceManager.Instance.OnCollected += HandleCollected;

            if (EvidenceManager.Instance.IsDiscovered(evidenceId))
                HandleDiscovered(evidenceId);
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
            {
                EvidenceManager.Instance.OnDiscovered -= HandleDiscovered;
                EvidenceManager.Instance.OnCollected -= HandleCollected;
            }
            PhotographableRegistry.Unregister(this);
        }

        private void HandleDiscovered(string id)
        {
            if (id != evidenceId) return;
            if (revealVisual != null)
                revealVisual.SetActive(true);
            PhotographableRegistry.Register(this);
        }

        private void HandleCollected(string id)
        {
            if (id != evidenceId) return;
            PhotographableRegistry.Unregister(this);
            gameObject.SetActive(false);
        }
    }
}
