using TMPro;
using UnityEngine;

namespace CSI.Tutorial
{
    /// <summary>
    /// A world-space beacon for one point of interest in the tutorial: a highlight VFX plus a
    /// floating instruction panel that billboards (yaw-only) toward the player. TutorialManager
    /// shows the marker relevant to the current step and hides it once that step completes, so the
    /// player always has one obvious "go here / do this" cue instead of a disconnected checklist.
    /// </summary>
    public class TutorialMarker : MonoBehaviour
    {
        [SerializeField] private GameObject highlightVisual;
        [SerializeField] private Canvas instructionCanvas;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI stepCounterText;
        [SerializeField] private Transform headCamera;

        private void Awake()
        {
            Hide();
        }

        public void Show(string text, string stepCounter = null)
        {
            if (instructionText != null)
                instructionText.text = text;
            if (stepCounterText != null)
                stepCounterText.text = stepCounter ?? string.Empty;

            if (highlightVisual != null)
                highlightVisual.SetActive(true);
            if (instructionCanvas != null)
                instructionCanvas.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (highlightVisual != null)
                highlightVisual.SetActive(false);
            if (instructionCanvas != null)
                instructionCanvas.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (headCamera == null || instructionCanvas == null || !instructionCanvas.gameObject.activeSelf)
                return;

            // Canvas forward must point away from the viewer for non-mirrored reading (same
            // convention as WorldSpaceUIFollow).
            Vector3 faceAwayFromViewer = instructionCanvas.transform.position - headCamera.position;
            faceAwayFromViewer.y = 0f;
            if (faceAwayFromViewer.sqrMagnitude > 0.001f)
                instructionCanvas.transform.rotation = Quaternion.LookRotation(faceAwayFromViewer.normalized, Vector3.up);
        }
    }
}
