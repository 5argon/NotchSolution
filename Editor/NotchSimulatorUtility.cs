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
                var device = SimulationDatabase.db.ContainsKey(selectedDevice) ?  selectedDevice : default;
                var safe = orientation == ScreenOrientation.Landscape ? SimulationDatabase.db[device].landscapeSafeArea : SimulationDatabase.db[device].portraitSafeArea;
                var screenSize = SimulationDatabase.db[device].screenSize;
                if(orientation == ScreenOrientation.Landscape)
                {
                    var swap = screenSize.x;
                    screenSize.x = screenSize.y;
                    screenSize.y = swap;
                }
                var relativeSafeArea = new Rect(safe.xMin / screenSize.x, safe.yMin / screenSize.y, safe.width / screenSize.x, safe.height / screenSize.y);

                if (NotchSimulatorUtility.flipOrientation)
                {
                    return new Rect(
                        1 - (relativeSafeArea.width + relativeSafeArea.xMin),
                        1 - (relativeSafeArea.height + relativeSafeArea.yMin),
                        relativeSafeArea.width,
                        relativeSafeArea.height
                    );
                }
                else
                {
                    return relativeSafeArea;
                }
            }
        }

        internal static ScreenOrientation GetGameViewOrientation()
        {
            var gameViewSize = GetMainGameViewSize();
            return gameViewSize.x > gameViewSize.y ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }

        internal static Vector2 GetMainGameViewSize()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
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