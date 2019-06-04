using UnityEditor;

namespace E7.NotchSolution
{
    [CustomEditor(typeof(SafeAreaPadding))]
    public class SafeAreaPaddingDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            var orientationType = serializedObject.FindProperty("orientationType");

            var portrait = serializedObject.FindProperty("portraitOrDefaultPaddings");
            var landscape = serializedObject.FindProperty("landscapePaddings");
            var influence = serializedObject.FindProperty("influence");

            bool landscapeCompatible = false;
            bool portraitCompatible = false;

            switch (PlayerSettings.defaultInterfaceOrientation)
            {
                case UIOrientation.LandscapeLeft:
                case UIOrientation.LandscapeRight:
                    landscapeCompatible = true;
                    break;
                case UIOrientation.Portrait:
                case UIOrientation.PortraitUpsideDown:
                    portraitCompatible = true;
                    break;
                case UIOrientation.AutoRotation:
                    if (PlayerSettings.allowedAutorotateToLandscapeLeft) landscapeCompatible = true;
                    if (PlayerSettings.allowedAutorotateToLandscapeRight) landscapeCompatible = true;
                    if (PlayerSettings.allowedAutorotateToPortrait) portraitCompatible = true;
                    if (PlayerSettings.allowedAutorotateToPortraitUpsideDown) portraitCompatible = true;
                    break;
            }

            if (portraitCompatible && landscapeCompatible)
            {
                EditorGUILayout.PropertyField(orientationType);
                EditorGUILayout.Separator();
            }

            bool dual = orientationType.enumValueIndex == (int)SafeAreaPaddingOrientationType.DualOrientation;

            if (dual)
            {
                EditorGUILayout.LabelField("Portrait Orientation", EditorStyles.boldLabel);
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
            {
                portrait.Next(enterChildren: true);
                EditorGUILayout.PropertyField(portrait);
            }
            EditorGUI.indentLevel--;

            if (dual)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("Landscape Orientation", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                {
                    landscape.Next(enterChildren: true);
                    EditorGUILayout.PropertyField(landscape);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(influence);

            serializedObject.ApplyModifiedProperties();
        }
    }


}
