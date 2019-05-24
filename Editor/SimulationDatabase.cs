using UnityEngine;
using System.Collections.Generic;

namespace E7.NotchSolution
{
    /// <summary>
    /// Please add more! [How to get device's safe area and cutouts](https://github.com/5argon/NotchSolution/issues/2).
    /// </summary>
    public static class SimulationDatabase
    {
        public static Dictionary<SimulationDevice, SimulationDatabaseData> db = new Dictionary<SimulationDevice, SimulationDatabaseData>()
        {
            [SimulationDevice.iPhoneX] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 102, 1125, 2202),
                portraitCutouts = new Rect[]{
                    new Rect (250, 2346, 625, 90),
                },
                landscapeSafeArea = new Rect(132, 63, 2172, 1062),
                landscapeCutouts = new Rect[]{
                    new Rect (2346, 250, 90, 625),
                },
                screenSize = new Vector2(1125, 2436),
            },
            [SimulationDevice.iPadPro] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 40, 2048, 2692),
                landscapeSafeArea = new Rect(0, 40, 2732, 2008),
                screenSize = new Vector2(2048, 2732),
            },
            [SimulationDevice.OnePlus6T] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 0, 1080, 2261),
                portraitCutouts = new Rect[]{
                    new Rect (372, 2261, 334, 79),
                },
                landscapeSafeArea = new Rect(0, 0, 2261, 1080),
                landscapeCutouts = new Rect[]{
                    new Rect (2261, 374, 79, 334),
                },
                screenSize = new Vector2(1080, 2340),
            },
            [SimulationDevice.HuaweiMate20Pro] = new SimulationDatabaseData
            {
                portraitSafeArea = new Rect(0, 0, 1440, 3020),
                portraitCutouts = new Rect[]{
                    new Rect (374, 3020, 693, 100),
                },
                landscapeSafeArea = new Rect(100, 0, 3020, 1440),
                landscapeCutouts = new Rect[]{
                    new Rect (0, 374, 100, 693),
                },
                screenSize = new Vector2(1440, 3120),
            },
        };
    }
}