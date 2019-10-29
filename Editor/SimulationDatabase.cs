using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace E7.NotchSolution
{
    /// <summary>
    /// Please add more! [How to get device's safe area and cutouts](https://github.com/5argon/NotchSolution/issues/2).
    /// </summary>
    public static class SimulationDatabase
    {
        public static List<SimulationDevice> db = new List<SimulationDevice>();
        public static void Refresh()
        {
            db = new List<SimulationDevice>();

            var deviceDirectory = new System.IO.DirectoryInfo(NotchSimulatorUtility.devicesPath);
            if (!deviceDirectory.Exists) return;
            var deviceDefinitions = deviceDirectory.GetFiles("*.device.json");

            foreach (var deviceDefinition in deviceDefinitions)
            {
                SimulationDevice deviceInfo;
                using (var sr = deviceDefinition.OpenText())
                {
                    deviceInfo = JsonUtility.FromJson<SimulationDevice>(sr.ReadToEnd());
                }
                db.Add(deviceInfo);
            }
        }

        public static SimulationDevice Get(string device) { return db.FirstOrDefault(d => d.Meta.friendlyName == device); }
    }
}