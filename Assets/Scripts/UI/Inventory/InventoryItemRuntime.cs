using CSI.Data;
using UnityEngine;

namespace CSI.UI.Inventory
{
    /// <summary>
    /// Plain display-data class for a collected/photographed item shown in the inventory UI.
    /// Built on demand from an EvidencePiece (static definition) + EvidenceManager state (runtime).
    /// </summary>
    public class InventoryItemRuntime
    {
        public string EvidenceId;
        public string DisplayName;
        public string Description;
        public Sprite Icon;
        public EvidenceCategory Category;
        public string SurfaceName;
        public string PhotoTimestampUtc;
        public GameObject WorldPrefab;
    }
}
