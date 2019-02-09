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
            [SimulationDevice.OnePlus6T] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 79, 1080, 2261),
                landscapeSafeArea = new Rect(79, 0, 2261, 1080),
                screenSize = new Vector2(1080, 2340),
            },
            [SimulationDevice.HuaweiMate20Pro] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 100, 1440, 3020),
                landscapeSafeArea = new Rect(100, 0, 3020, 1440),
                screenSize = new Vector2(1440, 3120),
            },
        };
    }
}