using System;
using System.Collections.Generic;
using CSI.Data;
using UnityEngine;

namespace CSI.Runtime
{
    public class EvidenceRuntimeState
    {
        public bool IsDiscovered;
        public bool IsCollected;
        public string PhotoTimestampUtc;
        public bool WasMovedBeforePhoto;
        public string SurfaceName;
    }

    /// <summary>
    /// Scene-scoped (not DontDestroyOnLoad) per-playthrough evidence state.
    /// Never writes to ScriptableObject assets — only in-memory + SaveSystem JSON.
    /// </summary>
    public class EvidenceManager : MonoBehaviour
    {
        public static EvidenceManager Instance { get; private set; }

        public event Action<string> OnDiscovered;
        public event Action<string> OnPhotographed;
        public event Action<string> OnCollected;

        private readonly Dictionary<string, EvidenceRuntimeState> _state = new Dictionary<string, EvidenceRuntimeState>();
        private CaseDefinition _activeCase;
        private bool _tutorialCompleted;

        public CaseDefinition ActiveCase => _activeCase;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _activeCase = ActiveCaseContext.Instance != null ? ActiveCaseContext.Instance.CurrentCase : null;
            if (_activeCase == null)
            {
                Debug.LogWarning("[EvidenceManager] No active case set — evidence tracking will be empty.");
                return;
            }

            foreach (var piece in _activeCase.evidencePieces)
            {
                if (piece == null) continue;
                _state[piece.evidenceId] = new EvidenceRuntimeState();
            }

            bool isNewGame = ActiveCaseContext.Instance == null || ActiveCaseContext.Instance.IsNewGame;
            if (!isNewGame && SaveSystem.Instance != null)
            {
                var save = SaveSystem.Instance.Load();
                if (save != null && save.caseId == _activeCase.caseId)
                {
                    ApplySaveData(save);
                    _tutorialCompleted = save.tutorialCompleted;
                }
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused) TriggerAutosave();
        }

        private void OnApplicationQuit()
        {
            TriggerAutosave();
        }

        public void MarkTutorialCompleted()
        {
            _tutorialCompleted = true;
            TriggerAutosave();
        }

        public void TriggerAutosave()
        {
            if (SaveSystem.Instance == null || _activeCase == null) return;

            float score = ProtocolScoringSystem.Instance != null ? ProtocolScoringSystem.Instance.CurrentScore : 100f;
            var penaltyIds = ProtocolScoringSystem.Instance != null ? ProtocolScoringSystem.Instance.BuildPenaltyIdList() : new List<string>();
            SaveSystem.Instance.Save(BuildSaveData(score, penaltyIds, _tutorialCompleted));
        }

        public EvidencePiece GetPiece(string evidenceId)
        {
            return _activeCase != null ? _activeCase.FindEvidence(evidenceId) : null;
        }

        public EvidenceRuntimeState GetState(string evidenceId)
        {
            _state.TryGetValue(evidenceId, out var s);
            return s;
        }

        public bool IsDiscovered(string evidenceId) => _state.TryGetValue(evidenceId, out var s) && s.IsDiscovered;
        public bool IsCollected(string evidenceId) => _state.TryGetValue(evidenceId, out var s) && s.IsCollected;
        public bool IsPhotographed(string evidenceId) => _state.TryGetValue(evidenceId, out var s) && !string.IsNullOrEmpty(s.PhotoTimestampUtc);

        public void DiscoverEvidence(string evidenceId)
        {
            if (!_state.TryGetValue(evidenceId, out var s)) return;
            if (s.IsDiscovered) return;
            s.IsDiscovered = true;
            OnDiscovered?.Invoke(evidenceId);
            // NEW
            if (DatabaseManager.Instance != null)
            {
                string timeNow = DateTime.Now.ToString("HH:mm:ss");
                DatabaseManager.Instance.SaveEvidenceState(evidenceId, true, timeNow);
            }
        }

        public void MarkPhotographed(string evidenceId)
        {
            if (!_state.TryGetValue(evidenceId, out var s)) return;
            if (!string.IsNullOrEmpty(s.PhotoTimestampUtc)) return;
            s.PhotoTimestampUtc = DateTime.UtcNow.ToString("o");
            OnPhotographed?.Invoke(evidenceId);
            TriggerAutosave();
            //NEW
            if (DatabaseManager.Instance != null)
            {
                string timeNow = DateTime.Now.ToString("HH:mm:ss");
                // Благодаря SQL-логике ON CONFLICT, запись просто обновит время фото
                DatabaseManager.Instance.SaveEvidenceState(evidenceId, true, timeNow);
            }
        }

        public void MarkMovedBeforePhoto(string evidenceId)
        {
            if (!_state.TryGetValue(evidenceId, out var s)) return;
            s.WasMovedBeforePhoto = true;
        }

        public void SetSurfaceName(string evidenceId, string surfaceName)
        {
            if (!_state.TryGetValue(evidenceId, out var s)) return;
            s.SurfaceName = surfaceName;
        }

        public void CollectEvidence(string evidenceId)
        {
            if (!_state.TryGetValue(evidenceId, out var s)) return;
            if (s.IsCollected) return;
            s.IsCollected = true;
            OnCollected?.Invoke(evidenceId);
            TriggerAutosave();
            //NEW
            if (DatabaseManager.Instance != null)
            {
                var piece = GetPiece(evidenceId); // Ищем данные улики в конфигурации
                string displayName = (piece != null && !string.IsNullOrEmpty(piece.name)) ? piece.name : evidenceId;
                string surface = (s != null && !string.IsNullOrEmpty(s.SurfaceName)) ? s.SurfaceName : "Место ОМП";

                DatabaseManager.Instance.AddInventoryItem(evidenceId, displayName, 1, surface);
            }
        }

        public SaveData BuildSaveData(float protocolScore, List<string> penaltyIds, bool tutorialCompleted)
        {
            var data = new SaveData
            {
                caseId = _activeCase != null ? _activeCase.caseId : null,
                protocolScore = protocolScore,
                penaltyIds = penaltyIds != null ? new List<string>(penaltyIds) : new List<string>(),
                tutorialCompleted = tutorialCompleted
            };

            foreach (var kvp in _state)
            {
                data.evidence.Add(new EvidenceSaveEntry
                {
                    id = kvp.Key,
                    discovered = kvp.Value.IsDiscovered,
                    collected = kvp.Value.IsCollected,
                    photoTimestampUtc = kvp.Value.PhotoTimestampUtc,
                    surfaceName = kvp.Value.SurfaceName
                });
            }

            return data;
        }

        public void ApplySaveData(SaveData data)
        {
            if (data == null) return;
            foreach (var entry in data.evidence)
            {
                if (!_state.TryGetValue(entry.id, out var s)) continue;
                s.IsDiscovered = entry.discovered;
                s.IsCollected = entry.collected;
                s.PhotoTimestampUtc = entry.photoTimestampUtc;
                s.SurfaceName = entry.surfaceName;
            }
        }
    }
}
