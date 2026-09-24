using UnityEngine;

namespace CSI.Tutorial
{
    /// <summary>
    /// One step of the tutorial sequence. `stepId` is a switch key TutorialManager uses to wire
    /// the one real-mechanic event that completes this step (see TutorialManager.BeginStep) —
    /// keeping the *order* and *text* data-driven while the finite set of step kinds stays in code.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "CSI/Tutorial Step")]
    public class TutorialStep : ScriptableObject
    {
        public string stepId;
        [TextArea(2, 5)] public string instructionText;
    }
}
