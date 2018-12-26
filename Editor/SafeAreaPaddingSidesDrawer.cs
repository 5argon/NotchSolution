using UnityEditor;
using UnityEngine;
using System.Linq;

namespace E7.NotchSolution
{
    [CustomPropertyDrawer(typeof(SafeAreaPaddingMode))]
    public class SafeAreaPaddingModeDrawer : EnumButtonsDrawer { }

    [CustomPropertyDrawer(typeof(SafeAreaPaddingOrientationType))]
    public class SafeAreaPaddingOrientationTypeDrawer : EnumButtonsDrawer { }
    
}
