using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using Systems.Omp;
using UI.Inventory;
using UnityEngine.UI;
using System.IO;
using Button = UnityEngine.UI.Button;

namespace UI.Inventory.Additional_UI
{
    public class FinalReport : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown suspects;
        [SerializeField] private TMP_Dropdown reasonsOfDeath;
        [SerializeField] private TMP_Dropdown motives;
        [SerializeField] private Button signTheReportButton;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private TMP_Text penaltySummaryText;
        [SerializeField] private TMP_Text penaltyDetailsText;
        [SerializeField] private Image suspectsBackground;
        [SerializeField] private Image reasonsOfDeathBackground;
        [SerializeField] private Image motivesBackground;
        [SerializeField] private Color correctColor = new Color(0f, 1f, 0f, 0.25f);
        [SerializeField] private Color incorrectColor = new Color(1f, 0f, 0f, 0.25f);
        [SerializeField] private float initialScore = 100f;

        private bool _isSubmitted;
        private InventoryUIItem _currentUiItem;

        private void OnEnable()
        {
            if (signTheReportButton)
            {
                signTheReportButton.onClick.AddListener(OnSignReportClicked);
            }
        }

        private void OnDisable()
        {
            if (signTheReportButton)
            {
                signTheReportButton.onClick.RemoveListener(OnSignReportClicked);
            }
        }
        
        public void PopulateFromUIItem(InventoryUIItem uiItem)
        {
            if (!uiItem)
                return;

            _currentUiItem = uiItem;
            
            if (feedbackText)
            {
                feedbackText.text = string.Empty;
            }

            SetDropdownOptions(suspects, uiItem.selectionOfSuspects);
            SetDropdownOptions(reasonsOfDeath, uiItem.selectionOfDeaths);
            SetDropdownOptions(motives, uiItem.selectionOfMotives);

            RefreshPenaltySection(false);
        }

        private void SetDropdownOptions(TMP_Dropdown dropdown, List<ResponseOptions> options)
        {
            if (!dropdown || options == null)
                return;

            dropdown.ClearOptions();

            var optionDataList = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < options.Count; i++)
            {
                var responseOption = options[i];
                optionDataList.Add(new TMP_Dropdown.OptionData(responseOption != null ? responseOption.ResponseName : string.Empty));
            }

            dropdown.AddOptions(optionDataList);
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private void OnSignReportClicked()
        {
            if (_isSubmitted || !_currentUiItem)
                return;

            OmpActionAnalysisResult analysisResult = RefreshPenaltySection(true);

            bool suspectCorrect = IsSelectionCorrect(suspects, _currentUiItem.selectionOfSuspects);
            bool deathCorrect = IsSelectionCorrect(reasonsOfDeath, _currentUiItem.selectionOfDeaths);
            bool motiveCorrect = IsSelectionCorrect(motives, _currentUiItem.selectionOfMotives);

            bool allCorrect = suspectCorrect && deathCorrect && motiveCorrect;

            int wrongAnswersCount = 0;
            if (!suspectCorrect) wrongAnswersCount++;
            if (!deathCorrect) wrongAnswersCount++;
            if (!motiveCorrect) wrongAnswersCount++;

            string selectedSuspect = GetSelectedOptionName(suspects, _currentUiItem.selectionOfSuspects);
            string selectedDeath = GetSelectedOptionName(reasonsOfDeath, _currentUiItem.selectionOfDeaths);
            string selectedMotive = GetSelectedOptionName(motives, _currentUiItem.selectionOfMotives);

            string correctSuspect = GetCorrectOptionNames(_currentUiItem.selectionOfSuspects);
            string correctDeath = GetCorrectOptionNames(_currentUiItem.selectionOfDeaths);
            string correctMotive = GetCorrectOptionNames(_currentUiItem.selectionOfMotives);

            float systemPenalty = analysisResult != null ? analysisResult.TotalPenalty : 0f;
            float totalPenalty = systemPenalty + wrongAnswersCount;
            float finalScore = initialScore - totalPenalty;

            ApplyHighlight(suspects, suspectsBackground, suspectCorrect);
            ApplyHighlight(reasonsOfDeath, reasonsOfDeathBackground, deathCorrect);
            ApplyHighlight(motives, motivesBackground, motiveCorrect);

            WriteFinalReportToFile(
                analysisResult,
                suspectCorrect,
                deathCorrect,
                motiveCorrect,
                wrongAnswersCount,
                systemPenalty,
                totalPenalty,
                finalScore,
                selectedSuspect,
                selectedDeath,
                selectedMotive,
                correctSuspect,
                correctDeath,
                correctMotive);

            if (feedbackText)
            {
                feedbackText.text = "Итоговый отчёт сформирован в папке Отчёты.";
            }

            if (penaltySummaryText) penaltySummaryText.text = string.Empty;
            if (penaltyDetailsText) penaltyDetailsText.text = string.Empty;

            _isSubmitted = true;
            SetInteractable(false);
        }

        private OmpActionAnalysisResult RefreshPenaltySection(bool finalizeBeforeRefresh)
        {
            OmpActionAnalyzer analyzer = OmpActionAnalyzer.Instance;
            if (!analyzer)
            {
                return null;
            }

            if (finalizeBeforeRefresh)
            {
                analyzer.FinalizeAnalysis();
            }

            OmpActionAnalysisResult result = analyzer.BuildResult();

            return result;
        }

        private bool IsSelectionCorrect(TMP_Dropdown dropdown, List<ResponseOptions> options)
        {
            if (!dropdown || options == null || options.Count == 0)
                return false;

            int index = dropdown.value;
            if (index < 0 || index >= options.Count)
                return false;

            ResponseOptions selected = options[index];
            return selected != null && selected.IsCorrect;
        }

        private string GetSelectedOptionName(TMP_Dropdown dropdown, List<ResponseOptions> options)
        {
            if (!dropdown || options == null || options.Count == 0)
                return "-";

            int index = dropdown.value;
            if (index < 0 || index >= options.Count)
                return "-";

            ResponseOptions selected = options[index];
            if (selected == null || string.IsNullOrEmpty(selected.ResponseName))
                return "-";

            return selected.ResponseName;
        }

        private string GetCorrectOptionNames(List<ResponseOptions> options)
        {
            if (options == null || options.Count == 0)
                return "-";

            List<string> names = new List<string>();
            foreach (var option in options)
            {
                if (option != null && option.IsCorrect && !string.IsNullOrEmpty(option.ResponseName))
                {
                    names.Add(option.ResponseName);
                }
            }

            if (names.Count == 0)
                return "-";

            return string.Join(", ", names);
        }

        private void SetInteractable(bool interactable)
        {
            if (suspects) suspects.interactable = interactable;
            if (reasonsOfDeath) reasonsOfDeath.interactable = interactable;
            if (motives) motives.interactable = interactable;
            if (signTheReportButton) signTheReportButton.interactable = interactable;
        }

        private void ApplyHighlight(TMP_Dropdown dropdown, Image background, bool isCorrect)
        {
            if (background)
            {
                if (isCorrect) background.color = correctColor;
                else background.color = incorrectColor;
            }
            if (dropdown && dropdown.captionText)
            {
                if (isCorrect) dropdown.captionText.color = new Color(0f, 0.5f, 0f);
                else dropdown.captionText.color = new Color(0.6f, 0f, 0f);
            }
        }

        private void WriteFinalReportToFile(
            OmpActionAnalysisResult analysisResult,
            bool suspectCorrect,
            bool deathCorrect,
            bool motiveCorrect,
            int wrongAnswersCount,
            float systemPenalty,
            float totalPenalty,
            float finalScore,
            string selectedSuspect,
            string selectedDeath,
            string selectedMotive,
            string correctSuspect,
            string correctDeath,
            string correctMotive)
        {
            try
            {
                string gameRootPath = Application.dataPath;
                DirectoryInfo dataDir = new DirectoryInfo(gameRootPath);
                string rootFolder = dataDir.Parent != null ? dataDir.Parent.FullName : gameRootPath;

                string reportsFolder = Path.Combine(rootFolder, "Отчёты");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string fileName = $"Отчёт_{timestamp}.txt";
                string filePath = Path.Combine(reportsFolder, fileName);

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("ИТОГОВЫЙ ОТЧЁТ");
                builder.AppendLine($"Дата и время: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                builder.AppendLine();

                builder.AppendLine("РЕЗУЛЬТАТЫ ВЫБОРА:");
                builder.AppendLine($"Подозреваемый: {(suspectCorrect ? "верно" : "ошибка")}");
                builder.AppendLine($"Причина смерти: {(deathCorrect ? "верно" : "ошибка")}");
                builder.AppendLine($"Мотив: {(motiveCorrect ? "верно" : "ошибка")}");

                builder.AppendLine();
                builder.AppendLine("ВЫБОР СТУДЕНТА И ПРАВИЛЬНЫЕ ОТВЕТЫ:");
                builder.AppendLine($"Подозреваемый: выбран \"{selectedSuspect}\", правильный \"{correctSuspect}\"");
                builder.AppendLine($"Причина смерти: выбрано \"{selectedDeath}\", правильный вариант \"{correctDeath}\"");
                builder.AppendLine($"Мотив: выбран \"{selectedMotive}\", правильный \"{correctMotive}\"");

                if (!suspectCorrect || !deathCorrect || !motiveCorrect)
                {
                    builder.AppendLine();
                    builder.AppendLine("Ошибки в выборе:");
                    if (!suspectCorrect) builder.AppendLine("- Подозреваемый выбран неверно.");
                    if (!deathCorrect) builder.AppendLine("- Причина смерти выбрана неверно.");
                    if (!motiveCorrect) builder.AppendLine("- Мотив выбран неверно.");
                }

                builder.AppendLine();
                builder.AppendLine("ШТРАФЫ СИСТЕМЫ АНАЛИЗА:");

                if (analysisResult == null)
                {
                    builder.AppendLine("Система анализа не активирована.");
                }
                else
                {
                    builder.AppendLine($"Суммарный штраф (система): {systemPenalty:0.##}");

                    if (analysisResult.Penalties.Count == 0)
                    {
                        builder.AppendLine("Нарушений не зафиксировано.");
                    }
                    else
                    {
                        foreach (OmpPenaltyEntry penalty in analysisResult.Penalties)
                        {
                            builder.Append("- ");
                            builder.Append(penalty.Reason);
                            builder.Append(" (штраф: -");
                            builder.Append(penalty.Points.ToString("0.##"));
                            builder.AppendLine(")");
                        }
                    }
                }

                builder.AppendLine();
                builder.AppendLine("ДОПОЛНИТЕЛЬНЫЕ ШТРАФЫ:");
                builder.AppendLine($"Количество неверных ответов: {wrongAnswersCount} (по -1 баллу за каждый неверный вариант)");

                builder.AppendLine();
                builder.AppendLine("ИТОГОВЫЙ БАЛЛ:");
                builder.AppendLine($"Начальный балл: {initialScore:0.##}");
                builder.AppendLine($"Общий штраф: {totalPenalty:0.##}");
                builder.AppendLine($"Итоговый балл: {finalScore:0.##}");

                File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError($"Не удалось записать итоговый отчёт: {e}");
            }
        }
    }
}
