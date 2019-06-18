#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchSolutionUtility
    {
        public const string prefix = nameof(NotchSolution) + "_";

        private const string simulateSafeAreaRect = prefix + nameof(simulateSafeAreaRect);
        private const string simulateCutoutsRect = prefix + nameof(simulateCutoutsRect);
        private const string prefabModeOverlayColor = prefix + nameof(prefabModeOverlayColor);

        private const string narrowestAspectIndex = prefix + nameof(narrowestAspectIndex);
        private const string widestAspectIndex = prefix + nameof(widestAspectIndex);

        /// <summary>
        /// A smart accessor which returns simulated relative safe area in-editor, or a real one outside of editor.
        /// </summary>
        public static Rect SafeAreaRelative
        {
            get
            {
#if UNITY_EDITOR
                // Need the simulator to calculate this for us.
                return NotchSolutionUtility.SimulatedSafeAreaRelative;
#else
                Rect absolutePaddings = Screen.safeArea;
                return new Rect(
                    absolutePaddings.x / Screen.width,
                    absolutePaddings.y / Screen.height,
                    absolutePaddings.width / Screen.width,
                    absolutePaddings.height / Screen.height
                );
#endif
            }
        }

#if UNITY_EDITOR

        public static (bool landscapeCompatible, bool portraitCompatible) GetOrientationCompatibility()
        {
            bool landscapeCompatible = false;
            bool portraitCompatible = false;
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
                    if (PlayerSettings.allowedAutorotateToLandscapeLeft) landscapeCompatible = true;
                    if (PlayerSettings.allowedAutorotateToLandscapeRight) landscapeCompatible = true;
                    if (PlayerSettings.allowedAutorotateToPortrait) portraitCompatible = true;
                    if (PlayerSettings.allowedAutorotateToPortraitUpsideDown) portraitCompatible = true;
                    break;
            }
            return (landscapeCompatible, portraitCompatible);
        }

        public static int NarrowestAspectIndex
        {
            get => EditorPrefs.GetInt(narrowestAspectIndex, 0);
            set => EditorPrefs.SetInt(narrowestAspectIndex, Mathf.Max(0, value));
        }

        public static int WidestAspectIndex
        {
            get => EditorPrefs.GetInt(widestAspectIndex, 0);
            set => EditorPrefs.SetInt(widestAspectIndex, Mathf.Max(0, value));
        }

        public static Color PrefabModeOverlayColor
        {
            get
            {
                var colorString = EditorPrefs.GetString(prefabModeOverlayColor, "0.297;0.405;0.481;1");
                var colorStrings = colorString.Split(';');
                Color color = new Color(0.297F, 0.405F, 0.481F, 1);
                for (int i = 0; i < 4; i++)
                {
                    if (float.TryParse(colorStrings[i], out float rgba)) color[i] = rgba;
                    else return new Color(0.297F, 0.405F, 0.481F, 1);
                }
                return color;
            }
            set
            {
                var colorString = string.Join(";", new string[]
                {
                    value.r.ToString(),
                    value.g.ToString(),
                    value.b.ToString(),
                    value.a.ToString(),
                });
                EditorPrefs.SetString(prefabModeOverlayColor, colorString);
            }
        }

        /// <summary>
        /// Calculated and stored by the notch simulator.
        /// This rect is kept in <see cref="EditorPrefs"> so that it survives assembly reload.
        /// 
        /// This doesn't exist outside of editor. To get a relative safe area depending if on the real device or in editor, 
        /// use <see cref="SafeAreaRelative"> instead.
        /// </summary>
        public static Rect SimulatedSafeAreaRelative
        {
            get { return ParseRect(EditorPrefs.GetString(simulateSafeAreaRect, "0;0;1;1")); }
            set { EditorPrefs.SetString(simulateSafeAreaRect, RectToString(value)); }
        }

#if UNITY_2019_2_OR_NEWER
        /// <summary>
        /// Calculated and stored by the notch simulator.
        /// This rect is kept in <see cref="EditorPrefs"> so that it survives assembly reload.
        /// 
        /// This doesn't exist outside of editor. 
        /// </summary>
        public static Rect[] SimulatedCutoutsRelative
        {
            get
            {
                var rectStrings = EditorPrefs.GetString(simulateCutoutsRect, "").Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                System.Collections.Generic.List<Rect> rects = new System.Collections.Generic.List<Rect>();
                foreach (var rectString in rectStrings) rects.Add(ParseRect(rectString));
                return rects.ToArray();
            }
            set
            {
                System.Collections.Generic.List<string> rectStrings = new System.Collections.Generic.List<string>();
                foreach (Rect rect in value) rectStrings.Add(RectToString(rect));
                EditorPrefs.SetString(simulateCutoutsRect, string.Join("\n", rectStrings));
            }
        }
#endif

        static Rect ParseRect(string rectString)
        {
            var rect = rectString.Split(';');
            return new Rect(
                        float.Parse(rect[0]),
                        float.Parse(rect[1]),
                        float.Parse(rect[2]),
                        float.Parse(rect[3])
                    );
        }

        static string RectToString(Rect rect)
        {
            return string.Join(";", new string[]
                    {
                    rect.xMin.ToString(),
                    rect.yMin.ToString(),
                    rect.width.ToString(),
                    rect.height.ToString(),
                    });
        }

        /// <summary>
        /// Integer aspect and various round off error may make the number hideous, we could find a similar one for display purpose.
        /// </summary>
        public static Vector2 CommonAspectLookup(Vector2 aspect)
        {
            float ratio = aspect.x / aspect.y;
            Vector2[] commonRatio = new Vector2[]
            {
                new Vector2(4,3),
                new Vector2(16,9),
                new Vector2(17,9),
                new Vector2(18,9),
                new Vector2(18.5f,9),
                new Vector2(18.7f,9),
                new Vector2(19,9),
                new Vector2(19.3f,9),
                new Vector2(19.5f,9),
                new Vector2(19,10),
                new Vector2(21,9),
                new Vector2(2,1),
            };
            foreach (var r in commonRatio)
            {
                var diff = Mathf.Abs(ratio - (r.x / r.y));
                if (diff < 0.001f)
                {
                    return r;
                }
                diff = Mathf.Abs(ratio - (r.y / r.x));
                if (diff < 0.001f)
                {
                    return new Vector2(r.y, r.x);
                }
            }
            return aspect;
        }

        public static Vector2 ScreenRatio(Vector2 screen)
        {
            int a = (int)screen.x;
            int b = (int)screen.y;

            int GCD(int A, int B) => B == 0 ? A : GCD(B, A % B);
            int gcd = GCD(a, b);
            var integerRatio = new Vector2(screen.x / gcd, screen.y / gcd);
            return CommonAspectLookup(integerRatio);
        }



#endif

        public static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }
    }
}