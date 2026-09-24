using System.Collections.Generic;
using UnityEngine;

namespace CSI.Tutorial
{
    [CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "CSI/Tutorial Sequence")]
    public class TutorialSequence : ScriptableObject
    {
        public List<TutorialStep> steps = new List<TutorialStep>();
    }
}
