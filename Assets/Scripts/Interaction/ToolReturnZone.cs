using UnityEngine;

namespace CSI.Interaction
{
    /// <summary>
    /// A holster zone that trails behind the player's head, yaw-only, so "behind your back" stays
    /// behind your back regardless of which way you're currently facing. A ReturnableTool released
    /// while inside this zone gets sent back to the inventory instead of being dropped in the world.
    /// </summary>
    public class ToolReturnZone : MonoBehaviour
    {
        [SerializeField] private Transform headCamera;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.25f, -0.3f);
        [SerializeField] private float radius = 0.25f;

        public static ToolReturnZone Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void LateUpdate()
        {
            if (headCamera == null) return;

            var yawOnly = Quaternion.Euler(0f, headCamera.eulerAngles.y, 0f);
            transform.position = headCamera.position + yawOnly * localOffset;
            transform.rotation = yawOnly;
        }

        public bool Contains(Vector3 worldPosition)
        {
            return (worldPosition - transform.position).sqrMagnitude <= radius * radius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.4f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
