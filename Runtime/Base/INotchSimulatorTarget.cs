using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    /// Put it on any component to make it receive simulation calls from the Notch Simulator.
    /// Because in editor <see cref="Screen"/> API is useless, instead we could receive some
    /// simulated values depending on what we have selected.
    /// </summary>
    /// <remarks>
    /// After 2019.3 finally there is an API that <see cref="Screen"/> or <see cref="SystemInfo"/> could
    /// be overridden. It is a base for Unity's own [Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/).
    /// </remarks>
    public interface INotchSimulatorTarget
    {
        /// <summary>
        /// Called frequently while the Notch Simulator is running.
        /// </summary>
        /// <remarks>
        /// If you have [Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/), it is **still called**.
        /// Your script using this should decide if you want to stop using these values when you can now trust the <see cref="Screen"/>
        /// or <see cref="SystemInfo"/> that is now modified. (You may use this helper : <see cref="NotchSolutionUtility.ShouldUseNotchSimulatorValue"/>)
        /// </remarks>
        /// <param name="simulatedSafeAreaRelative">Each values in the rect is 0~1, relative to the current screen size of active simulation device.</param>
        /// <param name="simulatedCutoutsRelative">
        /// Each values in the rect is 0~1, relative to the current screen size of active simulation device.
        /// You can always get this even if you are running 2019.1 where cutout API is not available. The simulated cutouts are always available in editor.
        /// </param>
        void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative);
    }
}