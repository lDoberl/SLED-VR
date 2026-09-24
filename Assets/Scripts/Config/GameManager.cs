using Audio;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Items;

namespace Config
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        public bool IsPaused;
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            AudioManager.Instance.PlayMusic("Test");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetEvidence();
        }

        private void ResetEvidence()
        {
            EvidenceDatabase database = EvidenceDatabase.Instance;
            
            if (!database) return;
            
            foreach (var evidence in database.AllEvidence)
            {
                evidence.IsDiscovered = false;
            }
                
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(database);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
    }
}
