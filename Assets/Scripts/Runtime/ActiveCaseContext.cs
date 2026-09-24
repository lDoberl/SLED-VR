using CSI.Data;
using UnityEngine;

namespace CSI.Runtime
{
    public class ActiveCaseContext : MonoBehaviour
    {
        public static ActiveCaseContext Instance { get; private set; }

        public CaseDefinition CurrentCase { get; private set; }
        public bool IsNewGame { get; private set; } = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void SetCase(CaseDefinition caseDefinition, bool isNewGame)
        {
            CurrentCase = caseDefinition;
            IsNewGame = isNewGame;
        }
    }
}
