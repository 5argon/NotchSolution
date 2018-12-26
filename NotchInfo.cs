using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchInfo
    {
        public static int NotchSign
        {
            get
            {
#if UNITY_EDITOR
                switch (NotchSolutionUtility.simulateScreenOrientation)
#else
                switch (Screen.orientation)
#endif
                {
                    case ScreenOrientation.Portrait: return -1;
                    case ScreenOrientation.PortraitUpsideDown: return 1;
                    case ScreenOrientation.LandscapeLeft: return 1;
                    case ScreenOrientation.LandscapeRight: return -1;
                    default: return 1;
                }
            }
        }

        /// <summary>
        /// If supporting safe area use the  https://developer.huawei.com/consumer/en/devservice/doc/30210
        /// </summary>
        public static int NotchSize
        {
            get
            {
                if (!HasNotch) return 0;
                return 90;
            }
        }

        /// <summary>
        /// For iOS and Android 9.0.0 we can determine for sure.
        /// For Android lower than Pie we consult a database.
        /// </summary>
        public static bool HasNotch
        {
            get
            {
                return true;
            }
        }
    }
}