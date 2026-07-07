# Notch Solution

A set of components and tools to solve notched/cutout phones layout problems for Unity UGUI.

**Official website:** http://exceed7.com/notch-solution/

## Repository layout

This repository is a Unity project that hosts the package in a subfolder, so the repo root is not the package itself (same approach as [UniTask](https://github.com/Cysharp/UniTask)). This lets you open the repo directly in the Unity Hub for development while still installing the package via a Git URL.

```
NotchSolution/                     # this git repo
  src/
    NotchSolution/                 # Unity project — open this folder in Unity Hub
      Assets/
        NotchSolution/             # the package — the Git URL UPM points here
      Packages/
      ProjectSettings/
```

## Install via UPM (Git URL)

In Unity, open **Window → Package Manager → + → Add package from git URL** and enter:

```
https://github.com/5argon/NotchSolution.git?path=src/NotchSolution/Assets/NotchSolution
```

To pin a specific version, append a tag, e.g.:

```
https://github.com/5argon/NotchSolution.git?path=src/NotchSolution/Assets/NotchSolution#3.0.0
```

Or add it directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.e7.notch-solution": "https://github.com/5argon/NotchSolution.git?path=src/NotchSolution/Assets/NotchSolution"
  }
}
```

## Development

Open `src/NotchSolution/` in the Unity Hub (Unity 6.3 LTS). The package source is embedded under `Assets/NotchSolution`, so you can edit and test it in place. The full package README, changelog, documentation, and samples live inside that package folder.

## License

[MIT](LICENSE.md)
