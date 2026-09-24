using System.Collections.Generic;
using Config.Attributes;
using UnityEngine;

namespace UI.Inventory
{
    [CreateAssetMenu(fileName = "ItemsDatabase", menuName = "Game/New UIItem")]
    public class InventoryUIItem : InventoryItem
    {
        public string additionalInfo;
        
        [Space(15)]
        public GameObject UIprefab;
        public List<ResponseOptions> selectionOfMotives = new List<ResponseOptions>();
        public List<ResponseOptions> selectionOfDeaths = new List<ResponseOptions>();
        public List<ResponseOptions> selectionOfSuspects = new List<ResponseOptions>();
    }

    [System.Serializable]
    public class ResponseOptions
    {
        public bool IsCorrect;
        public string ResponseName;

        public ResponseOptions(bool isCorrect, string responseName)
        {
            IsCorrect = isCorrect;
            ResponseName = responseName;
        }
    }
}
