using System.Collections.Generic;
using UnityEngine;

namespace Systems.Omp
{
    [CreateAssetMenu(menuName = "OMP/Action Sequence Config", fileName = "OmpActionSequence")]
    public class OmpActionSequenceConfig : ScriptableObject
    {
        [SerializeField] private List<OmpActionStep> steps = new List<OmpActionStep>();

        public IReadOnlyList<OmpActionStep> Steps => steps;
    }
}

