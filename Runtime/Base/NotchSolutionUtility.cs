using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    ///     Helper methods for Notch Solution's components.
    /// </summary>
    public static class NotchSolutionUtility
    {
        internal static Rect defaultSafeArea = new Rect(0, 0, 1, 1);
        internal static Rect[] defaultCutouts = new Rect[0];

        internal static Rect cachedScreenSafeArea;
        internal static Rect cachedScreenSafeAreaRelative;
        internal static bool safeAreaRelativeCached;

        internal static Rect[] cachedScreenCutouts;
        internal static Rect[] cachedScreenCutoutsRelative;
        internal static bool cutoutsRelativeCached;

        /// <summary>
        ///     You can use it in any of your component that uses <see cref="INotchSimulatorTarget"/>.
        ///     <list type="bullet">
        ///         <item>
        ///             If <c>true</c>, should trust the values sent to <see cref="INotchSimulatorTarget"/>.
        ///         </item>
        ///         <item>
        ///             If <c>false</c>, should trust <see cref="Screen"/> API because we detected something like
        ///             [Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/) is
        ///             controlling the <see cref="Screen"/> value and it is now useful in editor.
        ///         </item>
        ///     </list>
        /// </summary>
        public static bool ShouldUseNotchSimulatorValue
        {
            get
            {
#if UNITY_EDITOR
                if (NotchSolutionUtilityEditor.UnityDeviceSimulatorActive)
                {
                    return false;
                }

                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        ///     Calculated from <see cref="Screen"/> API without caring about simulated value.
        ///     Note that 2019.3 Unity Device Simulator can mock the <see cref="Screen"/> so this is not
        ///     necessary real in editor.
        /// </summary>
        // TODO : Cache potential, but many pitfalls awaits so I have not done it.
        // - Some first frames (1~3) Unity didn't return a rect that take account of safe area for some reason. If we cache that then we failed.
        // - Orientation change requries clearing the cache again. Manually or automatically? How?
        internal static Rect ScreenSafeAreaRelative
        {
            get
            {
                var absolutePaddings = Screen.safeArea;
                cachedScreenSafeAreaRelative = ToScreenRelativeRect(absolutePaddings);
                cachedScreenSafeArea = absolutePaddings;
                safeAreaRelativeCached = true;
                return cachedScreenSafeAreaRelative;
            }
        }

        /// <summary>
        ///     Calculated from <see cref="Screen"/> API without caring about simulated value.
        ///     Note that 2019.3 Unity Device Simulator can mock the <see cref="Screen"/> so this is not
        ///     necessary real in editor.
        /// </summary>
        // TODO : Cache potential, but many pitfalls awaits so I have not done it.
        // - Some first frames (1~3) Unity didn't return a rect that take account of safe area for some reason.
        // If we cache that then we failed.
        // - Orientation change requires clearing the cache again. Manually or automatically? How?
        internal static Rect[] ScreenCutoutsRelative
        {
            get
            {
                var absoluteCutouts = Screen.cutouts;

                cachedScreenCutoutsRelative = new Rect[absoluteCutouts.Length];
                for (var i = 0; i < absoluteCutouts.Length; i++)
                {
                    cachedScreenCutoutsRelative[i] = ToScreenRelativeRect(absoluteCutouts[i]);
                }

                cachedScreenCutouts = absoluteCutouts;
                cutoutsRelativeCached = true;
                return cachedScreenCutoutsRelative;
            }
        }

        internal static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait;
        }

        private static Rect ToScreenRelativeRect(Rect absoluteRect)
        {
#if UNITY_STANDALONE
            var w = absoluteRect.width;
            var h = absoluteRect.height;
#else
            int w = Screen.currentResolution.width;
            int h = Screen.currentResolution.height;
#endif
            //Debug.Log($"{w} {h} {Screen.currentResolution} {absoluteRect}");
            return new Rect(
                absoluteRect.x / w,
                absoluteRect.y / h,
                absoluteRect.width / w,
                absoluteRect.height / h
            );
        }
    }
}