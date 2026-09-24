using System;
using Data;
using TMPro;
using UI.Inventory;
using UnityEngine;

namespace UI.Convert
{
    public class ConvertConfig : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI date;
        [SerializeField] private TextMeshProUGUI place;
        private PickupableItem _otherObject;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PickupableItem>().Category == ToolCategory.Traces)
            {
                if (other.CompareTag("Fingerprint"))
                {
                    _otherObject = other.gameObject.GetComponent<PickupableItem>();
                    
                    var timeOfPhoto = EvidenceDatabase.Instance.GetEvidenceById<FingerprintData>(_otherObject.ItemId).TimeOfPhoto;
                    if (timeOfPhoto != null)
                    {
                        date.text = timeOfPhoto.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        if (_otherObject.SurfaceName != null)
                        {
                            place.text = _otherObject.SurfaceName;
                        }
                        else
                        {
                            place.text = "-";
                        }
                    }
                    else
                    {
                        date.text = "-";
                        place.text = "-";
                    }
                    other.gameObject.SetActive(false);
                }
                else
                {
                    _otherObject = other.gameObject.GetComponent<PickupableItem>();

                    var timeOfPhoto = _otherObject.TimeOfPhoto;
                    if (timeOfPhoto != null)
                    {
                        date.text = timeOfPhoto.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        if (_otherObject.SurfaceName != null)
                        {
                            place.text = _otherObject.SurfaceName;
                        }
                        else
                        {
                            place.text = "-";
                        }
                    }
                    else
                    {
                        date.text = "-";
                        place.text = "-";
                    }
                    other.gameObject.SetActive(false);
                }
            }
        }

        public PickupableItem ConvertFingerprint()
        {
            if (_otherObject)
            {
                _otherObject.SetDisplayName(_otherObject.DisplayName + " в конверте");
                Destroy(gameObject);
                return _otherObject;
            }
            Destroy(gameObject);
            return null;
        }
    }
}
