using UnityEngine;
using UnityEditor;

namespace E7.NotchSolution
{
    public static class NotchSimulatorUtility
    {
        const string enableSimulationKey = NotchSolutionUtility.prefix + "enableSimulation";
        const string simulationDeviceKey = NotchSolutionUtility.prefix + "simulationDevice";
        const string flipOrientationKey = NotchSolutionUtility.prefix + "flipOrientation";

        internal static Rect SimulatorSafeAreaRelative
        {
            get
            {
                var orientation = GetGameViewOrientation();
                var device = SimulationDatabase.db.ContainsKey(selectedDevice) ? selectedDevice : default;
                var safe = orientation == ScreenOrientation.Landscape ? SimulationDatabase.db[device].landscapeSafeArea : SimulationDatabase.db[device].portraitSafeArea;
                var screenSize = SimulationDatabase.db[device].screenSize;
                if (orientation == ScreenOrientation.Landscape)
                {
                    var swap = screenSize.x;
                    screenSize.x = screenSize.y;
                    screenSize.y = swap;
                }
                return GetRelative(safe, screenSize);
            }
        }

        internal static Rect[] SimulatorCutoutsRelative
        {
            get
            {
                var orientation = GetGameViewOrientation();
                var device = SimulationDatabase.db.ContainsKey(selectedDevice) ? selectedDevice : default;
                var cutouts = orientation == ScreenOrientation.Landscape ? SimulationDatabase.db[device].landscapeCutouts : SimulationDatabase.db[device].portraitCutouts;
                if (cutouts is null) return new Rect[0];
                var screenSize = SimulationDatabase.db[device].screenSize;
                if (orientation == ScreenOrientation.Landscape)
                {
                    var swap = screenSize.x;
                    screenSize.x = screenSize.y;
                    screenSize.y = swap;
                }

                System.Collections.Generic.List<Rect> rects = new System.Collections.Generic.List<Rect>();
                foreach (var cutout in cutouts) rects.Add(GetRelative(cutout, screenSize));
                return rects.ToArray();
            }
        }

        static Rect GetRelative(Rect original, Vector2 screenSize)
        {
            var relativeCutout = new Rect(original.xMin / screenSize.x, original.yMin / screenSize.y, original.width / screenSize.x, original.height / screenSize.y);
            if (flipOrientation)
            {
                return new Rect(
                    1 - (relativeCutout.width + relativeCutout.xMin),
                    1 - (relativeCutout.height + relativeCutout.yMin),
                    relativeCutout.width,
                    relativeCutout.height
                );
            }
            else return relativeCutout;
        }

        internal static ScreenOrientation GetGameViewOrientation()
        {
            var gameViewSize = GetMainGameViewSize();
            return gameViewSize.x > gameViewSize.y ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }

        static System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        static System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetMainGameViewTargetSizeNoBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        private static object[] argsForOut = new object[1];
        internal static Vector2 GetMainGameViewSize()
        {
            System.Object Res = GetSizeOfMainGameView.Invoke(null, argsForOut);
            return (Vector2)argsForOut[0];
        }

        internal static bool enableSimulation
        {
            get { return EditorPrefs.GetBool(enableSimulationKey); }
            set { EditorPrefs.SetBool(enableSimulationKey, value); }
        }

        internal static bool flipOrientation
        {
            get { return EditorPrefs.GetBool(flipOrientationKey); }
            set { EditorPrefs.SetBool(flipOrientationKey, value); }
        }

        internal static SimulationDevice selectedDevice
        {
            get { return (SimulationDevice)EditorPrefs.GetInt(simulationDeviceKey); }
            set { EditorPrefs.SetInt(simulationDeviceKey, (int)value); }
        }
    }
}