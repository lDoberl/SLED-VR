using CSI.Data;
using CSI.Interaction;
using CSI.Runtime;
using CSI.UI.FinalReport;
using CSI.UI.Inventory;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CSI.Tutorial
{
    /// <summary>
    /// Drives TutorialSequence in TrainingScene. Each step completes via the *same* events the
    /// real case uses (EvidenceManager, InventoryController, EvidenceEnvelope, NotebookProp,
    /// FinalReportController) — so the tutorial doubles as a live regression check of the real
    /// mechanics rather than a parallel mock. Bootstraps its own tiny practice "case" (EvidenceManager
    /// + ProtocolScoringSystem) so TrainingScene is self-sufficient and doesn't depend on whatever
    /// case, if any, was active before it was loaded.
    ///
    /// Guidance is spatial rather than a floating checklist: each step shows exactly one
    /// TutorialMarker — a highlight plus a floating instruction panel — at the place the player
    /// needs to go or the thing they need to use next. The marker for a finished step disappears
    /// and the next one appears, so there's never more than one "do this now" cue on screen.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private TutorialSequence sequence;
        [SerializeField] private CaseDefinition practiceCase;

        [Header("Practice scene references")]
        [SerializeField] private TutorialZoneRelay locomotionZone;
        [SerializeField] private XRGrabInteractable practiceGrabObject;
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private string practiceFingerprintId = "TutorialPrint";
        [SerializeField] private FinalReportController finalReportController;

        [Header("Step markers (highlight + floating instruction)")]
        [SerializeField] private TutorialMarker locomotionMarker;
        [SerializeField] private TutorialMarker grabMarker;
        [SerializeField] private TutorialMarker printMarker;
        [SerializeField] private TutorialMarker notebookMarker;
        [SerializeField] private TutorialMarker reportMarker;

        private int _currentIndex = -1;
        private System.Action _currentStepCleanup;
        private TutorialMarker _activeMarker;

        private void Start()
        {
            BootstrapPracticeCase();
            AdvanceToNextStep();
        }

        private void BootstrapPracticeCase()
        {
            if (practiceCase == null) return;

            if (ActiveCaseContext.Instance != null)
                ActiveCaseContext.Instance.SetCase(practiceCase, true);

            if (EvidenceManager.Instance == null)
                gameObject.AddComponent<EvidenceManager>();

            if (ProtocolScoringSystem.Instance == null)
                gameObject.AddComponent<ProtocolScoringSystem>();
        }

        private void OnDestroy()
        {
            _currentStepCleanup?.Invoke();
        }

        private void AdvanceToNextStep()
        {
            _currentStepCleanup?.Invoke();
            _currentStepCleanup = null;

            _activeMarker?.Hide();
            _activeMarker = null;

            _currentIndex++;

            if (sequence == null || _currentIndex >= sequence.steps.Count)
            {
                CompleteTutorial();
                return;
            }

            var step = sequence.steps[_currentIndex];
            _activeMarker = GetMarkerForStep(step.stepId);
            _activeMarker?.Show(step.instructionText, $"Шаг {_currentIndex + 1} из {sequence.steps.Count}");

            BeginStep(step);
        }

        private void CompleteTutorial()
        {
            reportMarker?.Show("Обучение пройдено! Можно приступать к настоящему делу.");

            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.MarkTutorialCompleted();
        }

        private TutorialMarker GetMarkerForStep(string stepId)
        {
            switch (stepId)
            {
                case "Locomotion": return locomotionMarker;
                case "Grab": return grabMarker;
                case "RevealPrint":
                case "Photograph":
                case "TapeLift":
                case "Envelope":
                    return printMarker;
                case "Notebook": return notebookMarker;
                case "FinalReportOpen":
                case "AccusationSubmit":
                    return reportMarker;
                default: return null;
            }
        }

        private void BeginStep(TutorialStep step)
        {
            switch (step.stepId)
            {
                case "Locomotion":
                    if (locomotionZone != null)
                    {
                        void OnEntered() => AdvanceToNextStep();
                        locomotionZone.OnPlayerEntered += OnEntered;
                        _currentStepCleanup = () => locomotionZone.OnPlayerEntered -= OnEntered;
                    }
                    break;

                case "Grab":
                    if (practiceGrabObject != null)
                    {
                        bool hasGrabbed = false;

                        void OnGrabbed(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args) => hasGrabbed = true;
                        void OnReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
                        {
                            if (hasGrabbed) AdvanceToNextStep();
                        }

                        practiceGrabObject.selectEntered.AddListener(OnGrabbed);
                        practiceGrabObject.selectExited.AddListener(OnReleased);
                        _currentStepCleanup = () =>
                        {
                            practiceGrabObject.selectEntered.RemoveListener(OnGrabbed);
                            practiceGrabObject.selectExited.RemoveListener(OnReleased);
                        };
                    }
                    break;

                case "RevealPrint":
                    // Completes once the print is dusted — which, mechanically, requires the player
                    // to have already opened the inventory and grabbed the brush, so this single
                    // step's instruction covers all three actions instead of gating them separately.
                    SubscribeEvidenceEvent(e => EvidenceManager.Instance.OnDiscovered += e, e => EvidenceManager.Instance.OnDiscovered -= e);
                    break;

                case "Photograph":
                    SubscribeEvidenceEvent(e => EvidenceManager.Instance.OnPhotographed += e, e => EvidenceManager.Instance.OnPhotographed -= e);
                    break;

                case "TapeLift":
                    SubscribeEvidenceEvent(e => EvidenceManager.Instance.OnCollected += e, e => EvidenceManager.Instance.OnCollected -= e);
                    break;

                case "Envelope":
                    void OnSealed(string id) => AdvanceToNextStep();
                    EvidenceEnvelope.OnSealed += OnSealed;
                    _currentStepCleanup = () => EvidenceEnvelope.OnSealed -= OnSealed;
                    break;

                case "Notebook":
                    void OnNotebookGrabbed() => AdvanceToNextStep();
                    NotebookProp.OnGrabbedByPlayer += OnNotebookGrabbed;
                    _currentStepCleanup = () => NotebookProp.OnGrabbedByPlayer -= OnNotebookGrabbed;
                    break;

                case "FinalReportOpen":
                    if (finalReportController != null)
                    {
                        void OnOpened() => AdvanceToNextStep();
                        finalReportController.OnOpened += OnOpened;
                        _currentStepCleanup = () => finalReportController.OnOpened -= OnOpened;
                    }
                    break;

                case "AccusationSubmit":
                    if (finalReportController != null)
                    {
                        void OnSubmitted(bool correct) => AdvanceToNextStep();
                        finalReportController.OnSubmitted += OnSubmitted;
                        _currentStepCleanup = () => finalReportController.OnSubmitted -= OnSubmitted;
                    }
                    break;

                default:
                    Debug.LogWarning($"[TutorialManager] Unknown step id '{step.stepId}' — skipping.");
                    AdvanceToNextStep();
                    break;
            }
        }

        private void SubscribeEvidenceEvent(System.Action<System.Action<string>> subscribe, System.Action<System.Action<string>> unsubscribe)
        {
            if (EvidenceManager.Instance == null) return;

            void Handler(string id)
            {
                if (id == practiceFingerprintId)
                    AdvanceToNextStep();
            }

            subscribe(Handler);
            _currentStepCleanup = () => unsubscribe(Handler);
        }
    }
}
