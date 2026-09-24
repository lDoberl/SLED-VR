using System.Collections.Generic;

namespace Systems.Omp
{
    public class OmpActionAnalysisResult
    {
        public float TotalPenalty;
        public IReadOnlyList<OmpPenaltyEntry> Penalties;
        public IReadOnlyList<OmpActionLogEntry> Log;
        public IReadOnlyList<string> CompletedActionIds;

        public OmpActionAnalysisResult(float totalPenalty,
            IReadOnlyList<OmpPenaltyEntry> penalties,
            IReadOnlyList<OmpActionLogEntry> log,
            IReadOnlyList<string> completedActionIds)
        {
            TotalPenalty = totalPenalty;
            Penalties = penalties;
            Log = log;
            CompletedActionIds = completedActionIds;
        }
    }
}

