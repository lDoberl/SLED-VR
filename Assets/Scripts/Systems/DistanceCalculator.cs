using UnityEngine;

namespace Systems
{
    [System.Serializable]
    public class DistanceCalculator
    {
        public float CalculateShortestDistance(GameObject obj1, GameObject obj2)
        {
            Collider[] colliders1 = obj1.GetComponentsInChildren<Collider>();
            Collider[] colliders2 = obj2.GetComponentsInChildren<Collider>();
            
            if (colliders1.Length == 0 || colliders2.Length == 0)
            {
                return Vector3.Distance(obj1.transform.position, obj2.transform.position);
            }

            float minDistance = float.MaxValue;
            
            foreach (Collider col1 in colliders1)
            {
                foreach (Collider col2 in colliders2)
                {
                    float currentDistance = ColliderDistance(col1, col2);
                    
                    if (currentDistance < minDistance)
                    {
                        minDistance = currentDistance;
                        
                        if (minDistance <= 0f) return 0f;
                    }
                }
            }

            return minDistance;
        }

        private float ColliderDistance(Collider col1, Collider col2)
        {
            if (col1 is MeshCollider || col2 is MeshCollider)
            {
                Vector3 closestPoint1 = col1.ClosestPoint(col2.transform.position);
                Vector3 closestPoint2 = col2.ClosestPoint(closestPoint1);
                
                return Vector3.Distance(closestPoint1, closestPoint2);
            }
            
            if (Physics.ComputePenetration(
                    col1, col1.transform.position, col1.transform.rotation,
                    col2, col2.transform.position, col2.transform.rotation,
                    out Vector3 direction, out float distance))
            {
                return -distance;
            }
            
            Vector3 point1 = col1.ClosestPoint(col2.bounds.center);
            Vector3 point2 = col2.ClosestPoint(point1);
            
            return Vector3.Distance(point1, point2);
        }
    }
}