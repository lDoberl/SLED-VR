using UnityEngine;

namespace CSI.Tutorial
{
    /// <summary>
    /// Placed on a trigger volume in the training scene; fires when the player (rig's
    /// CharacterController, tagged "Player") enters. Used by the LocomotionStep so it works
    /// identically whether the player walked, teleported, or turned into the zone.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TutorialZoneRelay : MonoBehaviour
    {
        public event System.Action OnPlayerEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                OnPlayerEntered?.Invoke();
        }
    }
}
