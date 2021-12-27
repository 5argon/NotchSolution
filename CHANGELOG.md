# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2021-12-31

### Changed

- Minimum version is now 2019.4 LTS, eliminating all Unity version scripting defines in the code. (Previously, .1 is the minimum, .2 enables `cutouts` API, and .3 enables Unity device simulator integration.) Being on LTS version positions this package for better future. This is a **breaking change** for those on lower than 2019.4 LTS.
- Author name in `package.json` changed from "Sirawat Pitaksarit / 5argon - Exceed7 Experiments" to just "Exceed7 Experiments". I found that Unity Package Manager has a drop down that group by this string. Now it is consistent with my other works and is also shorter.
- `Unity.ugui` removed from `E7.NotchSolution.asmdef` and `E7.NotchSolution.Editor.asmdef`. It is no longer needed to be explicitly referenced in 2019.4. (Thank god)
- XML code documentation refactors. API tab in the Website is also reflected.

### Fixed

Merged many PR fixes from contributors in the GitHub repository over the years where I took a break from this package. Thank you!

- [Notch Simulator automatically resizes Game window and configurable device orientation in Notch Simulator Window #54](https://github.com/5argon/NotchSolution/pull/54). (Contributed by [yasirkula](https://github.com/yasirkula).)
- [Fix GC allocation (96 byte) #55](https://github.com/5argon/NotchSolution/pull/55). (Contributed by [JohannesDeml](https://github.com/JohannesDeml).)
- [Gracefully handle a null mockupImage on reload #57](https://github.com/5argon/NotchSolution/pull/57). (Contributed by [BenWoodford](https://github.com/BenWoodford).)
- [Fix safe area calculations for Unity Standalone builds #60](https://github.com/5argon/NotchSolution/pull/60). (Contributed by [jollyMZ](https://github.com/jollyMZ).)
- [Fix bug on Unity 2019.3 where SafePadding Editor Labels (Left, Bottom,Top and Right) were not being shown! #63](https://github.com/5argon/NotchSolution/pull/63). (Contributed by [wagenheimer](https://github.com/wagenheimer).)
- [Handle new Unity Device Simulator window name #64](https://github.com/5argon/NotchSolution/pull/64). (Contributed by [robinnorth](https://github.com/robinnorth).)

## [1.9.0] - 2019-11-25

### Added

- Asset Store version is now available! It's been almost a year since start, right now I think I am quite confident about the package.
- New dedicated website http://exceed7.com/notch-solution which sync with the also new offline `Documentation~` hidden folder of this package.
- The debug scene is now compatible with UPM's import samples button.

### Changed

- Massive documentation structure change. They are now able to be used with [DocFx](https://dotnet.github.io/docfx/) while still quite readable from code editor. (But instead we could make a static site or even a PDF now.)
- XML triple slash code comments update across all files so DocFx could generate an online documentation correctly.
- Access modifiers revised, many wrong `public` are now properly `internal` for example. It also make DocFx generates only what the user should care about. If you were using some of those wrong `public` your code may break, however.
- The debug scene is moved to `/Samples~` to support [UPM sample importing](https://forum.unity.com/threads/samples-in-packages-manual-setup.623080/). (To maintainers : remove `~` when you want to edit, then add back when finished.) More scene which will actually be a sample of how to build the UI will be added in this proper format. Tutorial is also added to [the website](http://exceed7.com/notch-solution/simulator/debug-scene.html).
- `LICENSE` changed to `LICENSE.md` because otherwise the "license" link in UPM will not link to it.

### Fixed

- Fixed a problem that Device Simulator was simulating `Screen.currentResolution` instead of `Screen.width/height` and Notch Solution components were looking to `Screen.width/height` and getting wrong modifications.

## [1.8.0] - 2019-10-30

### Added

- Configuration system : you are given 2 configurations for each build platform. In each one it can remember its own overlay. This is used in a workflow where you constantly check between two extreme setups (e.g. iPhone - iPad) while iterating on the game UI.
- It now use asset file in `ProjectSettings` instead of `EditorPrefs`. Settings is now persisted per-project. Code related to `EditorPrefs` has been refactored everywhere.
- Device database migrated to definition file based in the same format as Unity Device Simulator package in 2019.3 is using. (Contributed by [06Games](https://github.com/06Games))

### Removed

- Alt+M to switch Game View between 2 settings has been removed along with `EditorPrefs`. This funtionality will be added back to the new configuration system later.

## [1.7.0] - 2019-10-24

### Added

- Added support for 2019.3 in general.
- Added support for [Unity Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest). If we detected that, all the components that wants a safe area simply ignores Notch Simulator simulated values and go back to follow `Screen.safeArea` just like in runtime, but it is now modified in edit time from inside-out by Device Simulator package. The package is 2019.3 or above only.
- Galaxy Note 10 Plus device overlay added.  (Contributed by [06Games](https://github.com/06Games))

## [1.6.0] - 2019-06-22 

### Added

- Add an editor button for creating all required assets for adaptation components.
- Add an editor button for temporarily assign the controller asset so you could design the adaptation in Animation panel. (Usable with asset created from the prior button.)
- Documentation structure refactored. (Contributed by [06Games](https://github.com/06Games))

## [1.5.0] - 2019-06-20 

### Added

- "Adaptation" components added. This series of components could adapt anything including non-UI stuff with help from **animation playables API**. This is needed because with `SafePadding` we were relying on uGUI layout system to move the entire hierarchy tree, and it is not usable outside of uGUI. The adaptation component is more flexible, as long as something is animation keyable, it could be responsive to a presence of notch & other things.
    - `SafeAdaptation` :  Adapt according to relative screen space taken by a selectable single side of safe area.
    - `AspectRatioAdaptation` : Like `SafeAdaptation` but use aspect ratio number of the screen. Doesn't sounds notch-related, but it is a part of "notch solution" because I have use this together to solve arrangement problems.
- New script icon for `SafePadding` since I lose the original vector project file, and I want the newly made component icons to look similar to it. ([Affinity Designer](https://affinity.serif.com/en-gb/designer/) icon project file is also added to `Icons` folder in the repo now to prevent losing it again.)
- Cutout information propagated to layout components, but currently unused.  (Contributed by [06Games](https://github.com/06Games))

### Fixed

- Code refactor throughout.
- Sometimes the overlay mockup didn't update its overlay graphic. Added a new destroy-create routine to force refresh it. (Contributed by [06Games](https://github.com/06Games))

## [1.4.0] - 2019-06-04

### Added

![preference](Documentation~/simulator/images/pref.png)

- Preference item added under "Notch Solution". You can adjust overlay color of prefab mode there.
- New shortcut to quick switch between 2 Game view aspect ratio. Set this up in the preference menu. This is ideal for mobile development where if you could ensure that narrowest and widest screen looks nice, everything in-between should also work.
- Overlay for Samsung Galaxy S10 and S10+ added. (Contributed by [06Games](https://github.com/06Games))
- Tree simulation device enum selector for 2019.2+ (Contributed by [06Games](https://github.com/06Games))

## [1.3.0] - 2019-05-28

### Added

- `influence` added to `SafePadding`, it is default to 1 that means the safe area taking full effect. It applies to all sides.
- "Dual Orientation" choice now only show up in Inspector if in your Player Settings, Resolution and Presentation section, you have Orientation settings in a way that it is possible to get both portrait and landscape orientation.
- Shortcut for toggling notch simulation added with the new `UnityEditor.ShortcutManagement` shortcut API. Bound to `Alt+N` by default.
- Cutout database for all available devices. Although they are not used yet currently.

### Fixed

- Removed `[ExecuteInEditMode]` from `SafePadding`.
- Simulation database for One Plus 6T and Huawei Mate 20 Pro was incorrect. It is now updated according to submitted debug data in [this thread](https://github.com/5argon/NotchSolution/issues/2).
- `HideFlags` mistake fixed.

## [1.2.0] - 2019-05-18

### Added

- Prefab mode ([`PrefabStage`](https://docs.unity3d.com/ScriptReference/Experimental.SceneManagement.PrefabStage.html)) suppport added. There is now an overlay while editing a prefab, so you could design "full screen canvas as a prefab" in isolation while preview the notch. Previously you must save the prefab first, the see the update in Game tab while Scene tab is still in prefab mode. Note that `PrefabStage` is in experimental namespace, it is bound to break in future version.
- New simulation device : iPad Pro. From running screen query on the Xcode simulator, the iPad Pro do have a safe area of 40px for that black line at the bottom (both orientations), plus small curved corners.
- Added a changelog.

### Fixed

- Aspect ratio number in the Notch Simulator warning help box is rounded to nice number.
- `SafePadding`'s delayed update is now using `WaitForEndOfFrame` instead of wait for the next frame.

## [1.1.0] - 2019-02-11

This is not an actual version since I just keep a changelog starting from 1.2.0, but I will list notable changes here off the top of my head.

### Added

- Debug scene added. It is now possible to distribute a test APK to collect cutout data of various phone. 2019.2's `Screen.cutouts` is supported in the debug scene, but no actual use by the `SafePadding` yet.
- Overlay color matches Personal and Professional skin.
- Two new devices: Huawei Mate 20 Pro & OnePlus 6T. (Contributed by [06Games](https://github.com/06Games))
- Added a warning about wrong Game tab aspect not matching notch preview device.
- Added a `README.md` documentation.

### Fixed

- `OnEnable` of `SafePadding` changed to delayed update. (Contributed by [Froghut](https://github.com/Froghut))
- Screen ratio function fixed. (Contributed by [06Games](https://github.com/06Games))
- Get the correct root canvas when you have multiple nested `Canvas` on the hierarchy. (Contributed by [mmatvein](https://github.com/mmatvein))


## [1.0.0] - 2018-12-27

The first version!!
