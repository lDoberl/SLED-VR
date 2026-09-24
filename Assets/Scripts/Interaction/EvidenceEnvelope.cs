using CSI.Runtime;
using TMPro;
using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Trigger volume representing an evidence envelope. Any CollectedSampleMarker (a lifted
    /// fingerprint sample carried on the tape) dropped inside gets sealed/filed and removed.
    /// The evidence was already marked collected at lift time — this is the narrative "filing" step.
    /// Fills in the envelope's printed "date/place collected" fields so sealing it visibly does
    /// something instead of just silently consuming the sample.
    /// </summary>
    public class EvidenceEnvelope : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI placeText;

        /// <summary>Static so listeners (e.g. the tutorial) can react without needing a fixed scene
        /// reference — the envelope is normally spawned from the inventory, not a fixed prop.</summary>
        public static event System.Action<string> OnSealed;

        private void OnTriggerEnter(Collider other)
        {
            // The sample marker is spawned as a child of the tape (see TapeLifter.SpawnSampleMarker),
            // i.e. a descendant of the collider that just touched us — not an ancestor — so this has
            // to search children, not parents.
            var marker = other.GetComponentInChildren<CollectedSampleMarker>();
            if (marker == null) return;

            FillEnvelopeLabel(marker.EvidenceId);

            Debug.Log($"[EvidenceEnvelope] Sealed sample '{marker.EvidenceId}' into envelope.");
            OnSealed?.Invoke(marker.EvidenceId);
            Destroy(marker.gameObject);
        }

        private void FillEnvelopeLabel(string evidenceId)
        {
            if (dateText != null)
                dateText.text = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            if (placeText != null)
            {
                var state = EvidenceManager.Instance != null ? EvidenceManager.Instance.GetState(evidenceId) : null;
                placeText.text = state != null && !string.IsNullOrEmpty(state.SurfaceName) ? state.SurfaceName : evidenceId;
            }
        }
    }
}
