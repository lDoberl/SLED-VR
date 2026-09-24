using UnityEngine;

namespace CSI.UI.PauseMenu
{
    /// <summary>
    /// Repositions/refaces a world-space canvas relative to a reference transform (the player's
    /// head) computed fresh each time it's shown, rather than relying on a static baked local
    /// offset. This avoids the old "canvas sinks underground" bug, which was caused by a fixed
    /// offset baked under Camera Offset that only matched the Editor's Device-mode preview height,
    /// not the real headset's Floor-mode height.
    /// </summary>
    public class WorldSpaceUIFollow : MonoBehaviour
    {
        [SerializeField] private float distance = 1.0f;
        [SerializeField] private float heightOffset = 0f;

        public void PositionInFrontOf(Transform reference)
        {
            if (reference == null) return;

            Vector3 flatForward = reference.forward;
            flatForward.y = 0f;
            if (flatForward.sqrMagnitude < 0.001f)
                flatForward = Vector3.forward;
            flatForward.Normalize();

            transform.position = reference.position + flatForward * distance + Vector3.up * heightOffset;

            // A UI canvas reads correctly when its local forward (+Z) points AWAY from the
            // viewer (same convention as the default Camera-at-(0,0,-10)-looking-at-canvas-at-origin
            // setup) — so face the canvas the same direction it was approached from, not back at
            // the player, or the text renders mirrored.
            Vector3 faceAwayFromReference = transform.position - reference.position;
            faceAwayFromReference.y = 0f;
            if (faceAwayFromReference.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(faceAwayFromReference, Vector3.up);
        }
    }
}
