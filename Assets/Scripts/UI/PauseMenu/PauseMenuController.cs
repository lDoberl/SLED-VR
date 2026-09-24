using CSI.Core;
using CSI.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CSI.UI.PauseMenu
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private WorldSpaceUIFollow worldSpaceUIFollow;
        [SerializeField] private Transform headTransform;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartCaseButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private bool _isPaused;

        // Built directly in code rather than wired to an external .inputactions asset —
        // keeps the toggle self-contained and guaranteed-enabled regardless of rig setup.
        private InputAction _pauseAction;

        private void Start()
        {
            _pauseAction = new InputAction("TogglePause", InputActionType.Button);
            _pauseAction.AddBinding("<XRController>{LeftHand}/{PrimaryButton}");
            _pauseAction.AddBinding("<XRController>{RightHand}/{PrimaryButton}");
            _pauseAction.Enable();

            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartCaseButton != null) restartCaseButton.onClick.AddListener(RestartCase);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (quitButton != null) quitButton.onClick.AddListener(() => Application.Quit());

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            _pauseAction?.Disable();
            _pauseAction?.Dispose();
        }

        private void Update()
        {
            if (_pauseAction != null && _pauseAction.triggered)
                Toggle();
        }

        private void Toggle()
        {
            if (_isPaused) Resume();
            else Pause();
        }

        private void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;

            if (worldSpaceUIFollow != null && headTransform != null)
                worldSpaceUIFollow.PositionInFrontOf(headTransform);

            if (panelRoot != null)
                panelRoot.SetActive(true);
        }

        private void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void RestartCase()
        {
            Time.timeScale = 1f;
            var current = ActiveCaseContext.Instance != null ? ActiveCaseContext.Instance.CurrentCase : null;
            if (current == null) return;

            if (SaveSystem.Instance != null)
                SaveSystem.Instance.DeleteSave();

            ActiveCaseContext.Instance.SetCase(current, true);
            SceneManager.LoadScene(current.sceneName);
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
