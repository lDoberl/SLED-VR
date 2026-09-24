using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class PhotoAlbum : MonoBehaviour
    {
        [Serializable]
        public class PhotoData
        {
            public Texture2D texture;
            public DateTime dateTime;
        }

        [SerializeField] private RawImage photoDisplay;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private TextMeshProUGUI dateText;

        private List<PhotoData> _photos = new List<PhotoData>();
        private int _currentIndex = -1;

        public void AddPhoto(Texture2D tex, DateTime dateTime)
        {
            _photos.Add(new PhotoData { texture = tex, dateTime = dateTime });
            _currentIndex = _photos.Count - 1;
            ShowCurrentPhoto();
        }

        public void NextPhoto()
        {
            if (_currentIndex >= _photos.Count - 1) return;
            _currentIndex++;
            ShowCurrentPhoto();
        }

        public void PrevPhoto()
        {
            if (_currentIndex <= 0) return;
            _currentIndex--;
            ShowCurrentPhoto();
        }

        public void ShowCurrentPhoto()
        {
            if (_photos.Count == 0 || _currentIndex < 0)
            {
                photoDisplay.texture = null;
                dateText.text = "";
                nextButton.gameObject.SetActive(false);
                prevButton.gameObject.SetActive(false);
                return;
            }

            photoDisplay.texture = _photos[_currentIndex].texture;
            dateText.text = _photos[_currentIndex].dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            nextButton.gameObject.SetActive(_currentIndex < _photos.Count - 1);
            prevButton.gameObject.SetActive(_currentIndex > 0);
        }
    }
}