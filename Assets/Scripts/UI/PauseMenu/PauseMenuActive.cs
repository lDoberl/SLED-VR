using Config;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.PauseMenu
{
    public class PauseMenuActive : MonoBehaviour
    {
        [SerializeField] private InputActionProperty pauseAction;

        [SerializeField] private GameObject pauseMenu;
        
        private void Update()
        {
            if (!pauseAction.action.triggered) return;
            
            Time.timeScale = GameManager.Instance.IsPaused ? 1 : 0;
                
            pauseMenu.SetActive(!GameManager.Instance.IsPaused);
                
            GameManager.Instance.IsPaused = !GameManager.Instance.IsPaused;
        }
    }
}
