using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace E7.NotchSolution
{
    /// <summary>
    /// A base class which use the first frame of 2 <see cref="AnimationClip"> and a blend value to control an <see cref="Animator">
    /// with Playables API once on <see cref="Start">. Those information are all in <see cref="AdaptationByAnimationClips">.
    /// </summary>

    //Don't know if it is possible or not? Multiple playables using 1 animator is possible but it wrecks the default state.
    //But also currently that you could look at 1 edge, it is difficult in some situation when you want to do 2 unrelated things
    //depending on different edge, on the same game object.
    [DisallowMultipleComponent] 

    [ExecuteAlways]
    [RequireComponent(typeof(Animator))]
    public abstract class AdaptationBase : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] protected SupportedOrientations supportedOrientations;

        [Space]

        [SerializeField] protected AdaptationByAnimationClips portraitOrDefaultAdaptation;
        [SerializeField] protected AdaptationByAnimationClips landscapeAdaptation;
#pragma warning restore 0649

        private AdaptationByAnimationClips SelectedAdaptation =>
            supportedOrientations == SupportedOrientations.Dual ?
                NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ?
                    landscapeAdaptation : portraitOrDefaultAdaptation
            : portraitOrDefaultAdaptation;

        private Animator Animator
        {
            get
            {
                return GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Drive <see cref="Animator"> with playable graph, based on a blend between the first frame of 2 <see cref="AnimationClip">.
        /// </summary>
        /// <param name="valueForAdaptationCurve">Adaptation curve evaluates this value into a 0~1 blend between 2 clips.</param>
        protected void Adapt(float valueForAdaptationCurve)
        {
            //Connect up a playable graph, evaluate once, then we're done with them.
            var adaptation = SelectedAdaptation;
            if(adaptation == null || adaptation.adaptationCurve == null || adaptation.normalState == null || adaptation.fullyAdaptedState == null)
            {
                return;
            }

            float blend = adaptation.adaptationCurve.Evaluate(valueForAdaptationCurve);

            PlayableGraph pg = PlayableGraph.Create("AdaptationGraph");
            pg.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var mixer = AnimationMixerPlayable.Create(pg, 2, normalizeWeights: true);
            //Not sure if the mixer should be "cross fade" like this, or should we do 0~1 weight over 1 weight?
            //But I think that's for AnimationLayerMixerPlayable ?
            mixer.SetInputWeight(inputIndex: 0, weight: 1 - blend);
            mixer.SetInputWeight(inputIndex: 1, weight: blend);


            var normalStateAcp = AnimationClipPlayable.Create(pg, adaptation.normalState);
            var fullyAdaptedStateAcp = AnimationClipPlayable.Create(pg, adaptation.fullyAdaptedState);
            pg.Connect(normalStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 0);
            pg.Connect(fullyAdaptedStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 1);

            var output = AnimationPlayableOutput.Create(pg, "AdaptationGraphOutput", Animator);
            output.SetSourcePlayable(mixer);

            pg.Evaluate();
            pg.Destroy();
        }

        void Start() => Adapt();

        void Update()
        {
            if(Application.isPlaying == false)
            {
                Adapt();
            }
        }

        /// <summary>
        /// This should ended up calling <see cref="Adapt(AdaptationByAnimationClips, float)"> somehow.
        /// </summary>
        public abstract void Adapt();

    }

}