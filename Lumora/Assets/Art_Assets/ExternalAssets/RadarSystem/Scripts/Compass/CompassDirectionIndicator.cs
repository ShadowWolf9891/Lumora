using UnityEngine;

namespace Ilumisoft.RadarSystem.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class CompassDirectionIndicator : MonoBehaviour
    {
        [Range(0, 360)]
        public float direction = 0;

        Compass compass;

        RectTransform rectTransform;

        private void Awake()
        {
            compass = GetComponentInParent<Compass>();
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (compass == null)
            {
                return;
            }

            Vector3 cameraForwardDirectionXZ = Vector3.forward;

            var cameraTransform = compass.CameraTransform;

            if (cameraTransform != null)
            {
                cameraForwardDirectionXZ = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
            }

            float angle = Vector3.SignedAngle(cameraForwardDirectionXZ, Vector3.forward, Vector3.up);

            angle = (angle + direction + 180) % 360 - 180;

            rectTransform.anchoredPosition = new Vector2(compass.Width * angle / compass.DetectionAngle, 0);
        }
    }
}