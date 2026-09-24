using UnityEngine;

namespace Systems.Omp
{
    public struct OmpActionContext
    {
        public string ToolId;
        public Vector3 WorldPosition;
        public string ExtraInfo;

        public OmpActionContext(string toolId, Vector3 worldPosition, string extraInfo)
        {
            ToolId = toolId;
            WorldPosition = worldPosition;
            ExtraInfo = extraInfo;
        }

        public static OmpActionContext FromTool(string toolId)
        {
            return new OmpActionContext(toolId, Vector3.zero, string.Empty);
        }
    }
}

