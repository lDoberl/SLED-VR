using System;
using System.Collections.Generic;

namespace CSI.Runtime
{
    [Serializable]
    public class SaveData
    {
        public string caseId;
        public List<EvidenceSaveEntry> evidence = new List<EvidenceSaveEntry>();
        public List<InventorySaveEntry> inventory = new List<InventorySaveEntry>();
        public float protocolScore = 100f;
        public List<string> penaltyIds = new List<string>();
        public bool tutorialCompleted;
        public string savedAtUtc;
    }

    [Serializable]
    public class EvidenceSaveEntry
    {
        public string id;
        public bool discovered;
        public bool collected;
        public string photoTimestampUtc;
        public string surfaceName;
    }

    [Serializable]
    public class InventorySaveEntry
    {
        public string evidenceId;
        public int count;
    }
}
