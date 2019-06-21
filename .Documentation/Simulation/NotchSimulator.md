[Back to the summary](../Simulation.md)

# Notch Simulator

![screenshot](../images/ssNotchSim.png)

Accessible from `Window > General > Notch Simulator`. Additionally with Shortcuts API introduced in 2019.1, you could press `Alt/Option + N` to toggle it to confirm your design. It could be adjusted in the `Shortcuts...` preference menu.

Works together with all `INotchSimulatorTarget` (`SafeAreaPadding` is one of them) in the current scene and prefab environment scene. Normally `Screen.safeArea` does not return a useful value in editor. Notch Simulator can simulate a safe area in editor for you **even in prefab mode**. You can toggle it on and off to see your UI reacts immediately.

Non-flipped orientation of landscape is assumed to be "landscape left" from natural portrait orientation.

## How it works

- The simulator maintains 2 `Canvas` game objects, one in the normal scene and one in the prefab environment scene. They are with `HideFlags` that make it invisible on Hierarchy and do not get saved.
- `AssetDatabase` search the plugin folder for the correct notch overlay image to put in that canvas. Portrait and landscape image is separated. (For example iPhoneX has a different bottom bar.)
- This `Canvas` is on "Screen Space - Overlay". with high sort order.
- You need to set the game view to match your simulation device choice or it would looks weird.
- Portrait or landscape orientation is determined from width vs height of the current game view's size. (Not by `Screen.` API, since that does not work in editor.)
- All `INotchSimulatorTarget` found will be sent a simulated `Rect`. Static access point to the latest simulated rect is also available with `NotchSolutionUtility.SimulateSafeAreaRelative` if passing around that `Rect` is a hassle. You could write your own extension that links with the simulator this way.

The overlay is also useful for aiming roughly what can fit in the corner around the notch, because you can see the notch's width and also rounded corner visually where safe area does not cover such information. (Safe area is a rectangle.) Although please note that Apple advise agaist intentionally trying to design on that gap.

2019.2's `Screen.cutouts` could precisely cover the cutout area, but no work has been put to utilize that yet.

## Preferences

There is a preference menu available. For example you could change prefab mode overlay color to be different from Game view ones, or enable some other extra toolings.

![preference](../images/pref.png)