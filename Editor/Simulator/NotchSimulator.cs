//#define NOTCH_SOLUTION_DEBUG_TRANSITIONS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build; //For bugfix hack
using UnityEditor.Build.Reporting; //For bugfix hack
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.EventSystems;

namespace E7.NotchSolution.Editor
{
    /// <summary>
    /// Notch Solution components can receive simulated device values in editor from this instead of the usual <see cref="Screen"/> API.
    /// 
    /// Also the mockup overlay is provided by an invisible full screen canvas game object with <see cref="HideFlags.HideAndDontSave"/>.
    /// </summary>
    internal class NotchSimulator : EditorWindow, IHasCustomMenu, IPreprocessBuildWithReport //For bugfix hack
    {
        private static NotchSimulator win;
        Vector2 gameviewResolution;

        [MenuItem("Window/General/Notch Simulator")]
        public static void ShowWindow()
        {
            win = (NotchSimulator)EditorWindow.GetWindow(typeof(NotchSimulator));
            win.titleContent = new GUIContent("Notch Simulator");
        }

        //IHasCustomMenu
        public void AddItemsToMenu(GenericMenu menu)
        {
            var flip = Settings.Instance.FlipOrientation;
            menu.AddItem(new GUIContent("Flip Orientation"), on: flip, () =>
            {
                var settings = Settings.Instance;
                settings.FlipOrientation = !settings.FlipOrientation;
                settings.Save();
                UpdateAllMockups();
                UpdateSimulatorTargets();
            });

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Reset Settings"), on: false, ResetSettings);
            menu.AddItem(new GUIContent("Refresh Device List"), on: false, SimulationDatabase.Refresh);
        }

        private void ResetSettings()
        {
            Settings.Instance.Reset();
            Debug.Log($"Reset all Notch Solution settings.");
        }

        [ExecuteInEditMode]
        private void OnEnable()
        {
            SimulationDatabase.Refresh();
            EditorApplication.update += RespawnMockup;
        }

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

        internal static void Redraw() => win?.Repaint();

        /// <summary>
        /// It is currently active only when Notch Simulator tab is present.
        /// </summary>
        void OnGUI()
        {
            win = this;

            //Sometimes even with flag I can see it in hierarchy until I move a mouse over it??
            EditorApplication.RepaintHierarchyWindow();

            EditorGUI.BeginChangeCheck();

            var settings = Settings.Instance;

            string switchConfigShortcut = ShortcutManager.instance.GetShortcutBinding(NotchSolutionShortcuts.switchConfigurationShortcut).ToString();
            if (string.IsNullOrEmpty(switchConfigShortcut)) switchConfigShortcut = "None";
            string simulateShortcut = ShortcutManager.instance.GetShortcutBinding(NotchSolutionShortcuts.toggleSimulationShortcut).ToString();
            if (string.IsNullOrEmpty(simulateShortcut)) simulateShortcut = "None";

            settings.EnableSimulation = EditorGUILayout.BeginToggleGroup($"Simulate ({simulateShortcut})", settings.EnableSimulation);

            int previousIndex = settings.ActiveConfiguration.DeviceIndex;
            int currentIndex = Mathf.Clamp(settings.ActiveConfiguration.DeviceIndex, 0, SimulationDatabase.KeyList.Length - 1);

            int selectedIndex = EditorGUILayout.Popup(currentIndex, SimulationDatabase.KeyList);
            if (GUILayout.Button($"{settings.ActiveConfiguration.ConfigurationName} ({switchConfigShortcut})", EditorStyles.helpBox))
            {
                NotchSolutionShortcuts.SwitchConfiguration();
            }
            EditorGUILayout.EndToggleGroup();

            settings.ActiveConfiguration.DeviceIndex = selectedIndex;

            var simulationDevice = SimulationDatabase.Get(SimulationDatabase.KeyList[selectedIndex]);

            //Draw warning about wrong aspect ratio
            if (settings.EnableSimulation && simulationDevice != null)
            {
                ScreenOrientation gameViewOrientation = NotchSimulatorUtility.GetGameViewOrientation();

                Vector2 gameViewSize = NotchSimulatorUtility.GetMainGameViewSize();
                if (gameViewOrientation == ScreenOrientation.Landscape)
                {
                    var flip = gameViewSize.x;
                    gameViewSize.x = gameViewSize.y;
                    gameViewSize.y = flip;
                }

                var screen = simulationDevice.Screens.FirstOrDefault();
                var simAspect = NotchSolutionUtilityEditor.ScreenRatio(new Vector2(screen.width, screen.height));
                var gameViewAspect = NotchSolutionUtilityEditor.ScreenRatio(gameViewSize);
                var aspectDiff = Math.Abs((simAspect.x / simAspect.y) - (gameViewAspect.x / gameViewAspect.y));
                if (aspectDiff > 0.01f)
                {
                    EditorGUILayout.HelpBox($"The selected simulation device has an aspect ratio of {simAspect.y}:{simAspect.x} ({screen.height}x{screen.width}) but your game view is currently in aspect {gameViewAspect.y}:{gameViewAspect.x} ({gameViewSize.y}x{gameViewSize.x}). The overlay mockup won't be displayed correctly.", MessageType.Warning);
                }
            }

            bool changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                UpdateAllMockups();
                settings.Save();
            }

            UpdateSimulatorTargets();
        }

        /// <summary>
        /// Get all <see cref="INotchSimulatorTarget"/> and update them.
        /// </summary>
        internal static void UpdateSimulatorTargets()
        {
            var enableSimulation = Settings.Instance.EnableSimulation;
            var selectedDevice = SimulationDatabase.ByIndex(Settings.Instance.ActiveConfiguration.DeviceIndex);

            var simulatedRectRelative = enableSimulation && selectedDevice != null ? NotchSimulatorUtility.CalculateSimulatorSafeAreaRelative(selectedDevice) : NotchSolutionUtility.defaultSafeArea;
            var simulatedCutoutsRelative = enableSimulation && selectedDevice != null ? NotchSimulatorUtility.CalculateSimulatorCutoutsRelative(selectedDevice) : NotchSolutionUtility.defaultCutouts;

            var normalSceneSimTargets = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<INotchSimulatorTarget>();
            foreach (var nst in normalSceneSimTargets)
            {
                nst.SimulatorUpdate(simulatedRectRelative, simulatedCutoutsRelative);
            }

            //Now find one in the prefab mode scene as well
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var prefabSceneSimTargets = prefabStage.stageHandle.FindComponentsOfType<MonoBehaviour>().OfType<INotchSimulatorTarget>();
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
                if (mockupCanvas == null) mockupCanvas = GameObject.Find(mockupCanvasName)?.GetComponent<MockupCanvas>();
                if (mockupCanvas != null) yield return mockupCanvas;

                if (prefabMockupCanvas == null)
                {
                    var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabStage != null) prefabMockupCanvas = prefabStage.stageHandle.FindComponentOfType<MockupCanvas>();
                }
                if (prefabMockupCanvas != null) yield return prefabMockupCanvas;
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
            foreach (var mockup in AllMockupCanvases)
            {
                if (EditorApplication.isPlaying) Destroy(mockup.gameObject);
                else DestroyImmediate(mockup.gameObject, false);
            }
        }

        private static bool eventAdded = false;

        internal static void UpdateAllMockups()
        {
            //When building, the scene may open-close multiple times and brought back the mockup canvas,
            //which combined with bugs mentioned at https://github.com/5argon/NotchSolution/issues/11,
            //will fail the build. This `if` prevents mockup refresh while building.
            if (BuildPipeline.isBuildingPlayer) return;

            EnsureCanvasAndEventSetup();

            //Make the editing environment contains an another copy of mockup canvas.
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EnsureCanvasAndEventSetup(prefabStage: prefabStage);
            }

            var settings = Settings.Instance;
            bool enableSimulation = settings.EnableSimulation;
            var selectedDevice = SimulationDatabase.ByIndex(Settings.Instance.ActiveConfiguration.DeviceIndex);

            if (enableSimulation && selectedDevice != null)
            {
                var name = selectedDevice.Meta.overlay;
                Sprite mockupSprite = null;
                if (!string.IsNullOrEmpty(name))
                {
                    var filePath = Path.Combine(NotchSimulatorUtility.DevicesFolder, name);
                    mockupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                    if (mockupSprite == null)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            Texture2D tex = new Texture2D(1, 1);
                            tex.LoadImage(System.IO.File.ReadAllBytes(filePath));
                            mockupSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                        }
                        else Debug.LogWarning($"No mockup image named {name} in {NotchSimulatorUtility.DevicesFolder} folder!");
                    }
                }


                foreach (var mockup in AllMockupCanvases)
                {
                    mockup.UpdateMockupSprite(
                         sprite: mockupSprite,
                         orientation: NotchSimulatorUtility.GetGameViewOrientation(),
                         simulate: enableSimulation,
                         flipped: settings.FlipOrientation,
                         prefabModeOverlayColor: Settings.Instance.PrefabModeOverlayColor
                     );
                }
            }
            else DestroyHiddenCanvas();
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
                    if (instantiated != null)
                    {
                        canvasObject = instantiated.GetComponent<MockupCanvas>();
                        instantiated.hideFlags = overlayCanvasFlag;

                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(canvasObject);
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
}
