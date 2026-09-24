using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// Implemented by any scene prop that CameraTool should be able to photograph
    /// (mark as photographed in EvidenceManager) when it's in the camera's frustum.
    /// </summary>
    public interface IPhotographable
    {
        string EvidenceId { get; }
        Bounds WorldBounds { get; }
    }
}
