[Back to the summary](../Simulation.md)

# Simulated Devices

## Simulation devices currently available

- iPhone X
- iPad Pro

(Contributed by [06Games](https://github.com/06Games))
- Huawei Mate 20 Pro 
- OnePlus 6T
- Samsung Galaxy S10
- Samsung Galaxy S10+

## How to add a new simulation device

- Download the [latest APK](https://github.com/5argon/NotchSolution/releases/latest) of the [Debug Scene](DebugScene.md) or build it yourself.
- Then install on your Android with file manager or `adb install -r` (r = replace). 
- After it runs, **rotate the device to both portrait and landscape** and take screenshots to remember informations.
- `Editor/SimulationDevice.cs` : Add a new `enum` to this file first. It would show up in the simulator with `enum` dropdown.
- `Editor/SimulationDatabase.cs` : A mapping from that `enum` to various information required.
- `Editor/Mockups` : Mockup overlay files are here. It should be named this pattern `NoSo-{enum}-{Portrait/Landscape}` and colored white/transparent. It can be in any resolution but needs to be aspect-correct as it will be stretched out to the `Canvas`.
- Submit a pull request! Thanks! : )