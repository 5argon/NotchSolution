# Adaptation Components

For non-uGUI objects, sometimes there are controls waiting to be raycasted by something like `Physics2DRaycaster` to trigger the `EventSystem` just like UI objects. Notches could make those objects difficult to touch, but this time we have no help from uGUI layout system to move things out of the way.

How "adaptation" components works is that it could move things out of the way with help from animation Playables API. My design is that we will have 2 **single-frame** `AnimationClip` that represent "normal state" and "fully adapted state". These clips could control just about anything keyable, and the evaluated value could be blended anywhere between 2 clips.

Moreover this control is not counted as dirtying the game object, which is perfect. If you use adaptation components on prefabs, it will not cause an override and dirty asterisk while you preview the adaptation via notch simulator/aspect switch.

Playables API requires an `Animator` component, however the controller asset is not needed so just add an empty `Animator`. (It must be `.enable = true` though.) Unfortunately without the controller asset you cannot use Animation tab to design the 2 clips I mentioned, so in the end you will need to connect a dummy controller asset to `Animator` just so you could design the clips, then you can remove it once you are done to prevent animation state machine creation at runtime.

Default to all adaptation components, it adapts once automatically only on `Start()` since unlike uGUI's layout system where all the edges and sizes cause ripple effect, it is not expected that the value controlling the adaptation will change often. (Aspect ratio couldn't change mid-game, notch couldn't expand mid-game, etc.) You could still adapt again normally with manual `Adapt()` call.

It also has one `AnimationCurve` mapper called the "adaptation curve". It should evaluates to 0 to 1, where 0 means you get a normal state clip, and 1 means you get fully-adapted clip. Now about the input to this curve, it depends on what kind of adaptation component will provide what value.

## <img src="Icons/AspectRatioAdaptationIcon.png" width="30"> AspectRatioAdaptation

An adaptation where the time value for the adaptation curve came from screen aspect ratio number. The number is always assumed to be width/height regardless of game's orientation, so for example, it is always 1.3333 (4/3) for iPad. You could use this to **indirectly** fix notch problem, without knowing if the notch exists or not. (Things respond to aspect ratio and not the notch.)

![WithoutAspectAdapt](.Documentation/images/woaspadapt.gif)

This gif demonstrate the problem of non uGUI objects when the aspect ratio changes. The camera shrink horizontally on narrower device when on portrait orientation. Before, I prepared a stage so that on iPad there are non-gameplay extra spaces around and could be cropped safely.

(It is not even safe anymore nowadays since I assumed the narrowest was 16:9 in the past, now it goes beyond 2:1 and clips even more edge out of the stage.)

However this assumption is no longer safe with an arrival of notch trend, since narrower device *horizontally* could possibly have lesser *vertical* space. It could make the control at top and bottom edges difficult to use.

With `AspectRatioAdaptation`, I could dynamically change *anything* according to aspect ratio number. Notice that nothing on the scene moves with notch on or off, because it adapts to aspect ratio and **indirectly** fix notch problem.

![AspectAdapt](.Documentation/images/aspadapt.gif)

In this example, in addition to the camera narrowing normally by Unity, I want it move a bit backwards with `AspectRatioAdaptation` when it gets narrower. Because of perspective settings, moving a camera backwards will scale everything down, make room on the top and bottom, and in turn create a space for notch. We have avoided the notch without querying for an existence of notch this way.

Additionally, you see that the stage itself expands vertically a bit too as a finishing touch. This affects gameplay a bit but overall looks nicer on narrow device than using the same stage shape as iPad aspect ratio.

The 2 clips on the camera in this example are each just a single frame keyed as `z = 13` for normal state and `z = 16` for fully-adapted state. The `adaptationCurve` is set so that time 1.3333 evaluates to 0, and time 2.1667 evaluates to 1. And 1.3333 is an iPad's aspect ratio, so it get fully normal state clip. If you use iPhone SE which has 16:9 ratio (1.7778), you would get somewhere between these 2 clips. (2.1667 is calculated from LG G7's ratio, I think it is the narrowest/longest phone right now.) You can see at the corner that `z` is really -13 when the game view is on iPad aspect.

## <img src="Icons/SafeAreaAdaptationIcon.png" width="30"> SafeAreaAdaptation

This is like `AspectRatioAdaptation` but this time we could adapt directly to **relative screen space taken** by a **single side** of safe area. (More documentation later when I arrived at the part in my game that actually use this, so I could make some screenshots.)