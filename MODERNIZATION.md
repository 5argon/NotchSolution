# Notch Solution — Unity 6.3 LTS Modernization Plan

## The one insight that changes everything

Notch Solution predates Unity's built-in Device Simulator. Back then, in-editor `UnityEngine.Screen.safeArea`/`cutouts` returned nothing useful, so the package built its **own** simulator: a Notch Simulator window, a JSON device database, a hidden overlay canvas, Game-view resolution hacking, and a value-push channel (`INotchSimulatorTarget`) to feed simulated rects into components.

Unity 6.3 provides `UnityEngine.Device.Screen` (and `Device.Application`, `Device.SystemInfo`). Its `safeArea` and `cutouts` return the **simulated device's** values when the built-in Device Simulator is active in the editor, and the **real** values in a build or plain Game view — automatically, same code path, no reflection, no pushing, and **nothing added to the scene**.

That makes almost the entire custom simulator subsystem obsolete, and it's exactly what the user wants: components that react to the built-in simulator for real, without modifying the scene.

## How the package works today

**Runtime components** (`SafePadding`, `SafeAdaptation`, `AspectRatioAdaptation`) derive from bases (`NotchSolutionUIBehaviourBase`, `AdaptationBase`) whose safe-area source is chosen by `NotchSolutionUtility.ShouldUseNotchSimulatorValue`:

- If the built-in simulator window is detected (via reflection + regex on the `SimulatorWindow` class name) → read `UnityEngine.Screen.safeArea`/`cutouts`.
- Otherwise → use simulated rects pushed in through `INotchSimulatorTarget.SimulatorUpdate()` by the custom Notch Simulator.

**Editor simulator subsystem** (`Editor/Simulator/`):

- `NotchSimulator` — an `EditorWindow` that reads a JSON device DB and pushes safe-area/cutout rects into every `INotchSimulatorTarget` in the scene and prefab stage.
- Spawns a hidden `MockupCanvas` GameObject (`HideAndDontSave`) into the scene / prefab stage to draw the device frame overlay — **this is the scene modification concern**. It's fragile enough that there's an `IPreprocessBuildWithReport` hack to force-destroy it so builds don't fail (issue #11).
- `GameViewResolution` — heavy reflection into internal `GameView`/`GameViewSizes` types to resize the Game view.
- `SimulationDatabase` + `*.device.json` — a hand-maintained device catalog duplicating what the built-in simulator already ships.

## What needs to be done

### 0. Repository restructuring — do this first
Right now the git repo root **is** the package (`package.json`, `Runtime/`, `Editor/`, etc. sit at the top level), so the repo can't be opened directly as a Unity project and Git-URL UPM points at the root. Adopt the UniTask layout: the repo hosts a real Unity project, and the package lives in a subfolder that the Git URL drills into.

UniTask does it as `UniTask/src/UniTask/Assets/Plugins/UniTask` (repo → Unity project → package), installed via `...UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`. We'll mirror that without the extra `Plugins` level:

```
NotchSolution/                      # 1) git repo root
  src/
    NotchSolution/                  # 2) Unity project — shows as "NotchSolution" in Hub
      Assets/
        NotchSolution/              # 3) the package — Git URL UPM points here
          package.json
          Runtime/
          Editor/
          Icons/
          Samples~/
          Documentation~/
          CHANGELOG.md
          LICENSE.md
          README.md
      Packages/
        manifest.json
      ProjectSettings/
  README.md                         # repo-level readme (install instructions, links)
  LICENSE.md
```

Concrete steps:
- Create `src/NotchSolution/` as a proper Unity project (6.3 LTS): `Assets/`, `Packages/manifest.json`, `ProjectSettings/`.
- Move all current package content into `src/NotchSolution/Assets/NotchSolution/`. Use `git mv` so history is preserved and `.meta` files stay paired with their assets.
- Keep a repo-root `README.md` (and `LICENSE.md`) for the GitHub landing page; the package keeps its own copies inside the package folder.
- Git URL for consumers becomes: `https://github.com/5argon/NotchSolution.git?path=src/NotchSolution/Assets/NotchSolution` (optionally `#<tag>` to pin a version).
- Update `.gitignore` for the Unity project (`Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`) while still committing `Assets/`, `Packages/`, `ProjectSettings/`.
- Optionally add the debug/sample scene into the project's `Assets/` (outside the package) so opening the repo project is immediately testable, keeping the distributable sample in the package's `Samples~/`.
- Update CI (`.github/`), the DocFX project paths, and any hardcoded asset paths (e.g. the `DevicesFolder` GUID lookup is GUID-based so it survives the move, but verify).

Do this before the code changes so the rest of the work happens inside a real, openable Unity 6.3 project.

### 1. Swap the runtime safe-area source to `UnityEngine.Device.Screen`
In `NotchSolutionUtility` (and `AspectRatioAdaptation`), replace `UnityEngine.Screen.safeArea`, `.cutouts`, `.currentResolution`, `.width`, `.height` with the `UnityEngine.Device.Screen` equivalents. Cleanest via alias directive at the top of each file:

```cs
using Screen = UnityEngine.Device.Screen;
```

This single change makes all components respond to the built-in Device Simulator live — no scene changes, no reflection.

### 2. Collapse the `ShouldUseNotchSimulatorValue` branch
With `Device.Screen`, there is no longer a "trust simulator vs. trust Screen" decision. `SafeAreaRelative` in the base classes just reads `Device.Screen`. Delete the reflection-based simulator-window detection in `NotchSolutionUtilityEditor`.

### 3. Decide the fate of the custom simulator + `INotchSimulatorTarget`
The push model is no longer needed for correctness. Recommended:
- Keep components fully self-sufficient via `Device.Screen`.
- **Deprecate** `INotchSimulatorTarget` and the value-push path (keep as no-op/obsolete for one release for API compatibility).
- Keep the Notch Simulator window only if it still earns its place (see #5); otherwise retire it.

### 4. Remove the hidden `MockupCanvas` scene overlay + build hack
This is the "modifies the scene" problem. Replace the instantiated `HideAndDontSave` canvas with a non-scene overlay — a Game/Scene view overlay (UI Toolkit `Overlay`) or a Device Simulator plugin panel — so nothing is ever added to the scene or prefab stage. Once gone, the `OnPreprocessBuild` destroy hack and issue #11 disappear entirely.

### 5. (The "integrate ours into it") Build a `DeviceSimulatorPlugin`
Unity 6.3 exposes `UnityEditor.DeviceSimulation.DeviceSimulatorPlugin`. Override `title` and `OnCreateUI()` (returning a `VisualElement`), with `OnCreate()` giving access to `deviceSimulator` events. Surface Notch Solution's controls — device-frame mockup toggle, per-edge evaluation preview, flip/influence — as a panel **inside the built-in Simulator Control Panel** instead of a separate window. This is the modern home for the package's editor UI.

### 6. Delete / trim the reflection layers
Once resolution and orientation come from the built-in simulator, most of this is dead code:
- `GameViewResolution` (internal Game view reflection) — remove.
- `NotchSimulatorUtility.GetMainGameViewSize` (reflection) — remove or replace with `Device.Screen`.
- `NotchSolutionUtilityEditor.PlayModeView`/`UnityDeviceSimulatorActive` reflection — remove.

### 7. Retire or repurpose the JSON device database
The built-in simulator ships its own device catalog (`.device` assets). The custom `SimulationDatabase` + `*.device.json` + mockup PNGs can be dropped, or kept only to feed the optional mockup-overlay feature. Prefer dropping to reduce maintenance.

### 8. Package manifest + assemblies
- `package.json`: bump `unity` to `6000.3` (Device.Screen has been in core since 2021.2, but target the stated LTS), bump `version` to `3.0.0`, update description.
- No `com.unity.device-simulator` package dependency needed — it's built into the editor.
- Assembly defs are clean; add references only if the plugin needs `UnityEditor.DeviceSimulation`.

### 9. Docs + CHANGELOG
- Rewrite `Documentation~/simulator/*` around the built-in Device Simulator workflow.
- Mark `INotchSimulatorTarget`, the Notch Simulator window, and the JSON DB as deprecated.
- Add a `3.0.0` CHANGELOG entry noting the breaking change (min Unity 6.3, custom simulator removed/deprecated).

### 10. Verify
- Safe area updates live when switching devices in the built-in simulator, with the scene staying **not dirty**.
- Builds contain no leftover mockup GameObject and don't error.
- Play mode and prefab-stage editing both behave.
- `SafePadding`, `SafeAdaptation`, `AspectRatioAdaptation` all track the simulated device.

## Effort / sequencing

1. **Restructure first:** #0 — move to the `src/NotchSolution/Assets/NotchSolution` layout so the repo opens as a Unity 6.3 project and Git-URL UPM drills into the package.
2. **Core (small, high value):** #1 + #2 + #8 — swap to `Device.Screen`, simplify base, bump manifest. Components immediately work with the built-in simulator.
3. **Cleanup (medium):** #3, #4, #6, #7 — remove scene overlay, push model, and reflection.
4. **Polish (optional, nice):** #5 + #9 — Device Simulator plugin panel and docs.

The simulation module can stay (as the user noted) — but it should be demoted to an optional mockup-overlay convenience, not the mechanism that drives components.
