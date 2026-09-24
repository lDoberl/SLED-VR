using System;
using UnityEngine;

namespace Data
{
    public abstract class EvidenceData : ScriptableObject
    {
        public string EvidenceId;
        public bool IsDiscovered;
        public DateTime? TimeOfPhoto;
        public Vector3 OriginalPosition; // Опционально
    }
} 