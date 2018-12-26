using UnityEngine;

namespace E7.NotchSolution
{
    public static class NotchSolutionUtility
    {
        public static ScreenOrientation GetCurrentOrientation()
        {
            return Screen.width > Screen.height ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
        }
    }
}