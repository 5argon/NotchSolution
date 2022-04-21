using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    internal static class NotchSimulatorUtility
    {
        internal static Rect CalculateSimulatorSafeAreaRelative(SimulationDevice device)
        {
            var firstScreen = device.Screens.FirstOrDefault();
            // Should not happen, but just to be safe.
            if (firstScreen == null)
            {
                return default;
            }

            var orientation = GetGameViewOrientation();
            var safe = firstScreen.orientations[orientation].safeArea;
            var screenSize = new Vector2(firstScreen.width, firstScreen.height);
            if (orientation == ScreenOrientation.LandscapeLeft)
            {
                var swap = screenSize.x;
                screenSize.x = screenSize.y;
                screenSize.y = swap;
            }

            return GetRectRelativeToScreenSize(safe, screenSize);
        }

        internal static Rect[] CalculateSimulatorCutoutsRelative(SimulationDevice device)
        {
            var firstScreen = device.Screens.FirstOrDefault();
            // Should not happen, but just to be safe.
            if (firstScreen == null)
            {
                return default;
            }

            var orientation = GetGameViewOrientation();
            var cutouts = firstScreen.orientations[orientation].cutouts;
            if (cutouts is null)
            {
                return new Rect[0];
            }

            var screenSize = new Vector2(firstScreen.width, firstScreen.height);
            if (orientation == ScreenOrientation.LandscapeLeft)
            {
                var swap = screenSize.x;
                screenSize.x = screenSize.y;
                screenSize.y = swap;
            }

            var rects = new List<Rect>();
            foreach (var cutout in cutouts)
            {
                rects.Add(GetRectRelativeToScreenSize(cutout, screenSize));
            }

            return rects.ToArray();
        }

        /// <param name="original">Must be inside of a rect positioned at 0,0 that has width and height as <paramref name="screenSize"></param>
        private static Rect GetRectRelativeToScreenSize(Rect original, Vector2 screenSize)
        {
            var relativeCutout = new Rect(original.xMin / screenSize.x, original.yMin / screenSize.y,
                original.width / screenSize.x, original.height / screenSize.y);
            //Debug.Log($"Calc relative {original} {screenSize} {relativeCutout}");
            if (Settings.Instance.FlipOrientation)
            {
                return new Rect(
                    1 - (relativeCutout.width + relativeCutout.xMin),
                    1 - (relativeCutout.height + relativeCutout.yMin),
                    relativeCutout.width,
                    relativeCutout.height
                );
            }

            return relativeCutout;
        }

        internal static ScreenOrientation GetGameViewOrientation()
        {
            var gameViewSize = GetMainGameViewSize();
            return gameViewSize.x > gameViewSize.y ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        }

#if UNITY_2019_3_OR_NEWER
        private static readonly Type T = Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        private static readonly MethodInfo GetSizeOfMainGameView =
            T.GetMethod("GetMainPlayModeViewTargetSize", BindingFlags.NonPublic | BindingFlags.Static);
#else
        static System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        static System.Reflection.MethodInfo GetSizeOfMainGameView =
 T.GetMethod("GetMainGameViewTargetSizeNoBox", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
#endif

        private static object[] argsForOut = new object[1];

        internal static Vector2 GetMainGameViewSize()
        {
#if UNITY_2019_3_OR_NEWER
            var result = (Vector2) GetSizeOfMainGameView.Invoke(null, null);
            if (result == new Vector2(640f, 480f) && !NotchSolutionUtilityEditor.PlayModeViewOpen)
            {
                // Neither Game window nor Unity Device Simulator window is open (can happen if Scene view is maximized)
                // In this case, the last open game window's size can be determined by looking at the mockup canvas' size
                foreach (var mockupCanvas in NotchSimulator.AllMockupCanvases)
                {
                    result = ((RectTransform) mockupCanvas.GetComponent<Canvas>().transform).sizeDelta;
                    break;
                }
            }

            return result;
#else
            System.Object Res = GetSizeOfMainGameView.Invoke(null, argsForOut);
            return (Vector2)argsForOut[0];
#endif
        }

        internal static string devicesFolderCached;

        internal static string DevicesFolder
        {
            get
            {
                if (string.IsNullOrEmpty(devicesFolderCached))
                {
                    devicesFolderCached = AssetDatabase.GUIDToAssetPath("e6ee4d37d64882c4fac2dc0b3aad29cb");
                }

                return devicesFolderCached;
            }
        }
    }
}