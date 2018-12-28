using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;

namespace E7.NotchSolution
{
    public class NotchSimulator : EditorWindow
    {
        [MenuItem("Window/General/Notch Simulator")]

        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow(typeof(NotchSimulator));
            win.titleContent= new GUIContent("Notch Simulator");
        }

        void OnGUI()
        {
            bool isEnableSimulation = NotchSimulatorUtility.enableSimulation;
            EditorGUI.BeginChangeCheck();
            NotchSimulatorUtility.enableSimulation = EditorGUILayout.BeginToggleGroup("Simulate", NotchSimulatorUtility.enableSimulation);
            EditorGUI.indentLevel++;
            NotchSimulatorUtility.selectedDevice = (SimulationDevice)EditorGUILayout.EnumPopup(NotchSimulatorUtility.selectedDevice);
            NotchSimulatorUtility.flipOrientation = EditorGUILayout.Toggle("Flip Orientation", NotchSimulatorUtility.flipOrientation);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            bool changed = EditorGUI.EndChangeCheck();

            if(changed)
            {
                UpdateMockup(NotchSimulatorUtility.selectedDevice);
            }

            if (isEnableSimulation || (!isEnableSimulation && NotchSimulatorUtility.enableSimulation))
            {
                NotchSolutionUtility.SimulateSafeAreaRelative = NotchSimulatorUtility.enableSimulation ? NotchSimulatorUtility.SimulatorSafeAreaRelative : new Rect(0, 0, 1, 1);

                var nps = GameObject.FindObjectsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
                foreach (var np in nps)
                {
                    np.SimulatorUpdate();
                }
            }
        }

        private const string prefix = "NoSo";
        private const string mockupCanvasName = prefix + "-MockupCanvas";

        private void UpdateMockup(SimulationDevice simDevice)
        {
            bool enableSimulation = NotchSimulatorUtility.enableSimulation;
            GameObject mockupCanvas = GameObject.Find(mockupCanvasName);
            if (enableSimulation)
            {
                //Landscape has an alias that turns ToString into LandscapeLeft lol
                var orientationString = NotchSimulatorUtility.GetGameViewOrientation() == ScreenOrientation.Landscape ? nameof(ScreenOrientation.Landscape) : nameof(ScreenOrientation.Portrait);
                var name = $"{prefix}-{simDevice.ToString()}-{orientationString}";
                var guids = AssetDatabase.FindAssets(name);
                var first = guids.FirstOrDefault();
                if(first == default(string))
                {
                    throw new InvalidOperationException($"No mockup image named {name} in NotchSolution/Editor/Mockups folder!");
                }
                Sprite mockupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(first));
                if (mockupCanvas == null)
                {
                    var prefabGuids = AssetDatabase.FindAssets(mockupCanvasName);
                    GameObject mockupCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuids.First()));
                    mockupCanvas = (GameObject)PrefabUtility.InstantiatePrefab(mockupCanvasPrefab);
                    mockupCanvas.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable | HideFlags.HideInInspector;
                }
                var mc = mockupCanvas.GetComponent<MockupCanvas>();

                mc.SetMockupSprite(mockupSprite, NotchSimulatorUtility.GetGameViewOrientation(), simulate: enableSimulation, flipped: NotchSimulatorUtility.flipOrientation);
            }
            else
            {
                GameObject.DestroyImmediate(mockupCanvas);
            }
        }
    }
}