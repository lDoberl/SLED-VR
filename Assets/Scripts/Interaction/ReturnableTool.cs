using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace CSI.Interaction
{
    /// <summary>
    /// Marks a handheld tool (camera/brush/tape/envelope) as returnable to the inventory: releasing
    /// it while it's inside the ToolReturnZone (reaching behind your back) sends it back —
    /// incrementing its remaining stock and removing the physical prop — instead of leaving it
    /// lying around in the world. While held, a double haptic pulse fires the moment it crosses
    /// into the zone, since the zone itself is invisible and otherwise hard to feel out.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ReturnableTool : MonoBehaviour
    {
        [SerializeField] private string evidenceId;
        [SerializeField] private float hapticAmplitude = 0.6f;
        [SerializeField] private float hapticPulseDuration = 0.08f;
        [SerializeField] private float hapticGapDuration = 0.1f;

        /// <summary>Static so the inventory (which owns remaining-stock counts) can react without
        /// needing a direct reference to whichever tool instance was returned.</summary>
        public static event Action<string> OnToolReturned;

        private XRGrabInteractable _grabInteractable;
        private bool _wasInsideZone;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        /// <summary>Sets which EvidencePiece this spawned instance restocks on return. The same
        /// prefab is shared across cases with different ids (e.g. the real case's camera vs. the
        /// tutorial's practice camera), so this can't be baked into the prefab — the inventory sets
        /// it right after spawning the tool into the player's hand.</summary>
        public void Initialize(string toolEvidenceId)
        {
            evidenceId = toolEvidenceId;
        }

        private void OnEnable()
        {
            _grabInteractable.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            _grabInteractable.selectExited.RemoveListener(HandleSelectExited);
        }

        private void Update()
        {
            if (!_grabInteractable.isSelected)
            {
                _wasInsideZone = false;
                return;
            }

            bool isInside = ToolReturnZone.Instance != null && ToolReturnZone.Instance.Contains(transform.position);
            if (isInside && !_wasInsideZone)
                PulseHaptics();

            _wasInsideZone = isInside;
        }

        private void PulseHaptics()
        {
            var interactors = _grabInteractable.interactorsSelecting;
            if (interactors.Count == 0) return;

            if (interactors[0] is XRBaseInputInteractor inputInteractor)
                StartCoroutine(DoubleHapticPulse(inputInteractor));
        }

        private IEnumerator DoubleHapticPulse(XRBaseInputInteractor interactor)
        {
            interactor.SendHapticImpulse(hapticAmplitude, hapticPulseDuration);
            yield return new WaitForSeconds(hapticPulseDuration + hapticGapDuration);
            interactor.SendHapticImpulse(hapticAmplitude, hapticPulseDuration);
        }

        private void HandleSelectExited(SelectExitEventArgs args)
        {
            _wasInsideZone = false;

            if (ToolReturnZone.Instance == null || !ToolReturnZone.Instance.Contains(transform.position))
                return;

            OnToolReturned?.Invoke(evidenceId);
            Destroy(gameObject);
        }
    }
}
