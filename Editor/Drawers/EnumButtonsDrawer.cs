using System.Linq;
using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    internal class EnumButtonsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawEnumAsButtons(position, property);
        }

        public static void DrawEnumAsButtons(Rect position, SerializedProperty property)
        {
            var actualLabel = EditorGUI.BeginProperty(position, null, property);
            var insideRect = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), actualLabel);

            var contents = property.enumDisplayNames.Select(x => new GUIContent(x)).ToArray();

            property.enumValueIndex = GUI.Toolbar(insideRect, property.enumValueIndex, contents,
                EditorStyles.miniButton, GUI.ToolbarButtonSize.Fixed);
            EditorGUI.EndProperty();
        }

        /* 

        public static VisualElement CreateToolbar(SerializedProperty property)
        {
            var contents = property.enumDisplayNames.Select(x => new GUIContent(x)).ToArray();

            var toolbarRoot = new VisualElement();
            toolbarRoot.AddToClassList("unity-base-field");

            var toolbar = new Toolbar();
            var label = new Label(property.displayName);
            label.AddToClassList("unity-base-field__label");

            toolbarRoot.Add(label);
            toolbarRoot.Add(toolbar);

            for (int i = 0; i < contents.Length; i++)
            {
                var keep = i;
                var tb = new ToolbarToggle();
                tb.value = property.enumValueIndex == keep;
                tb.RegisterValueChangedCallback((evc) =>
                {
                    property.enumValueIndex = keep;
                    property.serializedObject.ApplyModifiedProperties();
                    foreach (var t in toolbar.Query<ToolbarToggle>().ToList())
                    {
                        if (t != tb)
                        {
                            t.SetValueWithoutNotify(false);
                        }
                        else
                        {
                            t.SetValueWithoutNotify(true);
                        }
                    }
                });

                tb.text = contents[i].text;
                toolbar.Add(tb);
            }
            return toolbarRoot;
        }
        */
    }
}