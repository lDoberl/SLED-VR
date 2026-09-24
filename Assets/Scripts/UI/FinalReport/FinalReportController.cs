using System;
using System.Collections.Generic;
using CSI.Data;
using CSI.Runtime;
using CSI.UI.PauseMenu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CSI.UI.FinalReport
{
    public class FinalReportController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform motiveOptionsContainer;
        [SerializeField] private Transform causeOptionsContainer;
        [SerializeField] private Transform suspectOptionsContainer;
        [SerializeField] private FinalReportOptionView optionPrefab;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI resultText;

        [Header("Positioning (for opening from the inventory, i.e. from anywhere)")]
        [SerializeField] private WorldSpaceUIFollow worldSpaceUIFollow;
        [SerializeField] private Transform headTransform;

        public event Action OnOpened;
        public event Action<bool> OnSubmitted;

        private CaseDefinition _activeCase;
        private readonly List<FinalReportOptionView> _motiveViews = new List<FinalReportOptionView>();
        private readonly List<FinalReportOptionView> _causeViews = new List<FinalReportOptionView>();
        private readonly List<FinalReportOptionView> _suspectViews = new List<FinalReportOptionView>();

        private AnswerOption _selectedMotive;
        private AnswerOption _selectedCause;
        private AnswerOption _selectedSuspect;

        private void Start()
        {
            _activeCase = ActiveCaseContext.Instance != null ? ActiveCaseContext.Instance.CurrentCase : null;

            if (submitButton != null)
                submitButton.onClick.AddListener(Submit);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        public void Open()
        {
            if (_activeCase == null || optionPrefab == null) return;

            _selectedMotive = null;
            _selectedCause = null;
            _selectedSuspect = null;

            BuildOptions(_activeCase.motiveOptions, motiveOptionsContainer, _motiveViews, o => _selectedMotive = o);
            BuildOptions(_activeCase.causeOfDeathOptions, causeOptionsContainer, _causeViews, o => _selectedCause = o);
            BuildOptions(_activeCase.suspectOptions, suspectOptionsContainer, _suspectViews, o => _selectedSuspect = o);

            if (resultText != null)
                resultText.text = string.Empty;

            if (worldSpaceUIFollow != null && headTransform != null)
                worldSpaceUIFollow.PositionInFrontOf(headTransform);

            if (panelRoot != null)
                panelRoot.SetActive(true);

            OnOpened?.Invoke();
        }

        public void Close()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void BuildOptions(List<AnswerOption> options, Transform container, List<FinalReportOptionView> views, Action<AnswerOption> onSelect)
        {
            foreach (var v in views)
                Destroy(v.gameObject);
            views.Clear();

            if (container == null) return;

            foreach (var option in options)
            {
                var view = Instantiate(optionPrefab, container);
                // optionPrefab is a hidden template (inactive so it doesn't render on its own);
                // Instantiate copies that inactive state onto every clone, so each one needs to be
                // explicitly turned back on.
                view.gameObject.SetActive(true);
                view.SetData(option, onSelect);
                views.Add(view);
            }
        }

        private void Submit()
        {
            if (_activeCase == null) return;

            if (_selectedMotive == null || _selectedCause == null || _selectedSuspect == null)
            {
                if (resultText != null)
                    resultText.text = "Выберите все три пункта, прежде чем отправлять отчёт.";
                return;
            }

            bool allCorrect = _selectedMotive.isCorrect && _selectedCause.isCorrect && _selectedSuspect.isCorrect;
            float protocolScore = ProtocolScoringSystem.Instance != null ? ProtocolScoringSystem.Instance.CurrentScore : 100f;

            if (resultText != null)
            {
                resultText.text = (allCorrect ? "Дело раскрыто верно!" : "Обвинение неверно.") +
                                   $"\nБалл за соблюдение протокола ОМП: {protocolScore:0}/100";
            }

            if (EvidenceManager.Instance != null)
                EvidenceManager.Instance.TriggerAutosave();

            OnSubmitted?.Invoke(allCorrect);
        }
    }
}
