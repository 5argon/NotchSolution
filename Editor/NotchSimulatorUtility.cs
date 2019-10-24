using UnityEngine;
using UnityEditor;
using System.Linq;

namespace E7.NotchSolution
{
    public static class NotchSimulatorUtility
    {
        const string enableSimulationKey = NotchSolutionUtility.prefix + "enableSimulation";
        const string simulationDeviceKey = NotchSolutionUtility.prefix + "simulationDevice";
        const string flipOrientationKey = NotchSolutionUtility.prefix + "flipOrientation";

        internal static Rect CalculateSimulatorSafeAreaRelative()
        {
            var orientation = GetGameViewOrientation();
            var device = selectedDevice;
            var safe = device.Screens.FirstOrDefault().orientations[orientation].safeArea;
            var screenSize = new Vector2(device.Screens.FirstOrDefault().width, device.Screens.FirstOrDefault().height);
            if (orientation == ScreenOrientation.Landscape)
            {
                var swap = screenSize.x;
                screenSize.x = screenSize.y;
                screenSize.y = swap;
            }
            return GetRectRelativeToScreenSize(safe, screenSize);
        }

        internal static Rect[] CalculateSimulatorCutoutsRelative()
        {
            var orientation = GetGameViewOrientation();
            var device = selectedDevice;
            var cutouts = device.Screens.FirstOrDefault().orientations[orientation].cutouts;
            if (cutouts is null) return new Rect[0];
            var screenSize = new Vector2(device.Screens.FirstOrDefault().width, device.Screens.FirstOrDefault().height);
            if (orientation == ScreenOrientation.Landscape)
            {
                var swap = screenSize.x;
                screenSize.x = screenSize.y;
                screenSize.y = swap;
            }

            System.Collections.Generic.List<Rect> rects = new System.Collections.Generic.List<Rect>();
            foreach (var cutout in cutouts) rects.Add(GetRectRelativeToScreenSize(cutout, screenSize));
            return rects.ToArray();
        }

        /// <param name="original">Must be inside of a rect positioned at 0,0 that has width and height as <paramref name="screenSize"></param>
        static Rect GetRectRelativeToScreenSize(Rect original, Vector2 screenSize)
        {
            var relativeCutout = new Rect(original.xMin / screenSize.x, original.yMin / screenSize.y, original.width / screenSize.x, original.height / screenSize.y);
            //Debug.Log($"Calc relative {original} {screenSize} {relativeCutout}");
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

#if UNITY_2019_3_OR_NEWER
        static System.Type T = System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        static System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetMainPlayModeViewTargetSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
#else
        static System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        static System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetMainGameViewTargetSizeNoBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
#endif

        private static object[] argsForOut = new object[1];
        internal static Vector2 GetMainGameViewSize()
        {
#if UNITY_2019_3_OR_NEWER
            return (Vector2)GetSizeOfMainGameView.Invoke(null, null);
#else
            System.Object Res = GetSizeOfMainGameView.Invoke(null, argsForOut);
            return (Vector2)argsForOut[0];
#endif
        }

        internal static string devicesPath {
            get {
                var path = EditorPrefs.GetString(NotchSolutionUtility.prefix + "devicesPath");
                return string.IsNullOrEmpty(path) ? "Assets/NotchSolution/Editor/Devices/" : path + "/";
            }
            set { EditorPrefs.SetString(NotchSolutionUtility.prefix + "devicesPath", value.TrimEnd('/', '\\')); }
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

        internal static DeviceInfo selectedDevice
        {
            get { return SimulationDatabase.Get(EditorPrefs.GetString(simulationDeviceKey));}
            set { EditorPrefs.SetString(simulationDeviceKey, value == null ? null: value.Meta.friendlyName); }
        }
    }
}