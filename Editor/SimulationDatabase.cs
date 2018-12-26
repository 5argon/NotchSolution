using UnityEngine;
using System.Collections.Generic;

namespace E7.NotchSolution
{
    /// <summary>
    /// Please add more!
    /// </summary>
    public static class SimulationDatabase
    {
        public static Dictionary<SimulationDevice, SimulationDatabaseData> db = new Dictionary<SimulationDevice, SimulationDatabaseData>()
        {
            [SimulationDevice.iPhoneX] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 102, 1125, 2202),
                landscapeSafeArea = new Rect(132, 63, 2172, 1062),
                screenSize = new Vector2(1125, 2436),
            },
        };
    }
}