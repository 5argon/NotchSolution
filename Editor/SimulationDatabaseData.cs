using UnityEngine;

namespace E7.NotchSolution
{
    public class SimulationDatabaseData
    {
        public Rect portraitSafeArea;
        public Rect landscapeSafeArea;

        /// <summary>
        /// Optional, if no cutouts just left it as `null`.
        /// </summary>
        public Rect[] portraitCutouts;

        /// <summary>
        /// Optional, if no cutouts just left it as `null`.
        /// </summary>
        public Rect[] landscapeCutouts;

        /// <summary>
        /// Width x height of the portrait orientation.
        /// </summary>
        public Vector2 screenSize;
    }
}