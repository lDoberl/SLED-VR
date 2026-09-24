using System.Collections.Generic;
using UnityEngine;

namespace CSI.Data
{
    [CreateAssetMenu(fileName = "NewCaseDefinition", menuName = "CSI/Case Definition")]
    public class CaseDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string caseId;
        public string displayName;
        public string sceneName;

        [Header("Evidence")]
        public List<EvidencePiece> evidencePieces = new List<EvidencePiece>();

        [Header("Starting equipment (always in the Tools tab from the start, not \"discovered\")")]
        public List<EvidencePiece> startingTools = new List<EvidencePiece>();

        [Header("Notebook")]
        [TextArea(4, 12)] public string notebookLeadText;

        [Header("Final report answer key")]
        public List<AnswerOption> motiveOptions = new List<AnswerOption>();
        public List<AnswerOption> causeOfDeathOptions = new List<AnswerOption>();
        public List<AnswerOption> suspectOptions = new List<AnswerOption>();

        public EvidencePiece FindEvidence(string evidenceId)
        {
            for (int i = 0; i < evidencePieces.Count; i++)
            {
                if (evidencePieces[i] != null && evidencePieces[i].evidenceId == evidenceId)
                    return evidencePieces[i];
            }
            return null;
        }
    }
}
