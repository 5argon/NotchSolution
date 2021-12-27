using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("E7.NotchSolution.Editor")]

// This is a special exception since that scene is not really a true sample.
// (If it is, you can "do it too" without hacking internals.)
[assembly: InternalsVisibleTo("E7.NotchSolution.Samples.DebugScene")]