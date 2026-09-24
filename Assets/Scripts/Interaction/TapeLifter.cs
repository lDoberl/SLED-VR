using CSI.Runtime;
using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Handheld tape (XRGrabInteractable). Touching a discovered-but-not-yet-collected fingerprint
    /// lifts it: collects the evidence and attaches a sample marker to the tape to be sealed later
    /// in an EvidenceEnvelope. If the print wasn't photographed first, the collection still succeeds
    /// (protocol violations are penalized, not blocked) — ProtocolScoringSystem checks
    /// EvidenceManager.IsPhotographed(id) at the moment EvidenceManager.OnCollected fires.
    /// </summary>
    public class TapeLifter : MonoBehaviour
    {
        [SerializeField] private Transform sampleAttachPoint;

        private void OnTriggerEnter(Collider other)
        {
            var print = other.GetComponentInParent<FingerprintProp>();
            if (print == null) return;
            if (EvidenceManager.Instance == null) return;

            string id = print.EvidenceId;
            if (!EvidenceManager.Instance.IsDiscovered(id)) return;
            if (EvidenceManager.Instance.IsCollected(id)) return;

            EvidenceManager.Instance.CollectEvidence(id);
            SpawnSampleMarker(id);
        }

        private void SpawnSampleMarker(string evidenceId)
        {
            var parent = sampleAttachPoint != null ? sampleAttachPoint : transform;
            var markerObj = new GameObject($"Sample_{evidenceId}");
            markerObj.transform.SetParent(parent, false);
            var marker = markerObj.AddComponent<CollectedSampleMarker>();
            marker.Initialize(evidenceId);
        }
    }
}
