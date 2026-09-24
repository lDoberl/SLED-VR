using System.Collections.Generic;
using UnityEngine;

namespace CSI.Data
{
    [CreateAssetMenu(fileName = "CaseDatabase", menuName = "CSI/Case Database")]
    public class CaseDatabase : ScriptableObject
    {
        public List<CaseDefinition> cases = new List<CaseDefinition>();

        private static CaseDatabase _instance;
        public static CaseDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<CaseDatabase>("CaseDatabase");
                return _instance;
            }
        }

        public CaseDefinition FindCase(string caseId)
        {
            for (int i = 0; i < cases.Count; i++)
            {
                if (cases[i] != null && cases[i].caseId == caseId)
                    return cases[i];
            }
            return null;
        }
    }
}
