using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systems.Omp
{
    [Serializable]
    public class OmpActionStep
    {
        [Tooltip("Unique identifier used when reporting the completion of this step.")]
        public string ActionId;

        [Tooltip("Human friendly name that will appear inside penalty summaries.")]
        public string DisplayName;

        [Tooltip("Penalty applied when the step is completed out of sequence.")]
        [Min(0f)]
        public float SequencePenalty = 5f;

        [Tooltip("Penalty applied if the step is never completed by the time the report is signed.")]
        [Min(0f)]
        public float SkipPenalty = 10f;

        [Tooltip("Penalty applied when the same step is repeated without need.")]
        [Min(0f)]
        public float RepeatPenalty = 2f;

        [Tooltip("Optional list of tool identifiers that are considered valid for this step. Leave empty to allow any tool.")]
        public List<string> AllowedToolIds = new List<string>();

        public bool IsToolAllowed(string toolId)
        {
            if (AllowedToolIds == null || AllowedToolIds.Count == 0 || string.IsNullOrEmpty(toolId))
                return true;

            return AllowedToolIds.Contains(toolId);
        }
    }
}

