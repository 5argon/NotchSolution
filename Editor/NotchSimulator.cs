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
            bool enableSimulation = NotchSimulatorUtility.enableSimulation;
            EditorGUI.BeginChangeCheck();
            NotchSimulatorUtility.enableSimulation = EditorGUILayout.BeginToggleGroup("Simulate", NotchSimulatorUtility.enableSimulation);
            EditorGUI.indentLevel++;

            NotchSimulatorUtility.selectedDevice = (SimulationDevice)EditorGUILayout.EnumPopup(NotchSimulatorUtility.selectedDevice);
            NotchSimulatorUtility.flipOrientation = EditorGUILayout.Toggle("Flip Orientation", NotchSimulatorUtility.flipOrientation);

            var simulationDevice = SimulationDatabase.db[NotchSimulatorUtility.selectedDevice];

            //Draw warning about wrong aspect ratio
            if (enableSimulation)
            {
                ScreenOrientation gameViewOrientation = NotchSimulatorUtility.GetGameViewOrientation();

                Vector2 simSize = gameViewOrientation == ScreenOrientation.Portrait ?
                 simulationDevice.screenSize : new Vector2(simulationDevice.screenSize.y, simulationDevice.screenSize.x);

                Vector2 gameViewSize = NotchSimulatorUtility.GetMainGameViewSize();
                if(gameViewOrientation == ScreenOrientation.Landscape)
                {
                    var flip = gameViewSize.x;
                    gameViewSize.x = gameViewSize.y;
                    gameViewSize.y = flip;
                }

                var simAspect = ScreenRatio(simulationDevice.screenSize);
                var gameViewAspect = ScreenRatio(gameViewSize);
                var aspectDiff = Math.Abs((simAspect.x / simAspect.y) - (gameViewAspect.x / gameViewAspect.y));
                if (aspectDiff > 0.01f)
                {
                    EditorGUILayout.HelpBox($"The selected simulation device has an aspect ratio of {simAspect.y}:{simAspect.x} ({simulationDevice.screenSize.y}x{simulationDevice.screenSize.x}) but your game view is currently in aspect {gameViewAspect.y}:{gameViewAspect.x} ({gameViewSize.y}x{gameViewSize.x}). The overlay mockup will be stretched from its intended ratio.", MessageType.Warning);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            bool changed = EditorGUI.EndChangeCheck();

            if(changed)
            {
                UpdateMockup(NotchSimulatorUtility.selectedDevice);
            }

            if (enableSimulation || (!enableSimulation && NotchSimulatorUtility.enableSimulation))
            {
                NotchSolutionUtility.SimulateSafeAreaRelative = NotchSimulatorUtility.enableSimulation ? NotchSimulatorUtility.SimulatorSafeAreaRelative : new Rect(0, 0, 1, 1);
                var nps = GameObject.FindObjectsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
                foreach (var np in nps)
                {
                    np.SimulatorUpdate();
                }
            }
        }
        
        private Vector2 ScreenRatio(Vector2 screen)
        {
            int a = (int)screen.x;
            int b = (int)screen.y;

            int gcd = 0;
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            if (a == 0) gcd = b;
            else gcd = a;
            
            return new Vector2(screen.x / gcd, screen.y / gcd);
        }

        private const string prefix = "NoSo";
        private const string mockupCanvasName = prefix + "-MockupCanvas";

        //TODO : Game view related reflection methods should be cached.

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