using UnityEditor;
using UnityEngine;
using System.Linq;

namespace E7.NotchSolution
{
    public class EnumButtonsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var actualLabel = EditorGUI.BeginProperty(position, null, property);
            var insideRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), actualLabel);

            var contents = property.enumDisplayNames.Select(x => new GUIContent(x)).ToArray();

            property.enumValueIndex = GUI.Toolbar(insideRect, property.enumValueIndex, contents, EditorStyles.miniButton, GUI.ToolbarButtonSize.Fixed);
            EditorGUI.EndProperty();
        }
    }

    
}
