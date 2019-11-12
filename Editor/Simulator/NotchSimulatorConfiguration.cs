using System;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    /// <summary>
    /// Notch Solution can be in only 1 configuration at any point.
    /// </summary>
    [Serializable]
    internal class NotchSimulatorConfiguration
    {
        [SerializeField] private string configurationName;
        internal string ConfigurationName => configurationName;
        
        /// <summary>
        /// The device selected when linearize the simulator database and count from the beginning.
        /// </summary>
        [field: SerializeField] internal int DeviceIndex { get; set; }

        public NotchSimulatorConfiguration(string configurationName)
        {
            this.configurationName = configurationName;
        }
    }
}
