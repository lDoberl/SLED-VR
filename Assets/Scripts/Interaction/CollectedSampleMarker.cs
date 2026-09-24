using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Runtime-spawned marker representing a lifted trace sample stuck to the tape,
    /// waiting to be dropped into an EvidenceEnvelope to be sealed.
    /// </summary>
    public class CollectedSampleMarker : MonoBehaviour
    {
        public string EvidenceId { get; private set; }

        public void Initialize(string evidenceId)
        {
            EvidenceId = evidenceId;
        }
    }
}
