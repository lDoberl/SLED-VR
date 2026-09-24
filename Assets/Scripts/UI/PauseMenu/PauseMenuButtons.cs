using Config;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.PauseMenu
{
    public class PauseMenuButtons : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
    
        [SerializeField] private Button exitButton;

        [SerializeField] private GameObject pauseMenu;

        private void Start()
        {
            continueButton.onClick.AddListener(ContinueTraining);
            exitButton.onClick.AddListener(Exit);
        }

        private void ContinueTraining()
        {
            GameManager.Instance.IsPaused = false;
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }

        private void Exit()
        {
            GameManager.Instance.IsPaused = false;
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
            SceneManager.LoadScene("MainScene");
        }
    }
}
