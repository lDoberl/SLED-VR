using System;
using Data;
using Systems.Omp;
using UI.Inventory;
using UnityEngine;

namespace Items
{
    public class Fingerprint : Evidence
    {
        public string FingerprintId;
        public string SurfaceName = "Не указано";
        public DateTime? TimeOfPhoto;
        [SerializeField] private string assignedSurfaceName;
        private bool _pendingDestroy = false;
        
        protected override void Awake()
        {
            LoadEvidenceData();
            if (Data)
            {
                evidenceObject.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!_pendingDestroy || !evidenceObject) return;
            Destroy(evidenceObject);
            Destroy(gameObject);
            _pendingDestroy = false;
        }

        protected override void LoadEvidenceData()
        {
            Data = EvidenceDatabase.Instance.GetEvidenceById<FingerprintData>(evidenceId);
            
            if (!Data)
            {
                Debug.LogError($"Fingerprint with ID {evidenceId} not found in database!");
            }
        }
        
        public override void Activate()
        {
            if (!Data) return;
            
            evidenceObject.SetActive(true);
            
            EvidenceDatabase.Instance.DiscoverEvidence(evidenceId);
        }
        
        public void FixatePhoto()
        {
            if (!TimeOfPhoto.HasValue)
            {
                EvidenceDatabase.Instance.GetEvidenceById<FingerprintData>(evidenceId).TimeOfPhoto = DateTime.Now;
                TimeOfPhoto = DateTime.Now;
                SurfaceName = assignedSurfaceName;
                Debug.Log($"Отпечаток зафиксирован: {SurfaceName} в {TimeOfPhoto}");
            }
        }

        public override void DeActivate(InventoryItem item, AdaptiveGridInventory inventory)
        {
            // Проверка протокола: перед изъятием отпечаток должен быть сфотографирован
            if (!TimeOfPhoto.HasValue)
            {
                var analyzer = OmpActionAnalyzer.Instance;
                if (analyzer)
                {
                    analyzer.RegisterCustomError(
                        penaltyId: $"no-photo-before-fingerprint-lift-{FingerprintId}",
                        description: "Отпечаток изъят без предварительной фотофиксации.",
                        points: 5f,
                        relatedActionId: "LIFT_FINGERPRINT",
                        context: FingerprintId
                    );
                }
                else
                {
                    Debug.LogWarning("Fingerprint.DeActivate: OmpActionAnalyzer not found, штраф не зарегистрирован.");
                }
            }

            item.surfaceName = SurfaceName;
            item.timeOfPhoto = TimeOfPhoto;
            item.itemId = FingerprintId;
            inventory.AddItem(item);

            if (item.timeOfPhoto.HasValue)
            {
                Debug.Log(item.timeOfPhoto.Value);
            }

            evidenceObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
