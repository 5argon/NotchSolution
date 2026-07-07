using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace E7.NotchSolution
{
    /// <summary>
    ///     Helper methods for Notch Solution's components.
    /// </summary>
    /// <remarks>
    ///     Safe area and cutouts are read through <see cref="UnityEngine.Device.Screen"/>.
    /// </remarks>
    public static class NotchSolutionUtility
    {
        internal static Rect defaultSafeArea = new Rect(0, 0, 1, 1);
        internal static Rect[] defaultCutouts = new Rect[0];

        /// <summary>
        ///     <see cref="UnityEngine.Device.Screen.safeArea"/> expressed in 0~1 values relative to the
        ///     current screen size.
        /// </summary>
        internal static Rect ScreenSafeAreaRelative => ToScreenRelativeRect(Screen.safeArea);

        /// <summary>
        ///     <see cref="UnityEngine.Device.Screen.cutouts"/> expressed in 0~1 values relative to the
        ///     current screen size.
        /// </summary>
        internal static Rect[] ScreenCutoutsRelative
        {
            get
            {
                var absoluteCutouts = Screen.cutouts;
                var relative = new Rect[absoluteCutouts.Length];
                for (var i = 0; i < absoluteCutouts.Length; i++)
                {
                    relative[i] = ToScreenRelativeRect(absoluteCutouts[i]);
                }

                return relative;
            }
        }

        internal static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        }

        /// <summary>
        ///     Converts an absolute pixel rect into a 0~1 rect relative to the current screen size.
        /// </summary>
        private static Rect ToScreenRelativeRect(Rect absoluteRect)
        {
            float w = Screen.width;
            float h = Screen.height;
            return new Rect(
                absoluteRect.x / w,
                absoluteRect.y / h,
                absoluteRect.width / w,
                absoluteRect.height / h
            );
        }
    }
}
