namespace E7.NotchSolution
{
    /// <summary>
    ///     How a component looks at a particular edge to take the edge's property.
    ///     Meaning depends on context of that component.
    /// </summary>
    internal enum EdgeEvaluationMode
    {
        /// <summary>
        ///     Use a value reported from that edge.
        /// </summary>
        On,

        /// <summary>
        ///     Like <see cref="On"/> but also look at the opposite edge,
        ///     if the value reported is higher on the other side, assume that value instead.
        /// </summary>
        Balanced,

        /// <summary>
        ///     Do not use a value reported from that edge.
        /// </summary>
        Off,
    }
}