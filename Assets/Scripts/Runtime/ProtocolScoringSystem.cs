using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSI.Runtime
{
    /// <summary>
    /// Flat penalty list — deduped by rule+evidenceId so a reload can never double-penalize
    /// the same violation. Subscribes to EvidenceManager.OnCollected and checks whether the
    /// piece was photographed first; that's the one hard protocol rule that matters here.
    /// </summary>
    public class ProtocolScoringSystem : MonoBehaviour
    {
        public static ProtocolScoringSystem Instance { get; private set; }

        private const float CollectedBeforePhotoPenalty = 5f;
        private const float MovedBeforePhotoPenalty = 1f;

        public event Action<PenaltyEntry> OnPenaltyApplied;

        private readonly List<PenaltyEntry> _penalties = new List<PenaltyEntry>();
        private readonly HashSet<string> _appliedKeys = new HashSet<string>();

        public IReadOnlyList<PenaltyEntry> Penalties => _penalties;
        public float CurrentScore => Mathf.Max(0f, 100f - _penalties.Sum(p => p.Points));

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.OnCollected += HandleCollected;

            RestoreFromSave();
        }

        private void OnDestroy()
        {
            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.OnCollected -= HandleCollected;
        }

        private void HandleCollected(string evidenceId)
        {
            var mgr = EvidenceManager.Instance;
            var piece = mgr.GetPiece(evidenceId);
            if (piece == null || !piece.requiresPhotoBeforeCollection) return;

            if (!mgr.IsPhotographed(evidenceId))
                RegisterViolation(ProtocolRule.CollectedBeforePhoto, evidenceId, CollectedBeforePhotoPenalty);
        }

        public void RegisterViolation(ProtocolRule rule, string evidenceId, float points)
        {
            string key = BuildKey(rule, evidenceId);
            if (_appliedKeys.Contains(key)) return;

            _appliedKeys.Add(key);
            var entry = new PenaltyEntry { Rule = rule, EvidenceId = evidenceId, Points = points };
            _penalties.Add(entry);
            OnPenaltyApplied?.Invoke(entry);
            Debug.Log($"[ProtocolScoringSystem] Violation: {rule} on '{evidenceId}' (-{points} pts). Score now {CurrentScore}.");
            //NEW
            if (DatabaseManager.Instance != null)
            {
                string reason = $"Нарушение протокола ОМП: {rule} для объекта '{evidenceId}'";
                DatabaseManager.Instance.RecordOmpPenalty(reason, (int)points);
            }
        }

        public List<string> BuildPenaltyIdList()
        {
            return _appliedKeys.ToList();
        }

        private void RestoreFromSave()
        {
            if (SaveSystem.Instance == null || EvidenceManager.Instance == null) return;

            var save = SaveSystem.Instance.Load();
            if (save == null || save.caseId != EvidenceManager.Instance.ActiveCase?.caseId) return;

            foreach (var key in save.penaltyIds)
            {
                if (_appliedKeys.Contains(key)) continue;
                var parts = key.Split(':');
                if (parts.Length != 2 || !Enum.TryParse<ProtocolRule>(parts[0], out var rule)) continue;

                _appliedKeys.Add(key);
                float points = rule == ProtocolRule.CollectedBeforePhoto ? CollectedBeforePhotoPenalty : MovedBeforePhotoPenalty;
                _penalties.Add(new PenaltyEntry { Rule = rule, EvidenceId = parts[1], Points = points });
            }
        }

        private static string BuildKey(ProtocolRule rule, string evidenceId) => $"{rule}:{evidenceId}";
    }
}
