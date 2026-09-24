using UnityEngine;
using UnityEditor;
using System.IO;
using Data;

namespace Items.Editor
{
    public class EvidenceDatabaseCreator
    {
        [MenuItem("Tools/Create Evidence Database")]
        public static void CreateEvidenceDatabase()
        {
            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }
            
            var database = Resources.Load<EvidenceDatabase>("EvidenceDatabase");
            
            if (!database)
            {
                database = ScriptableObject.CreateInstance<EvidenceDatabase>();
                AssetDatabase.CreateAsset(database, "Assets/Resources/EvidenceDatabase.asset");
                AssetDatabase.SaveAssets();
                Debug.Log("EvidenceDatabase created in Resources folder");
            }
            else
            {
                Debug.Log("EvidenceDatabase already exists");
            }
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = database;
        }
    }
} 