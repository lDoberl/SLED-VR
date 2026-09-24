using System;
using System.Collections.Generic;
using CSI.Runtime;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace CSI.Interaction
{
    /// <summary>
    /// A single snapshot taken with the evidence camera — a real captured image, not just a
    /// "this evidence was photographed" flag.
    /// </summary>
    public class CapturedPhoto
    {
        public Sprite Thumbnail;
        public string TimestampUtc;
    }

    /// <summary>
    /// Handheld evidence camera (XRGrabInteractable). Pressing the grabbing controller's activate
    /// (trigger) button while holding it takes a photo: everything IPhotographable currently in the
    /// camera's view frustum gets marked photographed in EvidenceManager, AND a real snapshot is
    /// captured from the camera's render texture for the in-game photo gallery.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class CameraTool : MonoBehaviour
    {
        [SerializeField] private Camera photoCamera;

        public event Action OnShutter;

        /// <summary>Static so any listener (e.g. the inventory's photo gallery) can subscribe
        /// without needing a direct reference to a specific camera instance.</summary>
        public static event Action<CapturedPhoto> OnAnyPhotoCaptured;

        private XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            _grabInteractable.activated.AddListener(HandleActivated);
        }

        private void OnDisable()
        {
            _grabInteractable.activated.RemoveListener(HandleActivated);
        }

        private void HandleActivated(ActivateEventArgs args)
        {
            TakePhoto();
        }

        public void TakePhoto()
        {
            if (photoCamera == null) return;

            MarkPhotographableEvidenceInFrame();

            var thumbnail = CaptureSnapshot();
            if (thumbnail != null)
            {
                OnAnyPhotoCaptured?.Invoke(new CapturedPhoto
                {
                    Thumbnail = thumbnail,
                    TimestampUtc = DateTime.UtcNow.ToString("o")
                });
            }

            OnShutter?.Invoke();
        }

        private void MarkPhotographableEvidenceInFrame()
        {
            if (EvidenceManager.Instance == null) return;

            var planes = GeometryUtility.CalculateFrustumPlanes(photoCamera);
            var photographed = new List<string>();

            foreach (var item in PhotographableRegistry.All)
            {
                if (GeometryUtility.TestPlanesAABB(planes, item.WorldBounds))
                {
                    EvidenceManager.Instance.MarkPhotographed(item.EvidenceId);
                    photographed.Add(item.EvidenceId);
                }
            }

            Debug.Log($"[CameraTool] Shutter — photographed: {(photographed.Count > 0 ? string.Join(", ", photographed) : "nothing in frame")}");
        }

        private Sprite CaptureSnapshot()
        {
            var rt = photoCamera.targetTexture;
            if (rt == null) return null;

            var previousActive = RenderTexture.active;
            RenderTexture.active = rt;

            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            RenderTexture.active = previousActive;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
