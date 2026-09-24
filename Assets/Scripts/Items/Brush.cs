using System;
using Data;
using UnityEngine;

namespace Items
{
    public class Brush : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fingerprint"))
            {
                Fingerprint fingerprint = other.gameObject.GetComponent<Fingerprint>();
                
                if (fingerprint)
                {
                    FingerprintData data = fingerprint.GetEvidenceData() as FingerprintData;
                    
                    if (data && !data.IsDiscovered)
                    {
                        fingerprint.Activate();
                        Debug.Log($"Found fingerprint belonging to: {data.OwnerName}");
                    }
                }
            }
        }
    }
}
