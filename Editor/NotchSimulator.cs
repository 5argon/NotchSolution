using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace E7.NotchSolution
{
    public class NotchSimulator : EditorWindow
    {
        [MenuItem("Window/General/Notch Simulator")]

        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow(typeof(NotchSimulator));
            win.titleContent = new GUIContent("Notch Simulator");
        }

        void OnGUI()
        {
            //Sometimes even with flag I can see it in hierarchy until I move a mouse over it??
            EditorApplication.RepaintHierarchyWindow();

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
                if (gameViewOrientation == ScreenOrientation.Landscape)
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

            if (changed)
            {
                UpdateMockup();
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

            int GCD(int A, int B) => B == 0 ? A : GCD(B, A % B);
            int gcd = GCD(a,b);
            var integerRatio = new Vector2(screen.x / gcd, screen.y / gcd);
            return CommonAspectLookup(integerRatio);
        }

        /// <summary>
        /// Integer aspect and various round off error may make the number hideous, we could find a similar one for display purpose.
        /// </summary>
        private Vector2 CommonAspectLookup(Vector2 aspect)
        {
            float ratio = aspect.x / aspect.y;
            Vector2[] commonRatio = new Vector2[]
            {
                new Vector2(4,3),
                new Vector2(16,9),
                new Vector2(17,9),
                new Vector2(18,9),
                new Vector2(18.5f,9),
                new Vector2(18.7f,9),
                new Vector2(19,9),
                new Vector2(19.3f,9),
                new Vector2(19.5f,9),
                new Vector2(19,10),
                new Vector2(21,9),
                new Vector2(2,1),
            };
            foreach(var r in commonRatio)
            {
                var diff = Mathf.Abs(ratio - (r.x / r.y));
                if(diff < 0.001f)
                {
                    return r;
                }
                diff = Mathf.Abs(ratio - (r.y / r.x));
                if(diff < 0.001f)
                {
                    return new Vector2(r.y, r.x);
                }
            }
            return aspect;
        }

        private const string prefix = "NoSo";
        private const string mockupCanvasName = prefix + "-MockupCanvas";
        private const HideFlags overlayCanvasFlag = HideFlags.HideAndDontSave;

        private static GameObject canvasObject;
        private static MockupCanvas mockupCanvas;

        /// <summary>
        /// We lose all events on entering play mode, use this to register the event and also make a canvas again
        /// after it was destroyed by the event (that now disappeared)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AddOverlayInPlayMode()
        {
            UpdateMockup();
        }

        /// <summary>
        /// This is called even if Notch Simulator tab is not present on the screen.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //Debug.Log($"Script reloaded PLAY {EditorApplication.isPlaying} PLAY or WILL CHANGE {EditorApplication.isPlayingOrWillChangePlaymode}");

            //Avoid script reload due to entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                UpdateMockup();
            }
        }

        private static void DestroyHiddenCanvas()
        {
            if (canvasObject != null)
            {
                GameObject.DestroyImmediate(canvasObject);
            }
        }

        private static bool eventAdded = false;

        private static void UpdateMockup()
        {
            bool enableSimulation = NotchSimulatorUtility.enableSimulation;

            //Create the hidden canvas if not already.
            if (canvasObject == null)
            {
                //Find existing in the case of assembly reload
                canvasObject = GameObject.Find(mockupCanvasName);
                if (canvasObject != null)
                {
                    //Debug.Log($"[Notch Solution] Found existing");
                    mockupCanvas = canvasObject.GetComponent<MockupCanvas>();
                }
                else
                {
                    //Debug.Log($"[Notch Solution] Creating canvas");
                    var prefabGuids = AssetDatabase.FindAssets(mockupCanvasName);
                    GameObject mockupCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuids.First()));
                    canvasObject = (GameObject)PrefabUtility.InstantiatePrefab(mockupCanvasPrefab);
                    mockupCanvas = canvasObject.GetComponent<MockupCanvas>();
                    canvasObject.hideFlags = overlayCanvasFlag;

                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(canvasObject);
                    }
                }

                if (eventAdded == false)
                {
                    eventAdded = true;

                    //Add clean up event.
                    EditorApplication.playModeStateChanged += PlayModeStateChangeAction;
                    // EditorSceneManager.sceneClosing += (a, b) =>
                    // {
                    //     Debug.Log($"Scene closing {a} {b}");
                    // };
                    // EditorSceneManager.sceneClosed += (a) =>
                    // {
                    //     Debug.Log($"Scene closed {a}");
                    // };
                    // EditorSceneManager.sceneLoaded += (a, b) =>
                    //  {
                    //      Debug.Log($"Scene loaded {a} {b}");
                    //  };
                    // EditorSceneManager.sceneUnloaded += (a) =>
                    //  {
                    //      Debug.Log($"Scene unloaded {a}");
                    //  };
                    EditorSceneManager.sceneOpening += (a, b) =>
                    {
                        //Debug.Log($"Scene opening {a} {b}");
                        DestroyHiddenCanvas();
                    };

                    EditorSceneManager.sceneOpened += (a, b) =>
                    {
                        //Debug.Log($"Scene opened {a} {b}");
                        UpdateMockup();
                    };

                    void PlayModeStateChangeAction(PlayModeStateChange state)
                    {
                        //Debug.Log($"Changed state PLAY {EditorApplication.isPlaying} PLAY or WILL CHANGE {EditorApplication.isPlayingOrWillChangePlaymode}");
                        switch (state)
                        {
                            case PlayModeStateChange.EnteredEditMode:
                                //Debug.Log($"Entered Edit {canvasObject}");
                                AddOverlayInPlayMode(); //For when coming back from play mode.
                                break;
                            case PlayModeStateChange.EnteredPlayMode:
                                //Debug.Log($"Entered Play {canvasObject}");
                                break;
                            case PlayModeStateChange.ExitingEditMode:
                                //Debug.Log($"Exiting Edit {canvasObject}");
                                DestroyHiddenCanvas();//Clean up the DontSave canvas we made in edit mode.
                                break;
                            case PlayModeStateChange.ExitingPlayMode:
                                //Debug.Log($"Exiting Play {canvasObject}");
                                DestroyHiddenCanvas();//Clean up the DontSave canvas we made in play mode.
                                break;
                        }
                    }

                }
            }

            if (enableSimulation)
            {
                //Landscape has an alias that turns ToString into LandscapeLeft lol
                var orientationString = NotchSimulatorUtility.GetGameViewOrientation() == ScreenOrientation.Landscape ? nameof(ScreenOrientation.Landscape) : nameof(ScreenOrientation.Portrait);
                SimulationDevice simDevice = NotchSimulatorUtility.selectedDevice;
                var name = $"{prefix}-{simDevice.ToString()}-{orientationString}";
                var guids = AssetDatabase.FindAssets(name);
                var first = guids.FirstOrDefault();

                if (first == default(string))
                {
                    throw new InvalidOperationException($"No mockup image named {name} in NotchSolution/Editor/Mockups folder!");
                }
                Sprite mockupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(first));

                mockupCanvas.Show();
                mockupCanvas.SetMockupSprite(mockupSprite, NotchSimulatorUtility.GetGameViewOrientation(), simulate: enableSimulation, flipped: NotchSimulatorUtility.flipOrientation);
            }
            else
            {
                mockupCanvas.Hide();
            }
        }
    }
}