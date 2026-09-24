namespace CSI.Runtime
{
    public enum ProtocolRule
    {
        CollectedBeforePhoto,
        MovedBeforePhoto
    }

    [System.Serializable]
    public class PenaltyEntry
    {
        public ProtocolRule Rule;
        public string EvidenceId;
        public float Points;
    }
}
