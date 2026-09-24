using UnityEngine;
using UnityEngine.InputSystem;

namespace XR_Input
{
    public class HandsInteraction : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        [SerializeField] private InputActionProperty gripAction;
        
        [SerializeField] private InputActionProperty activateAction;

        private void Update()
        {
            var gripValue = gripAction.action.ReadValue<float>(); 
            var activateValue = activateAction.action.ReadValue<float>();

            animator.SetFloat("Grip", gripValue);
            animator.SetFloat("Trigger", activateValue);
        }
    }
}
