using UnityEngine;

namespace E7.NotchSolution
{
    public interface INotchSimulatorTarget
    {
        void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative);
    }
}