using CSI.Core;
using CSI.Data;
using CSI.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CSI.UI.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Case")]
        [SerializeField] private CaseDefinition defaultCase;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button trainingButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button settingsButton;

        [Header("New Game confirmation (shown only when a save already exists)")]
        [SerializeField] private GameObject confirmNewGamePanel;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private void Start()
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (trainingButton != null)
                trainingButton.onClick.AddListener(OnTrainingClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (confirmYesButton != null)
                confirmYesButton.onClick.AddListener(StartNewGame);

            if (confirmNoButton != null)
                confirmNoButton.onClick.AddListener(() => SetConfirmPanel(false));

            SetConfirmPanel(false);
        }

        private void OnNewGameClicked()
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile();
            if (hasSave)
                SetConfirmPanel(true);
            else
                StartNewGame();
        }

        private void StartNewGame()
        {
            SetConfirmPanel(false);
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.DeleteSave();

            if (ActiveCaseContext.Instance == null || defaultCase == null) return;
            ActiveCaseContext.Instance.SetCase(defaultCase, true);
            SceneManager.LoadScene(defaultCase.sceneName);
        }

        private void OnContinueClicked()
        {
            if (ActiveCaseContext.Instance == null || defaultCase == null) return;
            ActiveCaseContext.Instance.SetCase(defaultCase, false);
            SceneManager.LoadScene(defaultCase.sceneName);
        }

        private void OnTrainingClicked()
        {
            SceneManager.LoadScene(SceneNames.Training);
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private void SetConfirmPanel(bool show)
        {
            if (confirmNewGamePanel != null)
                confirmNewGamePanel.SetActive(show);

            // Hide the regular menu buttons while the confirmation is up so they can't be
            // clicked through the dialog (world-space canvas buttons don't reliably occlude
            // each other by sibling order the way a screen-space overlay would).
            if (newGameButton != null) newGameButton.gameObject.SetActive(!show);
            if (continueButton != null) continueButton.gameObject.SetActive(!show);
            if (trainingButton != null) trainingButton.gameObject.SetActive(!show);
            if (quitButton != null) quitButton.gameObject.SetActive(!show);
            if (settingsButton != null) settingsButton.gameObject.SetActive(!show);
        }
    }
}
