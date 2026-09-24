using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "New Fingerprint", menuName = "Game/Evidence/Fingerprint Data")]
    public class FingerprintData : EvidenceData
    {
        public string OwnerName; // Опционально
    }
} 