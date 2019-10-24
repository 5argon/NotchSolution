//#define NOTCH_SOLUTION_DEBUG_TRANSITIONS

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build; //For bugfix hack
using UnityEditor.Build.Reporting; //For bugfix hack
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace E7.NotchSolution
{
    /// <summary>
    /// Notch Solution components can receive simulated device values in editor from this instead of the usual <see cref="Screen"> API.
    /// 
    /// Also the mockup overlay is provided by an invisible full screen canvas game object with <see cref="HideFlags.HideAndDontSave">.
    /// </summary>
    public class NotchSimulator : EditorWindow , IPreprocessBuildWithReport //For bugfix hack
    {
        internal static NotchSimulator win;
        Vector2 gameviewResolution;

        [MenuItem("Window/General/Notch Simulator")]
        public static void ShowWindow()
        {
            win = (NotchSimulator)EditorWindow.GetWindow(typeof(NotchSimulator));
            win.titleContent = new GUIContent("Notch Simulator");
        }

        [ExecuteInEditMode] private void OnEnable() { EditorApplication.update += RespawnMockup; }
        [ExecuteInEditMode] private void OnDisable() { EditorApplication.update -= RespawnMockup; }
        void RespawnMockup()
        {
            //When the game view is changed, the mockup sometimes disappears or isn't scaled correctly
            if (gameviewResolution != Handles.GetMainGameViewSize())
            {
                DestroyHiddenCanvas(); //So we delete the old canvas
                UpdateAllMockups(); //And we respawn it
                UpdateSimulatorTargets();
                gameviewResolution = Handles.GetMainGameViewSize(); //Update the saved game view
            }
        }

        /// <summary>
        /// Part of the IPreprocessBuildWithReport
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        /// Bugfix hack
        /// https://github.com/5argon/NotchSolution/issues/11
        /// https://fogbugz.unity3d.com/default.asp?1157422_sfvtcfi1jmvc3702
        /// https://fogbugz.unity3d.com/default.asp?1167068_4884utp26ji27ro0
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            //Unity has a bug that any scene change not ordered by you (building, or execute a test)
            //could not destroy a DontSave game object (Hide or not doesn't matter) and instead logs some errors.
            //This is a problem on building since any console error will fail the build.
            //We will hack it to destroy it ourselve first before the scene could change.

            //However this destroy we are doing is also subjected to the same error, and will fail the build as this
            //preprocess build callback is called when you are already in a build. To counter this, we set a hide flag
            //to None (risk of being baked in the scene? I think not as long as we didn't save it) to not cause the bug
            //then quickly destroy it without errors.

            //The error when you run a Test Runner is still there though, because Unity didn't provide a callback 
            //when entering test for me to hack my way into it. At least you can build the game while NoSo is running.

            //I have submitted the bug since 2019.1 release, now 2019.2 is here but it is still not fixed.

            if (mockupCanvas != null)
            {
                mockupCanvas.gameObject.hideFlags = HideFlags.None;
                DestroyImmediate(mockupCanvas.gameObject);
                mockupCanvas = null;
            }
            if (prefabMockupCanvas != null)
            {
                prefabMockupCanvas.gameObject.hideFlags = HideFlags.None;
                DestroyImmediate(prefabMockupCanvas.gameObject);
                prefabMockupCanvas = null;
            }
        }

        /// <summary>
        /// It is currently active only when Notch Simulator tab is present.
        /// </summary>
        void OnGUI()
        {
            win = this;
            //Sometimes even with flag I can see it in hierarchy until I move a mouse over it??
            EditorApplication.RepaintHierarchyWindow();

            bool enableSimulation = NotchSimulatorUtility.enableSimulation;
            EditorGUI.BeginChangeCheck();

            string shortcut = ShortcutManager.instance.GetShortcutBinding(NotchSolutionShortcuts.toggleSimulationShortcut).ToString();
            if (string.IsNullOrEmpty(shortcut)) shortcut = "None";
            NotchSimulatorUtility.enableSimulation = EditorGUILayout.BeginToggleGroup($"Simulate ({shortcut})", NotchSimulatorUtility.enableSimulation);
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

                var simAspect = NotchSolutionUtilityEditor.ScreenRatio(simulationDevice.screenSize);
                var gameViewAspect = NotchSolutionUtilityEditor.ScreenRatio(gameViewSize);
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
                UpdateAllMockups();
            }

            UpdateSimulatorTargets();
        }

        /// <summary>
        /// Get all <see cref="INotchSimulatorTarget"> and update them.
        /// </summary>
        internal static void UpdateSimulatorTargets()
        {
            var simulatedRectRelative = NotchSimulatorUtility.enableSimulation ? NotchSimulatorUtility.CalculateSimulatorSafeAreaRelative() : new Rect(0, 0, 1, 1);
            var simulatedCutoutsRelative = NotchSimulatorUtility.enableSimulation ? NotchSimulatorUtility.CalculateSimulatorCutoutsRelative() : new Rect[0];

            //This value could be used by the component statically.
            NotchSolutionUtilityEditor.SimulatedSafeAreaRelative = simulatedRectRelative;
#if UNITY_2019_2_OR_NEWER
            NotchSolutionUtilityEditor.SimulatedCutoutsRelative = simulatedCutoutsRelative;
#endif

            var normalSceneSimTargets = GameObject.FindObjectsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
            foreach (var nst in normalSceneSimTargets)
            {
                nst.SimulatorUpdate(simulatedRectRelative, simulatedCutoutsRelative);
            }

            //Now find one in the prefab mode scene as well
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var prefabSceneSimTargets = prefabStage.stageHandle.FindComponentsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
                foreach (var nst in prefabSceneSimTargets)
                {
                    nst.SimulatorUpdate(simulatedRectRelative, simulatedCutoutsRelative);
                }
            }
        }

        private const string prefix = "NoSo";
        private const string mockupCanvasName = prefix + "-MockupCanvas";
        private const HideFlags overlayCanvasFlag = HideFlags.HideAndDontSave;

        private static MockupCanvas mockupCanvas;
        private static MockupCanvas prefabMockupCanvas;

        /// <summary>
        /// This need to return both from normal scene and prefab environment scene.
        /// </summary>
        private static IEnumerable<MockupCanvas> AllMockupCanvases
        {
            get
            {
                if (mockupCanvas != null)
                {
                    yield return mockupCanvas;
                }
                if (prefabMockupCanvas != null)
                {
                    yield return prefabMockupCanvas;
                }
            }
        }

        /// <summary>
        /// We lose all events on entering play mode, use this to register the event and also make a canvas again
        /// after it was destroyed by the event (that now disappeared)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AddOverlayInPlayMode()
        {
            UpdateAllMockups();
        }

        /// <summary>
        /// This is called even if Notch Simulator tab is not present on the screen.
        /// Also have to handle if we reload scripts while in prefab mode.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //DebugTransitions($"Script reloaded PLAY {EditorApplication.isPlaying} PLAY or WILL CHANGE {EditorApplication.isPlayingOrWillChangePlaymode}");

            //Avoid script reload due to entering playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode == false)
            {
                UpdateAllMockups();
            }
        }

        private static void DestroyHiddenCanvas()
        {
            if (mockupCanvas != null)
            {
                GameObject.DestroyImmediate(mockupCanvas.gameObject);
            }
        }

        private static bool eventAdded = false;

        internal static void UpdateAllMockups()
        {
            //When building, the scene may open-close multiple times and brought back the mockup canvas,
            //which combined with bugs mentioned at https://github.com/5argon/NotchSolution/issues/11,
            //will fail the build. This `if` prevents mockup refresh while building.
            if(BuildPipeline.isBuildingPlayer) return;

            EnsureCanvasAndEventSetup();

            //Make the editing environment contains an another copy of mockup canvas.
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EnsureCanvasAndEventSetup(prefabStage: prefabStage);
            }

            bool enableSimulation = NotchSimulatorUtility.enableSimulation;
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

                foreach (var mockup in AllMockupCanvases)
                {
                    mockup.Show();
                    mockup.SetMockupSprite(
                         sprite: mockupSprite,
                         orientation: NotchSimulatorUtility.GetGameViewOrientation(),
                         simulate: enableSimulation,
                         flipped: NotchSimulatorUtility.flipOrientation
                     );
                }
            }
            else
            {
                foreach (var mockup in AllMockupCanvases)
                {
                    mockup.Hide();
                }
            }
        }

        private static void DebugTransitions(string s)
        {
#if NOTCH_SOLUTION_DEBUG_TRANSITIONS
            Debug.Log(s);
#endif
        }


        /// <param name="prefabStage">If not `null`, look for the mockup canvas on environment scene for editing a prefab **instead** of normal scenes.</param>
        private static void EnsureCanvasAndEventSetup(PrefabStage prefabStage = null)
        {
            //Create the hidden canvas if not already.
            bool prefabMode = prefabStage != null;
            var selectedMockupCanvas = prefabMode ? prefabMockupCanvas : mockupCanvas;

            if (selectedMockupCanvas == null)
            {
                //Find existing in the case of assembly reload
                //For some reason GameObject.FindObjectOfType could not get the canvas on main scene, it is active also, but by name works...
                var canvasObject = prefabMode ? prefabStage.stageHandle.FindComponentOfType<MockupCanvas>() : GameObject.Find(mockupCanvasName)?.GetComponent<MockupCanvas>();
                if (canvasObject != null)
                {
                    DebugTransitions($"[Notch Solution] Found existing (Prefab mode {prefabMode})");
                }
                else
                {
                    var prefabGuids = AssetDatabase.FindAssets(mockupCanvasName);
                    if (prefabGuids.Length == 0)
                    {
                        return;
                    }
                    DebugTransitions($"[Notch Solution] Creating canvas (Prefab mode {prefabMode})");
                    GameObject mockupCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuids.First()));

                    var instantiated =
                    prefabMode ?
                    (GameObject)(PrefabUtility.InstantiatePrefab(mockupCanvasPrefab, prefabStage.scene)) :
                    (GameObject)PrefabUtility.InstantiatePrefab(mockupCanvasPrefab);

                    //It sometimes instantiated into null on script reloading when starting Unity?
                    if(instantiated != null)
                    {
                        canvasObject = instantiated.GetComponent<MockupCanvas>();
                        instantiated.hideFlags = overlayCanvasFlag;

                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(canvasObject);
                        }
                    }
                }

                canvasObject.PrefabStage = prefabMode;
                if (prefabMode) prefabMockupCanvas = canvasObject;
                else mockupCanvas = canvasObject;

                if (eventAdded == false)
                {
                    eventAdded = true;

                    //Add clean up event.
                    EditorApplication.playModeStateChanged += PlayModeStateChangeAction;
                    // EditorSceneManager.sceneClosing += (a, b) =>
                    // {
                    //     DebugTransitions($"Scene closing {a} {b}");
                    // };
                    // EditorSceneManager.sceneClosed += (a) =>
                    // {
                    //     DebugTransitions($"Scene closed {a}");
                    // };
                    // EditorSceneManager.sceneLoaded += (a, b) =>
                    //  {
                    //      DebugTransitions($"Scene loaded {a} {b}");
                    //  };
                    // EditorSceneManager.sceneUnloaded += (a) =>
                    //  {
                    //      DebugTransitions($"Scene unloaded {a}");
                    //  };
                    PrefabStage.prefabStageOpened += (ps) =>
                    {
                        DebugTransitions($"Prefab opening {ps.scene.GetRootGameObjects().First().name} {ps.prefabContentsRoot.name}");

                        //On open prefab, the "dont save" objects on the main scene will disappear too.
                        //So that we could still see it in the game view WHILE editing a prefab, we make it back.
                        //Along with this the prefab mode canvas will also be updated.
                        UpdateAllMockups();

                        //On entering prefab mode, the Notch Simulator panel did not get OnGUI().
                        UpdateSimulatorTargets();
                    };

                    PrefabStage.prefabStageClosing += (ps) =>
                    {
                        DebugTransitions($"Prefab closing {ps.scene.GetRootGameObjects().First().name} {ps.prefabContentsRoot.name}");
                        //There is no problem on closing prefab stage, no need to restore the outer mockup.
                    };

                    EditorSceneManager.sceneOpening += (a, b) =>
                    {
                        DebugTransitions($"Scene opening {a} {b}");
                        DestroyHiddenCanvas();
                    };

                    EditorSceneManager.sceneOpened += (a, b) =>
                    {
                        DebugTransitions($"Scene opened {a} {b}");
                        UpdateAllMockups();
                    };

                    void PlayModeStateChangeAction(PlayModeStateChange state)
                    {
                        DebugTransitions($"Changed state PLAY {EditorApplication.isPlaying} PLAY or WILL CHANGE {EditorApplication.isPlayingOrWillChangePlaymode}");
                        switch (state)
                        {
                            case PlayModeStateChange.EnteredEditMode:
                                DebugTransitions($"Entered Edit {canvasObject}");
                                AddOverlayInPlayMode(); //For when coming back from play mode.
                                break;
                            case PlayModeStateChange.EnteredPlayMode:
                                DebugTransitions($"Entered Play {canvasObject}");
                                break;
                            case PlayModeStateChange.ExitingEditMode:
                                DebugTransitions($"Exiting Edit {canvasObject}");
                                DestroyHiddenCanvas();//Clean up the DontSave canvas we made in edit mode.
                                break;
                            case PlayModeStateChange.ExitingPlayMode:
                                DebugTransitions($"Exiting Play {canvasObject}");
                                DestroyHiddenCanvas();//Clean up the DontSave canvas we made in play mode.
                                break;
                        }
                    }

                }
            }

        }
    }
}
