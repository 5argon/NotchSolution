using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    /// A base class which use the first frame of 2 <see cref="AnimationClip"> and a blend value to control an <see cref="Animator">
    /// with Playables API once on <see cref="Start">. Those information are all in <see cref="BlendedClipsAdaptor">.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Animator))]
    public abstract class AdaptationBase : MonoBehaviour, INotchSimulatorTarget
    {
#pragma warning disable 0649
        [SerializeField] protected SupportedOrientations supportedOrientations;

        [Space]

        [SerializeField] protected BlendedClipsAdaptor portraitOrDefaultAdaptation;
        [SerializeField] protected BlendedClipsAdaptor landscapeAdaptation;
#pragma warning restore 0649

#if UNITY_EDITOR
        public void AssignAdaptationClips(AnimationClip normalState, AnimationClip adaptedState, bool forPortrait)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            bca.AssignAdaptationClips(normalState, adaptedState);
        }

        /// <summary>
        /// Check if both clips are nested on the same controller asset or not.
        /// </summary>
        public bool TryGetLinkedControllerAsset(bool forPortrait, out RuntimeAnimatorController controllerAsset)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.TryGetLinkedControllerAsset(out controllerAsset);
        }

        public bool IsAdaptable(bool forPortrait)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.Adaptable;
        }
#endif

        private BlendedClipsAdaptor SelectedAdaptation =>
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
        protected void Adapt(float valueForAdaptationCurve) => SelectedAdaptation.Adapt(valueForAdaptationCurve, Animator);

        void Start() => Adapt();

        void Update()
        {
            if(Application.isPlaying == false)
            {
                Adapt();
            }
        }

        /// <summary>
        /// This should ended up calling <see cref="Adapt(BlendedClipsAdaptor, float)"> somehow.
        /// </summary>
        public abstract void Adapt();

        private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;
        private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;
        public void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            this.storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            this.storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            Adapt();
        }

        protected Rect SafeAreaRelative
            => NotchSolutionUtility.ShouldUseNotchSimulatorValue ? storedSimulatedSafeAreaRelative : NotchSolutionUtility.ScreenSafeAreaRelative;

    }

}