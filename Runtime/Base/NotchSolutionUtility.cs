#if UNITY_EDITOR
#endif

using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchSolutionUtility
    {
        public static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }

        /// <summary>
        /// A smart accessor which returns Notch Solution simulated relative safe area in-editor,
        /// hijacked safe area and other device information in 2019.3 with Unity Device Simulator package,
        /// or a real one outside of editor.
        /// </summary>
        public static Rect SafeAreaRelative
        {
            get
            {
#if UNITY_EDITOR
                bool useNotchSimulator = true;
#if UNITY_2019_3_OR_NEWER
                if (UnityDeviceSimulatorActive)
                {
                    //Trust the Unity-modified `Screen.safeArea`, do the same thing as runtime.
                    useNotchSimulator = false;
                }
#endif
                if (useNotchSimulator)
                {
                    // Need the simulator to calculate this for us.
                    return NotchSolutionUtilityEditor.SimulatedSafeAreaRelative;
                }
#endif

                Rect absolutePaddings = Screen.safeArea;
                return new Rect(
                    absolutePaddings.x / Screen.width,
                    absolutePaddings.y / Screen.height,
                    absolutePaddings.width / Screen.width,
                    absolutePaddings.height / Screen.height
                );
            }
        }

#if UNITY_2019_3_OR_NEWER
        static System.Type T = System.Type.GetType("UnityEditor.PlayModeView,UnityEditor");
        static System.Reflection.MethodInfo GetMainPlayModeView = T.GetMethod("GetMainPlayModeView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        /// <summary>
        /// With [Device Simulator](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest) installed it can use a secondary
        /// game view (also a new internal feature in 2019.3) and take control of the screen size and etc. If we detect that active, disable our
        /// own overrides and just go along with Device Simulator simulated value.
        /// </summary>
        private static bool UnityDeviceSimulatorActive
        {
            get
            {
                var mainPlayModeView = GetMainPlayModeView.Invoke(null,null);
                var name = mainPlayModeView.GetType().FullName;
                //I am lazy so I will simply do a class name check with the one in that package.
                return name == "Unity.DeviceSimulator.SimulatorWindow";
            }
        }
#endif
    }
}