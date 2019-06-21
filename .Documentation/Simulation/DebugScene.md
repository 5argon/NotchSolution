[Back to the summary](../Simulation.md)

# Debug Scene

This scene displays all the information about the device (simulated or not).
It works in the editor as well as on physical devices.

The information returned consists of:
- Rectangle of the `safeArea`. This is padded from the edge of the screen.
- Rectangle of the `cutouts` (if Unity 2019.2 or later), this is the exact bound surrounding the notch, not just an overall padding like `safeArea`.
- As well as full of other information about the phone. It is a dump of everything from [`SystemInfo`](https://docs.unity3d.com/ScriptReference/SystemInfo.html).

![scene](../images/debugScene.png)

You could download the [latest APK](https://github.com/5argon/NotchSolution/releases/latest) of this scene or distribute that link to someone who has the device which you want to know its notch/cutouts information.