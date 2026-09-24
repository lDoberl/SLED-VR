using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EvidenceDatabase", menuName = "Game/Evidence Database")]
    public class EvidenceDatabase : ScriptableObject
    {
        public List<EvidenceData> AllEvidence = new List<EvidenceData>();
        
        private static EvidenceDatabase _instance;
        
        public static EvidenceDatabase Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = Resources.Load<EvidenceDatabase>("EvidenceDatabase");
                    
                    if (!_instance)
                    {
                        Debug.LogError("EvidenceDatabase not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }

        public T GetEvidenceById<T>(string id) where T : EvidenceData
        {
            return AllEvidence.Find(e => e.EvidenceId == id) as T;
        }

        public List<T> GetAllEvidenceOfType<T>() where T : EvidenceData
        {
            return AllEvidence.OfType<T>().ToList();
        }

        public void DiscoverEvidence(string id)
        {
            var evidence = AllEvidence.Find(e => e.EvidenceId == id);
            if (evidence)
            {
                evidence.IsDiscovered = true;
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
                #endif
            }
        }

        public List<EvidenceData> GetDiscoveredEvidence()
        {
            return AllEvidence.FindAll(e => e.IsDiscovered);
        }

        public List<EvidenceData> GetUndiscoveredEvidence()
        {
            return AllEvidence.FindAll(e => !e.IsDiscovered);
        }

        public List<T> GetDiscoveredEvidenceOfType<T>() where T : EvidenceData
        {
            return GetAllEvidenceOfType<T>().FindAll(e => e.IsDiscovered);
        }

        public List<T> GetUndiscoveredEvidenceOfType<T>() where T : EvidenceData
        {
            return GetAllEvidenceOfType<T>().FindAll(e => !e.IsDiscovered);
        }
    }
} 