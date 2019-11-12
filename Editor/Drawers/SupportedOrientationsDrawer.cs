using UnityEditor;
using UnityEngine;
using System.Linq;

namespace E7.NotchSolution.Editor
{
    [CustomPropertyDrawer(typeof(SupportedOrientations))]
    internal class SupportedOrientationsDrawer : EnumButtonsDrawer { }
}
