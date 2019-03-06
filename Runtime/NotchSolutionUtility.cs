#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchSolutionUtility
    {
        public const string prefix = nameof(NotchSolution) + "_";
        public const string simulateSafeAreaRectKey = prefix + "safeAreaRect";

#if UNITY_EDITOR

        /// <summary>
        /// This rect is kept in EditorPref so that it survives assembly reload.
        /// </summary>
        public static Rect SimulateSafeAreaRelative
        {
            get
            {
                var rectString = EditorPrefs.GetString(simulateSafeAreaRectKey, "0,0,1,1");
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
                EditorPrefs.SetString(simulateSafeAreaRectKey, rectString);
            }
        }

#endif

        public static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }
    }
}