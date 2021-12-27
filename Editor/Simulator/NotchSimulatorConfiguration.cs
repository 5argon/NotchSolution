using System;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    public enum PreviewOrientation
    {
        Portrait,
        PortraitUpsideDown,
        LandscapeLeft,
        LandscapeRight,
    }

    public enum GameViewSizePolicy
    {
        MatchResolution,
        MatchAspectRatio,
    }

    /// <summary>
    ///     Notch Solution can be in only 1 configuration at any point.
    /// </summary>
    [Serializable]
    internal class NotchSimulatorConfiguration
    {
        [SerializeField] private string configurationName;

        public NotchSimulatorConfiguration(string configurationName)
        {
            this.configurationName = configurationName;
        }

        internal string ConfigurationName => configurationName;

        /// <summary>
        ///     The device selected when linearize the simulator database and count from the beginning.
        /// </summary>
        [field: SerializeField] internal int DeviceIndex { get; set; }

        /// <summary>
        ///     Screen orientation of the preview device.
        /// </summary>
        [field: SerializeField] internal PreviewOrientation Orientation { get; set; }

        /// <summary>
        ///     Whether the game view's size should match the device's exact resolution or only its aspect ratio.
        /// </summary>
        [field: SerializeField] internal GameViewSizePolicy GameViewSize { get; set; }
    }
}