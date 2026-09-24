using System;
using System.IO;
using UnityEngine;

namespace CSI.Runtime
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string SaveFileName = "savegame.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

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

        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        public void Save(SaveData data)
        {
            data.savedAtUtc = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public SaveData Load()
        {
            if (!HasSaveFile())
                return null;

            try
            {
                string json = File.ReadAllText(SavePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load save file: {ex.Message}");
                return null;
            }
        }

        public void DeleteSave()
        {
            if (HasSaveFile())
                File.Delete(SavePath);
        }
    }
}
