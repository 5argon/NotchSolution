namespace E7.NotchSolution
{
    internal enum SupportedOrientations
    {
        /// <summary>
        ///     The game is fixed on only portrait or landscape.
        ///     Device rotation maybe possible to rotate 180 degree to the opposite side.
        /// </summary>
        Single,

        /// <summary>
        ///     It is possible to rotate the screen between portrait and landscape. (90 degree rotation is possible)
        /// </summary>
        Dual,
    }
}