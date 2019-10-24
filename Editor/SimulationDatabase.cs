using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Xml.Serialization;

namespace E7.NotchSolution
{
    /// <summary>
    /// Please add more! [How to get device's safe area and cutouts](https://github.com/5argon/NotchSolution/issues/2).
    /// </summary>
    public static class SimulationDatabase
    {
        public static List<DeviceInfo> db = new List<DeviceInfo>();
        public static void Refresh()
        {
            db = new List<DeviceInfo>();

            var deviceDirectory = new DirectoryInfo(NotchSimulatorUtility.devicesPath);
            if (!deviceDirectory.Exists) return;
            var deviceDefinitions = deviceDirectory.GetFiles("*.device.json");

            foreach (var deviceDefinition in deviceDefinitions)
            {
                DeviceInfo deviceInfo;
                using (StreamReader sr = deviceDefinition.OpenText())
                {
                    deviceInfo = JsonUtility.FromJson<DeviceInfo>(sr.ReadToEnd());
                }
                db.Add(deviceInfo);
            }
        }

        public static DeviceInfo Get(string device) { return db.FirstOrDefault(d => d.Meta.friendlyName == device); }
    }
}