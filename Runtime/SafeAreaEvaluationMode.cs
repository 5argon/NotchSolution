using UnityEngine;

namespace E7.NotchSolution
{
    public enum SafeAreaEvaluationMode
    {
        /// <summary>
        /// Use a value reported from <see cref="Screen.safeArea">.
        /// </summary>
        Safe,

        /// <summary>
        /// Like <see cref="Safe"> but also look at the opposite edge, 
        /// if the value reported from <see cref="Screen.safeArea"> is higher on the other side, assume that value instead.
        /// </summary>
        SafeBalanced,

        /// <summary>
        /// Assume a value 0 regardless of what the device reports from <see cref="Screen.safeArea">.
        /// </summary>
        Zero,
    }
}