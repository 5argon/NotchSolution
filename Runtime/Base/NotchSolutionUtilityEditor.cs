using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.NotchSolution
{
    /// <summary>
    ///     Editor only utility but in the runtime assembly, since some runtime stuff has editor-only
    ///     code and it cannot reference editor assembly or we would have circular dependency.
    /// </summary>
    internal static class NotchSolutionUtilityEditor
    {
#if UNITY_EDITOR
        internal static MethodInfo GetMainPlayModeView = Type.GetType("UnityEditor.PlayModeView,UnityEditor")
            .GetMethod("GetMainPlayModeView", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        ///     Returns true if Game window, Unity Device Simulator window or any other PlayModeView window is open in the Editor.
        /// </summary>
        internal static bool PlayModeViewOpen => GetMainPlayModeView.Invoke(null, null) as EditorWindow;

        /// <summary>
        ///     With [Device Simulator](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest) installed
        ///     it can use a secondary game view (also a new internal feature in 2019.3) and take control of
        ///     the screen size and etc. If we detect that active, disable our own overrides
        ///     and just go along with Device Simulator simulated value.
        /// </summary>
        internal static bool UnityDeviceSimulatorActive
        {
            get
            {
                if (!PlayModeViewOpen)
                {
                    return false;
                }

                var mainPlayModeView = GetMainPlayModeView.Invoke(null, null);
                var name = mainPlayModeView.GetType().FullName;
                //I am lazy so I will simply do a class name check with the one in that package.
                return Regex.IsMatch(name, "Unity(Editor).DeviceSimulat(ion|or).SimulatorWindow");
            }
        }

        internal static (bool landscapeCompatible, bool portraitCompatible) GetOrientationCompatibility()
        {
            var landscapeCompatible = false;
            var portraitCompatible = false;
            switch (PlayerSettings.defaultInterfaceOrientation)
            {
                case UIOrientation.LandscapeLeft:
                case UIOrientation.LandscapeRight:
                    landscapeCompatible = true;
                    break;
                case UIOrientation.Portrait:
                case UIOrientation.PortraitUpsideDown:
                    portraitCompatible = true;
                    break;
                case UIOrientation.AutoRotation:
                    if (PlayerSettings.allowedAutorotateToLandscapeLeft)
                    {
                        landscapeCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToLandscapeRight)
                    {
                        landscapeCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToPortrait)
                    {
                        portraitCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToPortraitUpsideDown)
                    {
                        portraitCompatible = true;
                    }

                    break;
            }

            return (landscapeCompatible, portraitCompatible);
        }

        private static readonly Vector2[] commonRatio =
        {
            new Vector2(4, 3),
            new Vector2(16, 9),
            new Vector2(17, 9),
            new Vector2(18, 9),
            new Vector2(18.5f, 9),
            new Vector2(18.7f, 9),
            new Vector2(19, 9),
            new Vector2(19.3f, 9),
            new Vector2(19.5f, 9),
            new Vector2(19, 10),
            new Vector2(21, 9),
            new Vector2(2, 1),
        };

        /// <summary>
        ///     Integer aspect and various round off error may make the number hideous,
        ///     we could find a similar one for display purpose.
        /// </summary>
        private static Vector2 CommonAspectLookup(Vector2 aspect)
        {
            var ratio = aspect.x / aspect.y;
            foreach (var r in commonRatio)
            {
                var diff = Mathf.Abs(ratio - r.x / r.y);
                if (diff < 0.001f)
                {
                    return r;
                }

                diff = Mathf.Abs(ratio - r.y / r.x);
                if (diff < 0.001f)
                {
                    return new Vector2(r.y, r.x);
                }
            }

            return aspect;
        }

        internal static Vector2 ScreenRatio(Vector2 screen)
        {
            var a = (int) screen.x;
            var b = (int) screen.y;

            int GCD(int A, int B)
            {
                return B == 0 ? A : GCD(B, A % B);
            }

            var gcd = GCD(a, b);
            var integerRatio = new Vector2(screen.x / gcd, screen.y / gcd);
            return CommonAspectLookup(integerRatio);
        }
#endif
    }
}