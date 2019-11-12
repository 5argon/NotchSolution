using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    [Serializable]
    internal class Settings : GenericProjectSettings<Settings>
    {
        protected override int DataVersion => 1;
        protected override int SerializedVersion { get => version; set => version = value; }

        private const string fileName = "NotchSolutionSettings";
        protected override string FileName => fileName;

        [SerializeField] private int activeConfigurationIndex;
        [SerializeField] private int version;
        [SerializeField] private List<PerPlatformConfigurations> perPlatformConfigurations;

        [field: SerializeField] internal bool EnableSimulation { get; set; }
        [field: SerializeField] internal bool FlipOrientation { get; set; }
        [field: SerializeField] internal Color PrefabModeOverlayColor { get; set; }

        public Settings()
        {
            perPlatformConfigurations = new List<PerPlatformConfigurations>();
            PrefabModeOverlayColor = new Color(0.297F, 0.405F, 0.481F, 1);
        }

        /// <summary>
        /// Get a configuration depending on the current build target and configuration index.
        /// </summary>
        internal NotchSimulatorConfiguration ActiveConfiguration
        {
            get
            {
                EnsureValidConfigurationIndex();
                return PlatformConfigurations[activeConfigurationIndex];
            }
        }

        /// <summary>
        /// Cause <see cref="ActiveConfiguration"/> to go forward or wrap around to the first one when it can't.
        /// </summary>
        public NotchSimulatorConfiguration NextConfiguration()
        {
            EnsureValidConfigurationIndex();

            var count = PlatformConfigurations.ConfigurationCount;
            activeConfigurationIndex = (activeConfigurationIndex + 1) % count;
            Save();
            return ActiveConfiguration;
        }

        /// <summary>
        /// Get a configuration depending on the current build target.
        /// </summary>
        private PerPlatformConfigurations PlatformConfigurations
        {
            get
            {
                var conf = perPlatformConfigurations.FirstOrDefault(x => x.BuildTarget == EditorUserBuildSettings.activeBuildTarget);
                if (conf == default(PerPlatformConfigurations))
                {
                    var newConfig = new PerPlatformConfigurations(EditorUserBuildSettings.activeBuildTarget);
                    perPlatformConfigurations.Add(newConfig);
                    Save();
                    return newConfig;
                }
                else
                {
                    return conf;
                }
            }
        }

        private void EnsureValidConfigurationIndex()
        {
            var count = PlatformConfigurations.ConfigurationCount;
            if(activeConfigurationIndex >= count)
            {
                activeConfigurationIndex = 0;
                Save();
            }
        }
    }
}
