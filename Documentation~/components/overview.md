# Components Overview

You will be using these components to solve notch problems. Currently they are categorized into 2 categories.

## [UIBehaviour Components](ui-behaviour/ui-behaviour-components.md)

Works with `RectTransform` tree of the uGUI component system.

- <img src="../../Icons/SafeAreaPaddingIcon.png" width="18"> [SafePadding](ui-behaviour/safe-padding.md): pad the `RectTransform` in based on the value returned by [`Screen.safeArea`](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html). If you anchor any child on its padded edges, then you are automatically safe.
- **(Planned)** `SafePosition` : Controls only the `anchoredPosition` of `RectTransform` such that it avoids *both* unsafe area and cutouts, by moving away *perpendicularly* from any selected edge. This is to be the first component that supports cutouts API introduced in 2019.2, but it works with 2019.1 too by trusting only the safe area.
- **(Planned)** `EdgeSplit` : In contrary to `SafePosition`'s perpendicular notch avoiding, this component try to solve the problem by moving in *parallel* along any selected edge. It controls both `anchoredPosition` and `sizeDelta` of two `RectTransform` such that they can split or join together depending on cutout position of the device. (Imagine splitted on iPhone X but joining on Galaxy S10+) This requires 2019.2, it will do nothing without cutout API.

## [Adaptation Components](adaptation/adaptation-components.md)

They are based on using [Playables API](https://docs.unity3d.com/ScriptReference/Playables.Playable.html) to control `GameObject` with animation playables, therefore utilizing `Animator` and `AnimationClip` instead of `RectTransform`.

- <img src="../../Icons/AspectRatioAdaptationIcon.png" width="18"> [AspectRatioAdaptation](adaptation/aspect-ratio-adaptation.md): Dynamically changes anything keyable by animation system, based on the ratio of the screen.
- <img src="../../Icons/SafeAreaAdaptationIcon.png" width="18"> [SafeAdaptation](adaptation/safe-adaptation.md): Dynamically changes anything keyable by animation system, based on the safe area.

## UIElements Components

In the far far future (2021.1+) when the [runtime `UIElements`](https://www.youtube.com/watch?v=t4tfgI1XvGs) is available, likely I will have to develop a UIE version of everything..