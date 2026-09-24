using CSI.Runtime;
using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Handheld dusting brush (XRGrabInteractable). Touching an undiscovered fingerprint reveals it.
    /// </summary>
    public class DustingBrush : MonoBehaviour
    {
        [SerializeField] private Collider brushTip;

        private void Reset()
        {
            if (brushTip == null)
                brushTip = GetComponentInChildren<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var print = other.GetComponentInParent<FingerprintProp>();
            if (print == null) return;
            if (EvidenceManager.Instance == null) return;

            if (!EvidenceManager.Instance.IsDiscovered(print.EvidenceId))
                EvidenceManager.Instance.DiscoverEvidence(print.EvidenceId);
        }
    }
}
