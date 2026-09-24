using System;

namespace Systems.Omp
{
    [Serializable]
    public struct OmpActionLogEntry
    {
        public string ActionId;
        public DateTime TimestampUtc;
        public OmpActionContext Context;

        public OmpActionLogEntry(string actionId, DateTime timestampUtc, OmpActionContext context)
        {
            ActionId = actionId;
            TimestampUtc = timestampUtc;
            Context = context;
        }
    }
}

