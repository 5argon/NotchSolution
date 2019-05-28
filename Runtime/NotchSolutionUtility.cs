#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchSolutionUtility
    {
        public const string prefix = nameof(NotchSolution) + "_";

        private  const string simulateSafeAreaRect = prefix + nameof(simulateSafeAreaRect);
        private  const string prefabModeOverlayColor = prefix + nameof(prefabModeOverlayColor);

        private  const string narrowestAspectIndex = prefix + nameof(narrowestAspectIndex);
        private  const string widestAspectIndex = prefix + nameof(widestAspectIndex);

#if UNITY_EDITOR

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
                return new Color(
                    float.Parse(colorStrings[0]),
                    float.Parse(colorStrings[1]),
                    float.Parse(colorStrings[2]),
                    float.Parse(colorStrings[3])
                );
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
        /// This rect is kept in EditorPref so that it survives assembly reload.
        /// </summary>
        public static Rect SimulateSafeAreaRelative
        {
            get
            {
                var rectString = EditorPrefs.GetString(simulateSafeAreaRect, "0;0;1;1");
                var rectStrings = rectString.Split(';');
                return new Rect(
                    float.Parse(rectStrings[0]),
                    float.Parse(rectStrings[1]),
                    float.Parse(rectStrings[2]),
                    float.Parse(rectStrings[3])
                );
            }
            set
            {
                var rectString = string.Join(";", new string[]
                {
                    value.xMin.ToString(),
                    value.yMin.ToString(),
                    value.width.ToString(),
                    value.height.ToString(),
                });
                EditorPrefs.SetString(simulateSafeAreaRect, rectString);
            }
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