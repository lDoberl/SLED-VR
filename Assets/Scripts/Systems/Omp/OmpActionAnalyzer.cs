using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Systems.Omp
{
    public class OmpActionAnalyzer : MonoBehaviour
    {
        public static OmpActionAnalyzer Instance { get; private set; }

        [SerializeField] private OmpActionSequenceConfig sequenceConfig;
        [SerializeField] private bool initializeOnAwake = true;

        public event Action<OmpPenaltyEntry> PenaltyIssued;

        private readonly List<OmpPenaltyEntry> penalties = new List<OmpPenaltyEntry>();
        private readonly List<OmpActionLogEntry> actionLog = new List<OmpActionLogEntry>();
        private readonly Dictionary<string, int> stepIndexLookup = new Dictionary<string, int>();
        private readonly HashSet<string> completedSteps = new HashSet<string>();
        private readonly HashSet<string> skippedSteps = new HashSet<string>();

        private int _nextStepIndex;
        private bool _isFinalized;

        public bool IsReady => sequenceConfig && stepIndexLookup.Count > 0;
        public IReadOnlyList<OmpPenaltyEntry> Penalties => penalties;
        public IReadOnlyList<OmpActionLogEntry> ActionLog => actionLog;
        public float TotalPenalty => penalties.Sum(p => p.Points);

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (initializeOnAwake)
            {
                InitializeSession();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void InitializeSession()
        {
            penalties.Clear();
            actionLog.Clear();
            stepIndexLookup.Clear();
            completedSteps.Clear();
            skippedSteps.Clear();
            _nextStepIndex = 0;
            _isFinalized = false;

            if (!sequenceConfig)
            {
                Debug.LogWarning("OmpActionAnalyzer: Sequence config is not assigned.");
                return;
            }

            IReadOnlyList<OmpActionStep> steps = sequenceConfig.Steps;
            for (int i = 0; i < steps.Count; i++)
            {
                OmpActionStep step = steps[i];
                if (string.IsNullOrEmpty(step.ActionId))
                {
                    Debug.LogWarningFormat(this, "Sequence step at index {0} has empty ActionId.", i);
                    continue;
                }

                if (stepIndexLookup.ContainsKey(step.ActionId))
                {
                    Debug.LogWarningFormat(this, "Duplicate ActionId '{0}' detected in sequence config.", step.ActionId);
                    continue;
                }

                stepIndexLookup.Add(step.ActionId, i);
            }

            if (DatabaseManager.Instance != null)
            {
        DatabaseManager.Instance.StartNewSession("Следователь VR");
            }
        }

        public void RecordAction(string actionId)
        {
            RecordAction(actionId, default);
        }

        public void RecordAction(string actionId, OmpActionContext context)
        {
            if (string.IsNullOrEmpty(actionId))
                return;

            if (_isFinalized)
            {
                Debug.LogWarning("OmpActionAnalyzer: Attempted to record an action after the session has been finalized.");
                return;
            }

            actionLog.Add(new OmpActionLogEntry(actionId, DateTime.UtcNow, context));

            if (!sequenceConfig || !stepIndexLookup.TryGetValue(actionId, out int index))
            {
                IssuePenalty(new OmpPenaltyEntry
                {
                    PenaltyId = $"unknown-{actionId}",
                    RelatedActionId = actionId,
                    Points = 1f,
                    Reason = $"Действие '{actionId}' не описано в конфигурации ОМП.",
                    TimestampUtc = DateTime.UtcNow,
                    Type = OmpPenaltyType.UnknownAction
                });
                return;
            }

            OmpActionStep step = sequenceConfig.Steps[index];

            if (!step.IsToolAllowed(context.ToolId))
            {
                IssuePenalty(new OmpPenaltyEntry
                {
                    PenaltyId = $"tool-{actionId}-{DateTime.UtcNow.Ticks}",
                    RelatedActionId = actionId,
                    Points = Mathf.Max(step.SequencePenalty, 1f),
                    Reason = $"Неверный инструмент ({context.ToolId}) для шага '{GetDisplayName(step)}'.",
                    Context = context.ToolId,
                    TimestampUtc = DateTime.UtcNow,
                    Type = OmpPenaltyType.ToolViolation
                });
            }

            if (completedSteps.Contains(actionId))
            {
                IssuePenalty(new OmpPenaltyEntry
                {
                    PenaltyId = $"repeat-{actionId}-{DateTime.UtcNow.Ticks}",
                    RelatedActionId = actionId,
                    Points = Mathf.Max(step.RepeatPenalty, 0.5f),
                    Reason = $"Шаг '{GetDisplayName(step)}' повторён без необходимости.",
                    TimestampUtc = DateTime.UtcNow,
                    Type = OmpPenaltyType.Repeat
                });
                return;
            }

            if (index > _nextStepIndex)
            {
                for (int i = _nextStepIndex; i < index; i++)
                {
                    OmpActionStep skipped = sequenceConfig.Steps[i];
                    if (skippedSteps.Contains(skipped.ActionId))
                        continue;

                    FlagSkippedStep(skipped, $"Пропущен перед выполнением '{GetDisplayName(step)}'.");
                }

                IssuePenalty(new OmpPenaltyEntry
                {
                    PenaltyId = $"sequence-{actionId}-{DateTime.UtcNow.Ticks}",
                    RelatedActionId = actionId,
                    Points = Mathf.Max(step.SequencePenalty, 1f),
                    Reason = $"Шаг '{GetDisplayName(step)}' выполнен вне установленной последовательности.",
                    TimestampUtc = DateTime.UtcNow,
                    Type = OmpPenaltyType.WrongOrder
                });
            }
            else if (index < _nextStepIndex && !completedSteps.Contains(actionId))
            {
            }

            completedSteps.Add(actionId);
            _nextStepIndex = Mathf.Max(_nextStepIndex, index + 1);
        }

        public void RegisterCustomError(string penaltyId, string description, float points, string relatedActionId = null, string context = null)
        {
            IssuePenalty(new OmpPenaltyEntry
            {
                PenaltyId = penaltyId,
                RelatedActionId = relatedActionId,
                Points = Mathf.Max(points, 0f),
                Reason = description,
                Context = context,
                TimestampUtc = DateTime.UtcNow,
                Type = OmpPenaltyType.Custom
            });
        }

        public void FinalizeAnalysis()
        {
            if (_isFinalized || !sequenceConfig)
                return;

            foreach (OmpActionStep step in sequenceConfig.Steps)
            {
                if (!completedSteps.Contains(step.ActionId) && !skippedSteps.Contains(step.ActionId))
                {
                    FlagSkippedStep(step, "Шаг не выполнен к моменту подписания отчёта.");
                }
            }
            //NEW
            if (DatabaseManager.Instance != null)
            {
                DatabaseManager.Instance.CompleteSession();
            }

            _isFinalized = true;
        }

        public OmpActionAnalysisResult BuildResult()
        {
            return new OmpActionAnalysisResult(
                TotalPenalty,
                penalties.AsReadOnly(),
                actionLog.AsReadOnly(),
                completedSteps.ToList().AsReadOnly());
        }

        private void FlagSkippedStep(OmpActionStep step, string reasonSuffix)
        {
            skippedSteps.Add(step.ActionId);

            IssuePenalty(new OmpPenaltyEntry
            {
                PenaltyId = $"skip-{step.ActionId}-{DateTime.UtcNow.Ticks}",
                RelatedActionId = step.ActionId,
                Points = Mathf.Max(step.SkipPenalty, 1f),
                Reason = $"Шаг '{GetDisplayName(step)}' пропущен. {reasonSuffix}",
                TimestampUtc = DateTime.UtcNow,
                Type = OmpPenaltyType.Skip
            });
        }

        private void IssuePenalty(OmpPenaltyEntry entry)
        {
            penalties.Add(entry);
            PenaltyIssued?.Invoke(entry);
        }

        private static string GetDisplayName(OmpActionStep step)
        {
            return string.IsNullOrEmpty(step.DisplayName) ? step.ActionId : step.DisplayName;
        }
    }
}

