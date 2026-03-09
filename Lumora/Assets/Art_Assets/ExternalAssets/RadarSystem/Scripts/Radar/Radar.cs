using Ilumisoft.RadarSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.RadarSystem
{
    [AddComponentMenu("Radar System/Radar")]
    [DefaultExecutionOrder(-10)]
    public class Radar : MonoBehaviour
    {
        /// <summary>
        /// Dictionary allowing to access the icon of a locatable
        /// </summary>
        readonly Dictionary<LocatableComponent, LocatableIconComponent> locatableIconDictionary = new();

        [field: SerializeField]
        public bool Use2DMode = false;

        [field: SerializeField]
        public Transform PlayerTransform { get; set; } = null;

        [field: SerializeField]
        public Transform CameraTransform { get; set; } = null;

        [SerializeField, Min(1)]
        [Tooltip("The detection range of the radar in meter")]
        private float detectionRange = 100;

        [SerializeField]
        [Tooltip("Prevents the radar from being rotated")]
        private bool lockRotation = true;

        [SerializeField]
        [Range(0f, 360f)]
        private float rotationOffset = 0.0f;

        [Header("Icon Settings")]
        [SerializeField]
        [Tooltip("The container icons will be added to")]
        private RectTransform iconContainer;

        [SerializeField, Min(0)]
        float iconScaleMultiplier = 1.0f;

        /// <summary>
        /// The detection range of the radar
        /// </summary>
        public float DetectionRange { get => detectionRange; set => detectionRange = value; }

        /// <summary>
        /// Whether the radar rotates with the player or not
        /// </summary>
        public bool LockRotation { get => lockRotation; set => lockRotation = value; }
        public float RotationOffset { get => rotationOffset; set => rotationOffset = value; }

        private void Awake()
        {
            if (PlayerTransform == null)
            {
                Debug.LogWarning("Radar System: Player transform has not been assigned", this);
            }

            if (CameraTransform == null)
            {
                Debug.LogWarning("Radar System: Camera transform has not been assigned", this);
            }
        }

        private void OnEnable()
        {
            LocatableManager.OnLocatableAdded += OnLocatableAdded;
            LocatableManager.OnLocatableRemoved += OnLocatableRemoved;
        }

        private void OnDisable()
        {
            LocatableManager.OnLocatableAdded -= OnLocatableAdded;
            LocatableManager.OnLocatableRemoved -= OnLocatableRemoved;
        }

        /// <summary>
        /// Callback invoked when a locatable has been added
        /// </summary>
        /// <param name="locatable"></param>
        private void OnLocatableAdded(LocatableComponent locatable)
        {
            // Create the icon for the locatable and add a new entry to the dictionary
            if (locatable != null && !locatableIconDictionary.ContainsKey(locatable))
            {
                var icon = locatable.CreateIcon();

                icon.transform.SetParent(iconContainer.transform, false);

                locatableIconDictionary.Add(locatable, icon);
            }
        }

        /// <summary>
        /// Callback invoked when a locatable has been removed
        /// </summary>
        /// <param name="locatable"></param>
        private void OnLocatableRemoved(LocatableComponent locatable)
        {
            // Remove the locatable from the dictionary and destroy the icon
            if (locatable != null && locatableIconDictionary.TryGetValue(locatable, out LocatableIconComponent icon))
            {
                locatableIconDictionary.Remove(locatable);

                Destroy(icon.gameObject);
            }
        }

        private void Update()
        {
            if (PlayerTransform != null && CameraTransform != null)
            {
                Vector2 iconLocation;
                bool isVisible = false;

                // Run through all locatables in the dictionary
                foreach (var locatable in locatableIconDictionary.Keys)
                {
                    // Update the icon position and visibility for the locatable
                    if (locatableIconDictionary.TryGetValue(locatable, out var icon))
                    {
                        isVisible = Use2DMode ? TryGetIconLocation2D(locatable, out iconLocation) : TryGetIconLocation(locatable, out iconLocation);

                        if (isVisible)
                        {
                            icon.SetVisible(true);

                            var rectTransform = icon.GetComponent<RectTransform>();

                            rectTransform.anchoredPosition = iconLocation;
                            rectTransform.localScale = Vector3.one * iconScaleMultiplier;
                        }
                        else
                        {
                            icon.SetVisible(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Computes the location of the icon on the radar. Returns true if the icon is visible, false otherwise
        /// </summary>
        /// <param name="locatable"></param>
        /// <param name="iconLocation"></param>
        /// <returns></returns>
        private bool TryGetIconLocation(LocatableComponent locatable, out Vector2 iconLocation)
        {
            Vector3 distanceToPlayer = locatable.transform.position - PlayerTransform.position;

            iconLocation = new Vector2(distanceToPlayer.x, distanceToPlayer.z);

            float radarSize = GetRadarUISize();

            var scale = radarSize / DetectionRange;

            iconLocation *= scale;

            // Rotate the icon by the players y rotation if enabled

            // Get the forward vector of the player projected on the xz plane
            var cameraForwardDirectionXZ = Vector3.ProjectOnPlane(CameraTransform.forward, Vector3.up);

            // Create a roation from the direction
            var rotation = LockRotation ? Quaternion.identity : Quaternion.LookRotation(cameraForwardDirectionXZ);

            // Mirror y rotation
            var euler = rotation.eulerAngles;
            euler.y = -euler.y;
            euler.y += rotationOffset;
            rotation.eulerAngles = euler;

            // Rotate the icon location in 3D space
            var rotatedIconLocation = rotation * new Vector3(iconLocation.x, 0.0f, iconLocation.y);

            // Convert from 3D to 2D
            iconLocation = new Vector2(rotatedIconLocation.x, rotatedIconLocation.z);


            if (iconLocation.sqrMagnitude < radarSize * radarSize || locatable.IgnoreDetectionRange)
            {
                // Make sure it is not shown outside the radar
                iconLocation = Vector2.ClampMagnitude(iconLocation, radarSize);

                return true;
            }

            return false;
        }

        private bool TryGetIconLocation2D(LocatableComponent locatable, out Vector2 iconLocation)
        {
            Vector3 distanceToPlayer = locatable.transform.position - PlayerTransform.position;

            iconLocation = new Vector2(distanceToPlayer.x, distanceToPlayer.y);

            float radarSize = GetRadarUISize();

            var scale = radarSize / DetectionRange;

            iconLocation *= scale;

            if (iconLocation.sqrMagnitude < radarSize * radarSize || locatable.IgnoreDetectionRange)
            {
                // Make sure it is not shown outside the radar
                iconLocation = Vector2.ClampMagnitude(iconLocation, radarSize);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the size of the radar UI
        /// </summary>
        /// <returns></returns>
        private float GetRadarUISize()
        {
            return iconContainer.rect.width / 2;
        }
    }
}