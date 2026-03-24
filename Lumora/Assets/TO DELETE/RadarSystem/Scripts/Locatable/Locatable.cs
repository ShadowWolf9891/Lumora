using Ilumisoft.RadarSystem.UI;
using UnityEngine;

namespace Ilumisoft.RadarSystem
{
    [AddComponentMenu("Radar System/Locatable (Radar System)")]
    public class Locatable : LocatableComponent
    {
        [SerializeField]
        protected LocatableIconComponent iconPrefab;

        [SerializeField, Tooltip("If enabled the locatable will stay visible on the radar and compass when being out of the detection radius - useful for quest markers etc.")]
        private bool ignoreDetectionRange = false;

        public override bool IgnoreDetectionRange { get => ignoreDetectionRange; set => ignoreDetectionRange = value; }

        private void Awake()
        {
            if(iconPrefab == null)
            {
                Debug.LogWarning("Locatable (Radar System): Icon prefab has not been assigned!", this);
            }
        }

        public override LocatableIconComponent CreateIcon()
        {
            return Instantiate(iconPrefab);
        }
    }
}