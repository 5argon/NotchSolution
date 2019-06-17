using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace E7.NotchSolution
{
    /// <summary>
    /// Adapt anything according to how much a screen space was taken by a single side of safe area.
    /// It uses Playables API and animation playables to blend between **the first frame** of 2 <see cref="AnimationClip">, 
    /// which represent normal state and fully-adapted state. As long as something is keyable by the animation system, it could be adapted to the safe area.
    /// 
    /// Blend amount is 0 ~ 1, calculated from 0 ~ 1 relative screen space taken of a single side that is then
    /// evaluated through configurable <see cref="AdaptationByAnimationClips.adaptationCurve">.
    /// 
    /// By using animation system, the modification won't count as dirtying the scene which is great 
    /// when you want to switching the simulator on and off just to observe its effect.
    /// 
    /// Animator is needed for its field binding power but no need for the controller asset, because we don't need the costly state machine, 
    /// just like how <see cref="PlayableDirector"> works with animation playables. However since Unity can't design <see cref="AnimationClip"> on 
    /// the Animation pane without <see cref="Animation"> **with** controllers, you may need to temporarily add a controller when 
    /// you are making the clip + add clips as some states in the animation graph, then remove the controller once you finished making the clip...
    /// 
    /// At runtime it only take effect on `Start`, since safe area is not expected to change dynamically, and unlike uGUI layout system + <see cref="SafeAreaPadding">, 
    /// a frequent recalculation is not expected. Call <see cref="Adapt"> if you wish to apply the adaptation manually again.
    /// 
    /// In edit mode, it also apply on notch simulator update. So it is almost like the adaptation always lock your fields. 
    /// In real play it is possible to adjust these adapted fields later freely since it's only on `Start`.
    /// </summary>

    //Don't know if it is possible or not? Multiple playables using 1 animator is possible but it wrecks the default state.
    //But also currently that you could look at 1 edge, it is difficult in some situation when you want to do 2 unrelated things
    //depending on different edge, on the same game object.
    [DisallowMultipleComponent] 

    [RequireComponent(typeof(Animator))]
    [ExecuteAlways]
    [HelpURL("https://github.com/5argon/NotchSolution#safeareaadaptation")]
    public class SafeAreaAdaptation : MonoBehaviour, INotchSimulatorTarget
    {

        public Animator Animator
        {
            get{
                return GetComponent<Animator>();
            }
        }

#pragma warning disable 0649

        [SerializeField] RectTransform.Edge adaptToEdge;
        [SerializeField] SafeAreaEvaluationMode evaluationMode;
        [SerializeField] SupportedOrientations supportedOrientations;
        [Space]
        [SerializeField] AdaptationByAnimationClips portraitOrDefaultAdaptation;
        [SerializeField] AdaptationByAnimationClips landscapeAdaptation;

#pragma warning restore 0649

        private const float iPhoneXNotchHeightRelative = 0.05418718f;

        void Reset()
        {
            portraitOrDefaultAdaptation.adaptationCurve = AnimationCurve.Linear(0, 0, iPhoneXNotchHeightRelative, 1);
            landscapeAdaptation.adaptationCurve = AnimationCurve.Linear(0, 0, iPhoneXNotchHeightRelative, 1);
        }

        public void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative) 
            => AdaptWithRelativeSafeArea(simulatedSafeAreaRelative);

        void Start() => Adapt();

        void Update()
        {
            if(Application.isPlaying == false)
            {
                Adapt();
            }
        }

        /// <summary>
        /// At runtime <see cref="SafeAreaAdaptation"> only take effect on `Start`, since safe area is not expected to change dynamically, 
        /// and unlike uGUI and <see cref="SafeAreaPadding">  a frequent recalculation is not expected. 
        /// 
        /// This method applies that adaptation manually again.
        /// </summary>
        public void Adapt() => AdaptWithRelativeSafeArea(NotchSolutionUtility.SafeAreaRelative);

#if UNITY_EDITOR
        public float latestSimulatedSpaceTakenRelative;
#endif

        private void AdaptWithRelativeSafeArea(Rect relativeSafeArea)
        {
            float spaceTakenRelative = 0;

            if (evaluationMode != SafeAreaEvaluationMode.Zero)
            {
                switch (adaptToEdge)
                {
                    case RectTransform.Edge.Left: spaceTakenRelative = relativeSafeArea.xMin; break;
                    case RectTransform.Edge.Right: spaceTakenRelative = 1 - relativeSafeArea.xMax; break;
                    case RectTransform.Edge.Top: spaceTakenRelative = 1 - relativeSafeArea.yMax; break;
                    case RectTransform.Edge.Bottom: spaceTakenRelative = relativeSafeArea.yMin; break;
                }

                if (evaluationMode == SafeAreaEvaluationMode.SafeBalanced)
                {
                    switch (adaptToEdge)
                    {
                        case RectTransform.Edge.Left: spaceTakenRelative = Mathf.Max(spaceTakenRelative, 1 - relativeSafeArea.xMax); break;
                        case RectTransform.Edge.Right: spaceTakenRelative = Mathf.Max(spaceTakenRelative, relativeSafeArea.xMin); break;
                        case RectTransform.Edge.Top: spaceTakenRelative = Mathf.Max(spaceTakenRelative, relativeSafeArea.yMin); break;
                        case RectTransform.Edge.Bottom: spaceTakenRelative = Mathf.Max(spaceTakenRelative, 1 - relativeSafeArea.yMax); break;
                    }
                }
            }

#if UNITY_EDITOR
            latestSimulatedSpaceTakenRelative = spaceTakenRelative;
#endif

            AdaptationByAnimationClips selectedAdaptation =
            supportedOrientations == SupportedOrientations.Dual ?
                NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ?
                    landscapeAdaptation : portraitOrDefaultAdaptation
            : portraitOrDefaultAdaptation;

            float blend = selectedAdaptation.adaptationCurve.Evaluate(spaceTakenRelative);

            //Connect up a playable graph, evaluate once, then we're done with them.

            PlayableGraph pg = PlayableGraph.Create("AdaptationGraph");
            pg.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var mixer = AnimationMixerPlayable.Create(pg, 2, normalizeWeights: true);
            //Not sure if the mixer should be "cross fade" like this, or should we do 0~1 weight over 1 weight?
            //But I think that's for AnimationLayerMixerPlayable ?
            mixer.SetInputWeight(inputIndex: 0, weight: 1 - blend);
            mixer.SetInputWeight(inputIndex: 1, weight: blend);

            var normalStateAcp = AnimationClipPlayable.Create(pg, selectedAdaptation.normalState);
            var fullyAdaptedStateAcp = AnimationClipPlayable.Create(pg, selectedAdaptation.fullyAdaptedState);
            pg.Connect(normalStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 0);
            pg.Connect(fullyAdaptedStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 1);

            var output = AnimationPlayableOutput.Create(pg, "AdaptationGraphOutput", Animator);
            output.SetSourcePlayable(mixer);

            pg.Evaluate();
            pg.Destroy();

            //Debug.Log($"Evaluated! Got blend {blend} from {spaceTakenRelative} space taken (Relative safe area {relativeSafeArea.xMin} {relativeSafeArea.xMax} {relativeSafeArea.yMin} {relativeSafeArea.yMax})");
        }
    }

}