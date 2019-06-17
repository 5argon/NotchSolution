using UnityEditor;
using UnityEngine;
using System.Linq;

namespace E7.NotchSolution
{
    [CustomPropertyDrawer(typeof(SupportedOrientations))]
    public class SupportedOrientationsDrawer : EnumButtonsDrawer { }
}
