namespace E7.NotchSolution
{
    public enum SafeAreaPaddingMode
    {
        //Pad this side according to safe area.
        Safe,
        //Pad according to safe area, but if the opposite side is larger use that value instead.
        //Used to make a balanced padding to match the opposing notch.
        SafeBalanced,
        // This side's padding is locked to the edge.
        Zero,
        // This side is free to move.
        // Unlocked,
    }
}