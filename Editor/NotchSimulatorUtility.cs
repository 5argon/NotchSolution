using UnityEngine;
using UnityEditor;

namespace E7.NotchSolution
{
    public static class NotchSimulatorUtility
    {
        const string prefix = "NotchSolution_";
        const string enableSimulationKey = prefix + "enableSimulation";
        const string simulationDeviceKey = prefix + "simulationDevice";

        internal static Rect SimulatorSafeAreaRelative
        {
            get
            {
                var gameViewSize = GetMainGameViewSize();
                var orientation = gameViewSize.x > gameViewSize.y ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
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
                return relativeSafeArea;
            }
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

        internal static SimulationDevice selectedDevice
        {
            get { return (SimulationDevice)EditorPrefs.GetInt(simulationDeviceKey); }
            set { EditorPrefs.SetInt(simulationDeviceKey, (int)value); }
        }
    }
}