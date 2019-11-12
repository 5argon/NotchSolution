using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    internal class AdaptationBaseDrawer : UnityEditor.Editor
    {
        private void DrawGenButton(AdaptationBase ab, bool forPortrait)
        {
            var animator = ab.GetComponent<Animator>();
            if (ab.TryGetLinkedControllerAsset(forPortrait, out var controllerAsset))
            {
                bool controllerNull = animator.runtimeAnimatorController == null || animator.runtimeAnimatorController != controllerAsset;

                var miniMod = new GUIStyle(EditorStyles.miniButton);

                GUI.backgroundColor = !controllerNull ? Color.red : Color.white;
                string toggleText = !controllerNull ? "Editing Adaptation..." : "Edit Adaptation";

                bool newToggle = GUILayout.Toggle(
                    value: !controllerNull,
                    content: new GUIContent(toggleText, "Toggle on to assign an animator controller asset temporarily, so you could edit its clips in the Animation panel."),
                    EditorStyles.miniButton
                );

                GUI.backgroundColor = Color.white;

                if (newToggle == controllerNull)
                {
                    animator.runtimeAnimatorController = controllerNull ? controllerAsset : null;
                    EditorApplication.RepaintAnimationWindow();
                }
            }
            else
            {
                if (ab.IsAdaptable(forPortrait) == false)
                {
                    if (GUILayout.Button("Generate Adaptation Assets", EditorStyles.miniButton))
                    {
                        CreateAnimator(forPortrait);
                    }
                }
            }
        }

        void CreateAnimator(bool forPortrait)
        {
            AdaptationBase adaptation = (AdaptationBase)target;
            Animator animator = adaptation.gameObject.GetComponent<Animator>();

            var incompletePath = GetSaveControllerPath(adaptation.gameObject);
            if (string.IsNullOrEmpty(incompletePath)) return;
            var prefix = Path.GetFileNameWithoutExtension(incompletePath);
            var path = $"{Path.GetDirectoryName(incompletePath)}{Path.DirectorySeparatorChar}{prefix}Adaptation.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AssetDatabase.ImportAsset(path);


            animator.runtimeAnimatorController = controller;

            var normalStateClip = AnimatorController.AllocateAnimatorClip($"{prefix}NormalState");
            var adaptedStateClip = AnimatorController.AllocateAnimatorClip($"{prefix}AdaptedState");

            AssetDatabase.AddObjectToAsset(normalStateClip, controller);
            AssetDatabase.AddObjectToAsset(adaptedStateClip, controller);
            AssetDatabase.ImportAsset(path);
            adaptation.AssignAdaptationClips(normalStateClip, adaptedStateClip, forPortrait);


            var ly = controller.layers[0].stateMachine;
            var emptyState = ly.AddState("Empty State");
            ly.defaultState = emptyState;

            var normalGraphState = controller.AddMotion(normalStateClip, 0);
            var adaptedGraphState = controller.AddMotion(adaptedStateClip, 0);

            string GetSaveControllerPath(GameObject go)
            {
                var defaultName = go.name;
                var message = $"Create a new adaptation assets for the game object '{defaultName}'\nThis name will be a prefix for all assets.";
                return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "", message);
            }
        }

        /// <summary>
        /// Draw the part of fields in <see cref="AdaptationBase"/>.
        /// </summary>
        //public static void Draw(SerializedObject serializedObject)
        public override void OnInspectorGUI()
        {
            AdaptationBase adaptation = (AdaptationBase)target;
            var supportedProp = serializedObject.FindProperty("supportedOrientations");
            var portraitProp = serializedObject.FindProperty("portraitOrDefaultAdaptation");
            var landscapeProp = serializedObject.FindProperty("landscapeAdaptation");

            (bool landscapeCompatible, bool portraitCompatible) = NotchSolutionUtilityEditor.GetOrientationCompatibility();

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

            DrawGenButton(adaptation, forPortrait: true);

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
                DrawGenButton(adaptation, forPortrait: false);
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
