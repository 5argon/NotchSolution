using UnityEditor;

namespace E7.NotchSolution
{
    public class AdaptationBaseDrawer : Editor
    {
        /// <summary>
        /// Draw the part of fields in <see cref="AdaptationBase">.
        /// </summary>
        //public static void Draw(SerializedObject serializedObject)
        public override void OnInspectorGUI()
        {
            var supportedProp = serializedObject.FindProperty("supportedOrientations");
            var portraitProp = serializedObject.FindProperty("portraitOrDefaultAdaptation");
            var landscapeProp = serializedObject.FindProperty("landscapeAdaptation");

            (bool landscapeCompatible, bool portraitCompatible) = NotchSolutionUtility.GetOrientationCompatibility();

            if (portraitCompatible && landscapeCompatible)
            {
                EditorGUILayout.PropertyField(supportedProp);
                EditorGUILayout.Separator();
            }

            bool dual = supportedProp.enumValueIndex == (int)SupportedOrientations.Dual;

            if (dual)
            {
                EditorGUILayout.LabelField("Portrait Orientation", EditorStyles.boldLabel);
            }

            portraitProp.Next(enterChildren: true);

            if (portraitCompatible && landscapeCompatible) EditorGUI.indentLevel++;

            for (int i = 0; i < 3; i++)
            {
                EditorGUILayout.PropertyField(portraitProp);
                portraitProp.Next(enterChildren: false);
            }
            
            if (portraitCompatible && landscapeCompatible) EditorGUI.indentLevel--;

            if (dual)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Landscape Orientation", EditorStyles.boldLabel);
                landscapeProp.Next(enterChildren: true);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 3; i++)
                {
                    EditorGUILayout.PropertyField(landscapeProp);
                    landscapeProp.Next(enterChildren: false);
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }


}
