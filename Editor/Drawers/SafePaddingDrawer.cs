using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    [CustomEditor(typeof(SafePadding))]
    internal class SafePaddingDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var orientationType = serializedObject.FindProperty("orientationType");

            var portrait = serializedObject.FindProperty("portraitOrDefaultPaddings");
            var landscape = serializedObject.FindProperty("landscapePaddings");
            var influence = serializedObject.FindProperty("influence");
            var flipPadding = serializedObject.FindProperty("flipPadding");

            var (landscapeCompatible, portraitCompatible) =
                NotchSolutionUtilityEditor.GetOrientationCompatibility();

            if (portraitCompatible && landscapeCompatible)
            {
                EditorGUILayout.PropertyField(orientationType);
                EditorGUILayout.Separator();
            }

            var dual = orientationType.enumValueIndex == (int) SupportedOrientations.Dual;

            if (dual)
            {
                EditorGUILayout.LabelField("Portrait Orientation", EditorStyles.boldLabel);
            }

            if (portraitCompatible && landscapeCompatible)
            {
                EditorGUI.indentLevel++;
            }

            for (var i = 0; i < 4; i++)
            {
                portrait.Next(true);
                EditorGUILayout.PropertyField(portrait, new GUIContent(portrait.displayName));
            }

            if (portraitCompatible && landscapeCompatible)
            {
                EditorGUI.indentLevel--;
            }

            if (dual)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Landscape Orientation", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (var i = 0; i < 4; i++)
                {
                    landscape.Next(true);
                    EditorGUILayout.PropertyField(landscape);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(influence);
            EditorGUILayout.PropertyField(flipPadding);

            serializedObject.ApplyModifiedProperties();
        }
    }
}