using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    /// A base class which use the first frame of 2 <see cref="AnimationClip"/>, and a blend value, to control an <see cref="Animator"/>
    /// with Playables API once on `Start()`. Those information are all in <see cref="BlendedClipsAdaptor"/> serialized together with this class.
    /// </summary>
    /// <remarks>
    /// You can create other kinds of adaptation component by subclassing this.
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(Animator))]
    public abstract class AdaptationBase : MonoBehaviour, INotchSimulatorTarget
    {
#pragma warning disable 0649
        [SerializeField] private SupportedOrientations supportedOrientations;

        [Space]

        /// <summary>
        /// Holds 2 <see cref="AnimationClip"/> that will be used when the game has one possible orientation, 
        /// or portrait orientation when the game could handle both portrait and landscape orientation.
        /// </summary>
        [SerializeField] private BlendedClipsAdaptor portraitOrDefaultAdaptation;

        /// <summary>
        /// Holds 2 <see cref="AnimationClip"/> that will be used when in landscape orientation 
        /// when the game could handle both portrait and landscape orientation.
        /// </summary>
        [SerializeField] private BlendedClipsAdaptor landscapeAdaptation;
#pragma warning restore 0649

#if UNITY_EDITOR
        internal void AssignAdaptationClips(AnimationClip normalState, AnimationClip adaptedState, bool forPortrait)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            bca.AssignAdaptationClips(normalState, adaptedState);
        }

        /// <summary>
        /// Check if both clips are nested on the same controller asset or not.
        /// </summary>
        internal bool TryGetLinkedControllerAsset(bool forPortrait, out RuntimeAnimatorController controllerAsset)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.TryGetLinkedControllerAsset(out controllerAsset);
        }

        internal bool IsAdaptable(bool forPortrait)
        {
            BlendedClipsAdaptor bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.Adaptable;
        }
#endif

        /// <summary>
        /// Make the adaptation curve reset to the specified <paramref name="curve">.
        /// </summary>
        private protected void ResetAdaptationToCurve(AnimationCurve curve)
        {
            portraitOrDefaultAdaptation = new BlendedClipsAdaptor(curve);
            landscapeAdaptation = new BlendedClipsAdaptor(curve);
        }

        private BlendedClipsAdaptor SelectedAdaptation =>
            supportedOrientations == SupportedOrientations.Dual ?
                NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ?
                    landscapeAdaptation : portraitOrDefaultAdaptation
            : portraitOrDefaultAdaptation;

        private Animator AnimatorComponent => GetComponent<Animator>();

        /// <summary>
        /// Drive <see cref="Animator"/> with playable graph, based on a blend between the first frame of 2 <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="valueForAdaptationCurve">Adaptation curve evaluates this value into a 0-1 blend between 2 clips.</param>
        protected void Adapt(float valueForAdaptationCurve) => SelectedAdaptation.Adapt(valueForAdaptationCurve, AnimatorComponent);

        void Start() => Adapt();

        void Update()
        {
            if(Application.isPlaying == false)
            {
                Adapt();
            }
        }

        /// <summary>
        /// Any adaptation component will be "adapted" **only once** on `Start()`, but
        /// this call could make that happen again on-demand.
        /// </summary>
        /// <remarks>
        /// This should ended up calling <see cref="Adapt(float)"/> somehow in the implementation.
        /// </remarks>
        public abstract void Adapt();

        private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;
        private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;

        void INotchSimulatorTarget.SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            this.storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            this.storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            Adapt();
        }

        /// <summary>
        /// Provide safe area in 0~1 value related to the screen size, already taken account of simulated or runtime value.
        /// </summary>
        protected Rect SafeAreaRelative
            => NotchSolutionUtility.ShouldUseNotchSimulatorValue ? storedSimulatedSafeAreaRelative : NotchSolutionUtility.ScreenSafeAreaRelative;

    }

}