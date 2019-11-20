# UIBehaviour Components Overview

While not exactly performant, uGUI has always been quite extensible. By deriving from `UIBehaviour`, you gain several callbacks that uGUI will call while it is traversing the `GameObject` tree. You have to also implements a desired `interface` : `ILayoutController`, `ILayoutSelfController`, `ILayoutGroup`, `ILayoutIgnorer` so that your custom component take an appropriate role plugging into the layout system.

The layout script is nothing more than continuously setting `RectTransform` values, but thanks to [`DrivenRectTransformTracker`](https://docs.unity3d.com/ScriptReference/DrivenRectTransformTracker.html), the `RectTransform` UI properly greyed out and looks like the values are really controlled by the component. The tracker helps telling the editor that values changed this way does not count as dirtying the scene too.

So Notch Solution use this to control the `RectTransform` such that its content area is safe, not outside of safe area and not occluded by any cutouts.