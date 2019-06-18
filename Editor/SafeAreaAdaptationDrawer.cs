using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace E7.NotchSolution
{
    [CustomEditor(typeof(AspectRatioAdaptation))]
    public class AspectRatioAdaptationDrawer : AdaptationBaseDrawer { }

    [CustomEditor(typeof(SafeAreaAdaptation))]
    public class SafeAreaAdaptationDrawer : AdaptationBaseDrawer
    {
        public override void OnInspectorGUI()
        {
            var adaptToEdgeProp = serializedObject.FindProperty("adaptToEdge");
            var evalModeProp = serializedObject.FindProperty("evaluationMode");

            var rect = EditorGUILayout.GetControlRect(hasLabel:true);
            EditorGUI.BeginProperty(rect, new GUIContent(adaptToEdgeProp.displayName), adaptToEdgeProp);
            EnumButtonsDrawer.DrawEnumAsButtons(rect, adaptToEdgeProp);
            EditorGUI.EndProperty();
            EditorGUILayout.PropertyField(evalModeProp);

            base.OnInspectorGUI();

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

            //serializedObject.ApplyModifiedProperties();
        }
    }


}
