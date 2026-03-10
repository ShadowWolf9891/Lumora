using Ilumisoft.RadarSystem.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.RadarSystem
{
    [DefaultExecutionOrder(-10)]
    public class Compass : MonoBehaviour
    {
        /// <summary>
        /// Dictionary allowing to access the icon of a locatable
        /// </summary>
        readonly Dictionary<LocatableComponent, LocatableIconComponent> locatableIconDictionary = new();

        [field:SerializeField]
        public Transform PlayerTransform { get; set; } = null;

        [field: SerializeField]
        public Transform CameraTransform { get; set; } = null;

        [SerializeField, Min(1)]
        [Tooltip("The detection range of the radar in meter")]
        private float detectionRange = 100;

        [SerializeField, Range(0, 360)]
        private float detectionAngle = 180;

        [Header("Icon Settings")]
        [SerializeField]
        [Tooltip("The container icons will be added to")]
        private RectTransform iconContainer;

        [SerializeField, Range(0, 1)]
        float minIconScale = 0.5f;

        [SerializeField, Min(0)]
        float minIconScaleDistance = 50.0f;

        [SerializeField, Min(0)]
        float iconScaleMultiplier = 1.0f;

        /// <summary>
        /// The detection range of the radar
        /// </summary>
        public float DetectionRange { get => detectionRange; set => detectionRange = value; }
        public float DetectionAngle { get => detectionAngle; set => detectionAngle = value; }

        public float Width => iconContainer.rect.width;

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
                // Run through all locatables in the dictionary
                foreach (var locatable in locatableIconDictionary.Keys)
                {
                    // Update the icon position and visibility for the locatable
                    if (locatableIconDictionary.TryGetValue(locatable, out var icon))
                    {
                        var rectTransform = icon.GetComponent<RectTransform>();

                        Vector3 distanceToPlayerVector = locatable.transform.position - PlayerTransform.transform.position;
                        float distanceToPlayer = distanceToPlayerVector.magnitude;

                        var directionFromPlayerXZ = Vector3.ProjectOnPlane(distanceToPlayerVector.normalized, Vector3.up);
                        var cameraForwardDirectionXZ = Vector3.ProjectOnPlane(CameraTransform.forward, Vector3.up);

                        var angle = Vector3.SignedAngle(cameraForwardDirectionXZ, directionFromPlayerXZ, Vector3.up);

                        rectTransform.anchoredPosition = new Vector2(iconContainer.rect.width * angle / DetectionAngle, 0);
                        rectTransform.localScale = iconScaleMultiplier * Mathf.Lerp(1, minIconScale, Mathf.Clamp01(distanceToPlayer / minIconScaleDistance)) * Vector3.one;

                        if (distanceToPlayer < DetectionRange || locatable.IgnoreDetectionRange)
                        {
                            icon.SetVisible(true);
                        }
                        else
                        {
                            icon.SetVisible(false);
                        }
                    }
                }
            }
        }
    }
}