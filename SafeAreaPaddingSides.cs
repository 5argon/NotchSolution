using UnityEngine;
using System;

namespace E7.NotchSolution
{
    [Serializable]
    public class SafeAreaPaddingSides
    {
        [SerializeField] public SafeAreaPaddingMode left;
        [SerializeField] public SafeAreaPaddingMode bottom;
        [SerializeField] public SafeAreaPaddingMode top;
        [SerializeField] public SafeAreaPaddingMode right;
    }
}