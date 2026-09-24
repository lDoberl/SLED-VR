using System;

namespace Systems.Omp
{
    [Serializable]
    public enum OmpPenaltyType
    {
        WrongOrder,
        Skip,
        Repeat,
        ToolViolation,
        Custom,
        UnknownAction
    }

    [Serializable]
    public class OmpPenaltyEntry
    {
        public string PenaltyId;
        public string Reason;
        public float Points;
        public string RelatedActionId;
        public string Context;
        public DateTime TimestampUtc;
        public OmpPenaltyType Type;
    }
}

