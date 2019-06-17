using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace E7.NotchSolution
{
    [CustomEditor(typeof(SafeAreaAdaptation))]
    public class SafeAreaAdaptationDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            var adaptToEdgeProp = serializedObject.FindProperty("adaptToEdge");
            var evalModeProp = serializedObject.FindProperty("evaluationMode");
            var supportedProp = serializedObject.FindProperty("supportedOrientations");
            var portraitProp = serializedObject.FindProperty("portraitOrDefaultAdaptation");
            var landscapeProp = serializedObject.FindProperty("landscapeAdaptation");

            var rect = EditorGUILayout.GetControlRect(hasLabel:true);
            EditorGUI.BeginProperty(rect, new GUIContent(adaptToEdgeProp.displayName), adaptToEdgeProp);
            EnumButtonsDrawer.DrawEnumAsButtons(rect, adaptToEdgeProp);
            EditorGUI.EndProperty();
            EditorGUILayout.PropertyField(evalModeProp);

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
            EditorGUI.indentLevel++;
            for(int i = 0; i < 3 ;i++)
            {
                EditorGUILayout.PropertyField(portraitProp);
                portraitProp.Next(enterChildren: false);
            }
            EditorGUI.indentLevel--;

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

            //WIP evaluated value debugger

            // var p = serializedObject.FindProperty("portraitOrDefaultAdaptation").FindPropertyRelative("adaptationCurve");
            // var p2 = serializedObject.FindProperty("landscapeAdaptation").FindPropertyRelative("adaptationCurve");

            // var latest = Selection.activeGameObject.GetComponent<SafeAreaAdaptation>().latestSimulatedSpaceTakenRelative;
            // var minValue = p.animationCurveValue.keys[0].value;
            // var maxValue = p.animationCurveValue.keys[p.animationCurveValue.length - 1].value;

            // var r = EditorGUILayout.GetControlRect(hasLabel: true, EditorGUIUtility.singleLineHeight * 3);

            // EditorGUIUtility.DrawCurveSwatch(r,AnimationCurve.EaseInOut(0, 0, 1, 2), p, Color.green, Color.gray);
            // Handles.color = Color.red;
            // Handles.DrawAAPolyLine(new Vector3(0,0,0), new Vector3(100,100,0));
            // Handles.color = Color.yellow;
            // Handles.ArrowHandleCap(0, new Vector3(50,50,0), Random.rotation, 3, EventType.DragPerform);

            serializedObject.ApplyModifiedProperties();



        }
    }


}
