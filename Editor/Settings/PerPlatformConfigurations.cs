using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    /// <summary>
    ///     When switching platforms the list of aspect ratio index changes completely.
    ///     Therefore we need a new set for each platforms.
    /// </summary>
    [Serializable]
    internal class PerPlatformConfigurations
    {
        [SerializeField]
        private BuildTarget buildTarget;

        /// <summary>
        ///     You can switch between multiple configurations for A-B testing while designing, for example.
        /// </summary>
        [SerializeField] private List<NotchSimulatorConfiguration> configurations;

        public PerPlatformConfigurations(BuildTarget buildTarget)
        {
            this.buildTarget = buildTarget;
            configurations = new List<NotchSimulatorConfiguration>
            {
                new NotchSimulatorConfiguration("Config A"),
                new NotchSimulatorConfiguration("Config B"),
            };
        }

        internal BuildTarget BuildTarget => buildTarget;

        internal int ConfigurationCount => configurations.Count;
        internal NotchSimulatorConfiguration this[int i] => configurations[i];
    }
}