//#define NOTCH_SOLUTION_DEBUG_TRANSITIONS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

//For bugfix hack
//For bugfix hack

namespace E7.NotchSolution.Editor
{
    /// <summary>
    ///     Notch Solution components can receive simulated device values in editor from this
    ///     instead of the usual <see cref="Screen"/> API. Also the mockup overlay is provided by
    ///     an invisible full screen canvas game object with <see cref="HideFlags.HideAndDontSave"/>.
    /// </summary>
    internal class NotchSimulator : EditorWindow, IHasCustomMenu, IPreprocessBuildWithReport //For bugfix hack
    {
        private const string prefix = "NoSo";
        private const string mockupCanvasName = prefix + "-MockupCanvas";
        private const HideFlags overlayCanvasFlag = HideFlags.HideAndDontSave;
        private static NotchSimulator win;
        private static Vector2 gameviewResolution;

        private static MockupCanvas mockupCanvas;
        private static MockupCanvas prefabMockupCanvas;

        private static bool eventAdded;

        private Vector2 scrollPos;

        internal static bool IsOpen => win;

        /// <summary>
        ///     This need to return both from normal scene and prefab environment scene.
        /// </summary>
        internal static IEnumerable<MockupCanvas> AllMockupCanvases
        {
            get
            {
                if (mockupCanvas == null)
                {
                    mockupCanvas = GameObject.Find(mockupCanvasName)?.GetComponent<MockupCanvas>();
                }

                if (mockupCanvas != null)
                {
                    yield return mockupCanvas;
                }

                if (prefabMockupCanvas == null)
                {
                    var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabStage != null)
                    {
                        prefabMockupCanvas = prefabStage.stageHandle.FindComponentOfType<MockupCanvas>();
                    }
                }

                if (prefabMockupCanvas != null)
                {
                    yield return prefabMockupCanvas;
                }
            }
        }

        [ExecuteInEditMode]
        private void OnEnable()
        {
            SimulationDatabase.Refresh();
            EditorApplication.update += RespawnMockup;
        }

        [ExecuteInEditMode] private void OnDisable()
        {
            EditorApplication.update -= RespawnMockup;
        }

        /// <summary>
        ///     It is currently active only when Notch Simulator tab is present.
        /// </summary>
        private void OnGUI()
        {
            win = this;

            //Sometimes even with flag I can see it in hierarchy until I move a mouse over it??
            EditorApplication.RepaintHierarchyWindow();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUI.BeginChangeCheck();

            var settings = Settings.Instance;

            var switchConfigShortcut = ShortcutManager.instance
                .GetShortcutBinding(NotchSolutionShortcuts.switchConfigurationShortcut).ToString();
            if (string.IsNullOrEmpty(switchConfigShortcut))
            {
                switchConfigShortcut = "None";
            }

            var simulateShortcut = ShortcutManager.instance
                .GetShortcutBinding(NotchSolutionShortcuts.toggleSimulationShortcut).ToString();
            if (string.IsNullOrEmpty(simulateShortcut))
            {
                simulateShortcut = "None";
            }

            settings.EnableSimulation =
                EditorGUILayout.BeginToggleGroup($"Simulate ({simulateShortcut})", settings.EnableSimulation);

            var previousIndex = settings.ActiveConfiguration.DeviceIndex;
            var currentIndex = Mathf.Clamp(settings.ActiveConfiguration.DeviceIndex, 0,
                SimulationDatabase.KeyList.Length - 1);

            var selectedIndex = EditorGUILayout.Popup(currentIndex, SimulationDatabase.KeyList);
            settings.ActiveConfiguration.Orientation =
                (PreviewOrientation) EditorGUILayout.EnumPopup("Orientation", settings.ActiveConfiguration.Orientation);
            settings.ActiveConfiguration.GameViewSize =
                (GameViewSizePolicy) EditorGUILayout.EnumPopup("Game View Size",
                    settings.ActiveConfiguration.GameViewSize);

            if (GUILayout.Button($"{settings.ActiveConfiguration.ConfigurationName} ({switchConfigShortcut})",
                EditorStyles.helpBox))
            {
                NotchSolutionShortcuts.SwitchConfiguration();
            }

            EditorGUILayout.EndToggleGroup();

            settings.ActiveConfiguration.DeviceIndex = selectedIndex;

            var simulationDevice = SimulationDatabase.Get(SimulationDatabase.KeyList[selectedIndex]);

            //Draw warning about wrong aspect ratio
            if (settings.EnableSimulation && simulationDevice != null)
            {
                if (NotchSolutionUtilityEditor.UnityDeviceSimulatorActive)
                {
                    EditorGUILayout.HelpBox(
                        "Device preview won't resize automatically because Unity Device Simulator window is open.",
                        MessageType.Warning);
                }

                var gameViewOrientation = NotchSimulatorUtility.GetGameViewOrientation();

                var gameViewSize = NotchSimulatorUtility.GetMainGameViewSize();
                if (gameViewOrientation == ScreenOrientation.Landscape)
                {
                    var flip = gameViewSize.x;
                    gameViewSize.x = gameViewSize.y;
                    gameViewSize.y = flip;
                }

                var screen = simulationDevice.Screens.FirstOrDefault();
                if (screen != null)
                {
                    var simAspect = NotchSolutionUtilityEditor.ScreenRatio(new Vector2(screen.width, screen.height));
                    var gameViewAspect = NotchSolutionUtilityEditor.ScreenRatio(gameViewSize);
                    var aspectDiff = Math.Abs(simAspect.x / simAspect.y - gameViewAspect.x / gameViewAspect.y);
                    if (aspectDiff > 0.01f)
                    {
                        EditorGUILayout.HelpBox(
                            "The selected simulation device has an aspect ratio of " +
                            $"{simAspect.y}:{simAspect.x} ({screen.height}x{screen.width}) but your game view is currently " +
                            $"in aspect {gameViewAspect.y}:{gameViewAspect.x} ({gameViewSize.y}x{gameViewSize.x}). " +
                            "The overlay mockup won't be displayed correctly.",
                            MessageType.Warning);
                    }
                }
            }

            var changed = EditorGUI.EndChangeCheck();

            if (changed)
            {
                UpdateAllMockups();
                settings.Save();
            }

            EditorGUILayout.EndScrollView();

            UpdateSimulatorTargets();
        }

        //IHasCustomMenu
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Reset Settings"), false, ResetSettings);
            menu.AddItem(new GUIContent("Refresh Device List"), false, SimulationDatabase.Refresh);
        }

        /// <summary>
        ///     Part of the IPreprocessBuildWithReport
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        ///     Bugfix hack
        ///     https://github.com/5argon/NotchSolution/issues/11
        ///     https://fogbugz.unity3d.com/default.asp?1157422_sfvtcfi1jmvc3702
        ///     https://fogbugz.unity3d.com/default.asp?1167068_4884utp26ji27ro0
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

        [MenuItem("Window/General/Notch Simulator")]
        public static void ShowWindow()
        {
            win = (NotchSimulator) GetWindow(typeof(NotchSimulator));
            win.titleContent = new GUIContent("Notch Simulator");
        }

        private void ResetSettings()
        {
            Settings.Instance.Reset();
            Debug.Log("Reset all Notch Solution settings.");
        }

        internal static void RespawnMockup()
        {
            //When the game view is changed, the mockup sometimes disappears or isn't scaled correctly
            if (gameviewResolution != NotchSimulatorUtility.GetMainGameViewSize())
            {
                DestroyHiddenCanvas(); //So we delete the old canvas
                UpdateAllMockups(); //And we respawn it
                UpdateSimulatorTargets();
                gameviewResolution = NotchSimulatorUtility.GetMainGameViewSize(); //Update the saved game view
            }
        }

        internal static void Redraw()
        {
            win?.Repaint();
        }

        /// <summary>
        ///     Get all <see cref="INotchSimulatorTarget"/> and update them.
        /// </summary>
        internal static void UpdateSimulatorTargets()
        {
            var enableSimulation = Settings.Instance.EnableSimulation;
            var selectedDevice = SimulationDatabase.ByIndex(Settings.Instance.ActiveConfiguration.DeviceIndex);

            var simulatedRectRelative = enableSimulation && selectedDevice != null
                ? NotchSimulatorUtility.CalculateSimulatorSafeAreaRelative(selectedDevice)
                : NotchSolutionUtility.defaultSafeArea;
            var simulatedCutoutsRelative = enableSimulation && selectedDevice != null
                ? NotchSimulatorUtility.CalculateSimulatorCutoutsRelative(selectedDevice)
                : NotchSolutionUtility.defaultCutouts;

            var normalSceneSimTargets = FindObjectsOfType<MonoBehaviour>().OfType<INotchSimulatorTarget>();
            foreach (var nst in normalSceneSimTargets)
            {
                nst.SimulatorUpdate(simulatedRectRelative, simulatedCutoutsRelative);
            }

            //Now find one in the prefab mode scene as well
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                var prefabSceneSimTargets = prefabStage.stageHandle.FindComponentsOfType<MonoBehaviour>()
                    .OfType<INotchSimulatorTarget>();
                foreach (var nst in prefabSceneSimTargets)
                {
                    nst.SimulatorUpdate(simulatedRectRelative, simulatedCutoutsRelative);
                }
            }
        }

        /// <summary>
        ///     We lose all events on entering play mode, use this to register the event and also make a canvas again
        ///     after it was destroyed by the event (that now disappeared)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AddOverlayInPlayMode()
        {
            UpdateAllMockups();
        }

        /// <summary>
        ///     This is called even if Notch Simulator tab is not present on the screen.
        ///     Also have to handle if we reload scripts while in prefab mode.
        /// </summary>
        [DidReloadScripts]
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
                if (EditorApplication.isPlaying)
                {
                    Destroy(mockup.gameObject);
                }
                else
                {
                    DestroyImmediate(mockup.gameObject, false);
                }
            }
        }

        internal static void UpdateAllMockups()
        {
            //When building, the scene may open-close multiple times and brought back the mockup canvas,
            //which combined with bugs mentioned at https://github.com/5argon/NotchSolution/issues/11,
            //will fail the build. This `if` prevents mockup refresh while building.
            if (BuildPipeline.isBuildingPlayer)
            {
                return;
            }

            EnsureCanvasAndEventSetup();

            //Make the editing environment contains an another copy of mockup canvas.
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                EnsureCanvasAndEventSetup(prefabStage);
            }

            var settings = Settings.Instance;
            var enableSimulation = settings.EnableSimulation;
            var selectedDevice = SimulationDatabase.ByIndex(Settings.Instance.ActiveConfiguration.DeviceIndex);
            var landscape = Settings.Instance.ActiveConfiguration.Orientation == PreviewOrientation.LandscapeLeft ||
                            Settings.Instance.ActiveConfiguration.Orientation == PreviewOrientation.LandscapeRight;

            if (enableSimulation && selectedDevice != null)
            {
                var screen = selectedDevice.Screens.FirstOrDefault();
                GameViewResolution.SetResolution(landscape ? screen.height : screen.width,
                    landscape ? screen.width : screen.height,
                    Settings.Instance.ActiveConfiguration.GameViewSize == GameViewSizePolicy.MatchAspectRatio);

                var name = selectedDevice.Meta.overlay;
                Sprite mockupSprite = null;
                if (!string.IsNullOrEmpty(name))
                {
                    var filePath = Path.Combine(NotchSimulatorUtility.DevicesFolder, name);
                    mockupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(filePath);
                    if (mockupSprite == null)
                    {
                        if (File.Exists(filePath))
                        {
                            var tex = new Texture2D(1, 1);
                            tex.LoadImage(File.ReadAllBytes(filePath));
                            mockupSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                                new Vector2(0.5f, 0.5f), 100.0f);
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"No mockup image named {name} in {NotchSimulatorUtility.DevicesFolder} folder!");
                        }
                    }
                }

                foreach (var mockup in AllMockupCanvases)
                {
                    mockup.UpdateMockupSprite(
                        mockupSprite,
                        NotchSimulatorUtility.GetGameViewOrientation(),
                        enableSimulation,
                        settings.FlipOrientation,
                        Settings.Instance.PrefabModeOverlayColor
                    );
                }
            }
            else
            {
                DestroyHiddenCanvas();
                GameViewResolution.ClearResolution();
            }
        }

        private static void DebugTransitions(string s)
        {
#if NOTCH_SOLUTION_DEBUG_TRANSITIONS
            Debug.Log(s);
#endif
        }


        /// <param name="prefabStage">
        ///     If not <c>null</c>, look for the mockup canvas on environment scene
        ///     for editing a prefab <b>instead</b> of normal scenes.
        /// </param>
        private static void EnsureCanvasAndEventSetup(PrefabStage prefabStage = null)
        {
            //Create the hidden canvas if not already.
            var prefabMode = prefabStage != null;
            var selectedMockupCanvas = prefabMode ? prefabMockupCanvas : mockupCanvas;

            if (selectedMockupCanvas == null)
            {
                //Find existing in the case of assembly reload
                //For some reason GameObject.FindObjectOfType could not get the canvas on main scene, it is active also, but by name works...
                var canvasObject = prefabMode
                    ? prefabStage.stageHandle.FindComponentOfType<MockupCanvas>()
                    : GameObject.Find(mockupCanvasName)?.GetComponent<MockupCanvas>();
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
                    var mockupCanvasPrefab =
                        AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuids.First()));

                    var instantiated =
                        prefabMode
                            ? (GameObject) PrefabUtility.InstantiatePrefab(mockupCanvasPrefab, prefabStage.scene)
                            : (GameObject) PrefabUtility.InstantiatePrefab(mockupCanvasPrefab);

                    //It sometimes instantiated into null on script reloading when starting Unity?
                    if (instantiated != null)
                    {
                        canvasObject = instantiated.GetComponent<MockupCanvas>();
                        instantiated.hideFlags = overlayCanvasFlag;

                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(canvasObject);
                        }

                        canvasObject.PrefabStage = prefabMode;
                        if (prefabMode)
                        {
                            prefabMockupCanvas = canvasObject;
                        }
                        else
                        {
                            mockupCanvas = canvasObject;
                        }
                    }

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
                        PrefabStage.prefabStageOpened += ps =>
                        {
                            DebugTransitions(
                                $"Prefab opening {ps.scene.GetRootGameObjects().First().name} {ps.prefabContentsRoot.name}");

                            //On open prefab, the "dont save" objects on the main scene will disappear too.
                            //So that we could still see it in the game view WHILE editing a prefab, we make it back.
                            //Along with this the prefab mode canvas will also be updated.
                            UpdateAllMockups();

                            //On entering prefab mode, the Notch Simulator panel did not get OnGUI().
                            UpdateSimulatorTargets();
                        };

                        PrefabStage.prefabStageClosing += ps =>
                        {
                            DebugTransitions(
                                $"Prefab closing {ps.scene.GetRootGameObjects().First().name} {ps.prefabContentsRoot.name}");
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
                            DebugTransitions(
                                $"Changed state PLAY {EditorApplication.isPlaying} PLAY or WILL CHANGE {EditorApplication.isPlayingOrWillChangePlaymode}");
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
                                    DestroyHiddenCanvas(); //Clean up the DontSave canvas we made in edit mode.
                                    break;
                                case PlayModeStateChange.ExitingPlayMode:
                                    DebugTransitions($"Exiting Play {canvasObject}");
                                    DestroyHiddenCanvas(); //Clean up the DontSave canvas we made in play mode.
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}