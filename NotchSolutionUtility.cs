using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.NotchSolution
{
    public static class SimulatorDatabase
    {
        public static Dictionary<SimulationDevice, SimulatorDatabaseData> db = new Dictionary<SimulationDevice, SimulatorDatabaseData>()
        {
            [SimulationDevice.iPhoneX] = new SimulatorDatabaseData
            {
                portraitSafeArea = new Rect(0, 102, 1125, 2202),
                landscapeSafeArea = new Rect(132, 63, 2172, 1062),
                screenSize = new Vector2(1125, 2436),
            },
        };
    }

    public enum SimulationDevice
    {
        iPhoneX,
    }

    public class SimulatorDatabaseData
    {
        public Rect portraitSafeArea;
        public Rect landscapeSafeArea;
        public Vector2 screenSize; //width x height
    }

    public static class NotchSolutionUtility
    {
        public static ScreenOrientation GetCurrentOrientation()
        {
#if UNITY_EDITOR
            var gameViewSize = GetMainGameViewSize();
            return gameViewSize.x > gameViewSize.y ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
#else
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
#endif
        }

#if UNITY_EDITOR
        const string prefix = "NotchSolution_";
        const string enableSimulationKey = prefix + "enableSimulation";
        const string simulateNotchSizeKey = prefix + "simulateNotchSize";
        const string simulateScreenOrientationKey = prefix + "simulateScreenOrientation";

        private static Rect iPhoneXPortraitSafeArea = new Rect(0f, 102f / 2436f, 1f, 2202f / 2436f);
        private static Rect iPhoneXLandscapeSafeArea = new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f);

        public static Rect SimulatorSafeArea
        {
            get
            {
                return GetCurrentOrientation() == ScreenOrientation.Landscape ? iPhoneXLandscapeSafeArea : iPhoneXPortraitSafeArea;
            }
        }

        private static Vector2 GetMainGameViewSize()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            return (Vector2)Res;
        }

        public static bool enableSimulation 
        {
            get { return EditorPrefs.GetBool(enableSimulationKey); }
            set { EditorPrefs.SetBool(enableSimulationKey, value); }
        }

        public static int simulateNotchSize
        {
            get { return EditorPrefs.GetInt(simulateNotchSizeKey); }
            set { EditorPrefs.SetInt(simulateNotchSizeKey, value); }
        }

        public static ScreenOrientation simulateScreenOrientation
        {
            get { return (ScreenOrientation) EditorPrefs.GetInt(simulateScreenOrientationKey); }
            set { EditorPrefs.SetInt(simulateScreenOrientationKey, (int)value); }
        }
    }
#endif
}