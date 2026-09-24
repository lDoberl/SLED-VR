using UnityEngine;

namespace CSI.Data
{
    [CreateAssetMenu(fileName = "NewEvidencePiece", menuName = "CSI/Evidence Piece")]
    public class EvidencePiece : ScriptableObject
    {
        [Header("Identity")]
        public string evidenceId;
        public EvidenceCategory category;
        public CollectionMethod collectionMethod;
        public bool requiresPhotoBeforeCollection = true;

        [Header("Display — before analysis")]
        public string visibleDisplayName;
        [TextArea(2, 4)] public string visibleDescription;

        [Header("Display — after analysis/collection")]
        public string hiddenDisplayName;
        [TextArea(2, 4)] public string hiddenDescription;

        [Header("Fingerprint-specific")]
        public string ownerNameIfFingerprint;

        [Header("Presentation")]
        public Sprite icon;

        [Header("World pickup (for standing equipment — spawned into the player's hand from the Tools tab)")]
        public GameObject worldPrefab;
        public int startingQuantity = 1;

        [Header("Design metadata (not shown to player)")]
        public bool isRedHerring;
    }
}
