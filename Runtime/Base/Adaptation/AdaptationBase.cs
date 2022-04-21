using UnityEngine;

namespace E7.NotchSolution
{
    /// <summary>
    ///     A base class which use the first frame of 2 <see cref="AnimationClip"/>, and a blend value,
    ///     to control an <see cref="Animator"/> with Playables API once on `Start()`.
    ///     Those information are all in <see cref="BlendedClipsAdaptor"/> serialized together with this class.
    /// </summary>
    /// <remarks>
    ///     You can create other kinds of adaptation component by subclassing this.
    /// </remarks>
    [ExecuteAlways]
    [RequireComponent(typeof(Animator))]
    public abstract class AdaptationBase : MonoBehaviour, INotchSimulatorTarget
    {
        private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;

        private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;

        private BlendedClipsAdaptor SelectedAdaptation =>
            supportedOrientations == SupportedOrientations.Dual
                ? NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.LandscapeLeft ? landscapeAdaptation :
                portraitOrDefaultAdaptation
                : portraitOrDefaultAdaptation;

        private Animator AnimatorComponent => GetComponent<Animator>();

        /// <summary>
        ///     Provide safe area in 0~1 value related to the screen size,
        ///     already taken account of simulated or runtime value.
        /// </summary>
        protected Rect SafeAreaRelative
            => NotchSolutionUtility.ShouldUseNotchSimulatorValue
                ? storedSimulatedSafeAreaRelative
                : NotchSolutionUtility.ScreenSafeAreaRelative;

        private void Start()
        {
            Adapt();
        }

        private void Update()
        {
            if (Application.isPlaying == false)
            {
                Adapt();
            }
        }

        void INotchSimulatorTarget.SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            Adapt();
        }

        /// <summary>
        ///     Make the adaptation curve reset to the specified <paramref name="curve">.
        /// </summary>
        private protected void ResetAdaptationToCurve(AnimationCurve curve)
        {
            portraitOrDefaultAdaptation = new BlendedClipsAdaptor(curve);
            landscapeAdaptation = new BlendedClipsAdaptor(curve);
        }

        /// <summary>
        ///     Drive <see cref="Animator"/> with playable graph, based on a blend
        ///     between the first frame of 2 <see cref="AnimationClip"/>.
        /// </summary>
        /// <param name="valueForAdaptationCurve">
        ///     Adaptation curve evaluates this value into a 0-1 blend between 2 clips.
        /// </param>
        protected void Adapt(float valueForAdaptationCurve)
        {
            SelectedAdaptation.Adapt(valueForAdaptationCurve, AnimatorComponent);
        }

        /// <summary>
        ///     Any adaptation component will be "adapted" <b>only once</b> on <c>Start()</c>, but
        ///     this call could make that happen again on-demand.
        /// </summary>
        /// <remarks>
        ///     This should ended up calling <see cref="Adapt(float)"/> somehow in the implementation.
        /// </remarks>
        public abstract void Adapt();
#pragma warning disable 0649
        [SerializeField] private SupportedOrientations supportedOrientations;

        /// <summary>
        ///     Holds 2 <see cref="AnimationClip"/> that will be used when the game has one possible orientation,
        ///     or portrait orientation when the game could handle both portrait and landscape orientation.
        /// </summary>
        [Space]
        [SerializeField] private BlendedClipsAdaptor portraitOrDefaultAdaptation;

        /// <summary>
        ///     Holds 2 <see cref="AnimationClip"/> that will be used when in landscape orientation
        ///     when the game could handle both portrait and landscape orientation.
        /// </summary>
        [SerializeField] private BlendedClipsAdaptor landscapeAdaptation;
#pragma warning restore 0649

#if UNITY_EDITOR
        internal void AssignAdaptationClips(AnimationClip normalState, AnimationClip adaptedState, bool forPortrait)
        {
            var bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            bca.AssignAdaptationClips(normalState, adaptedState);
        }

        /// <summary>
        ///     Check if both clips are nested on the same controller asset or not.
        /// </summary>
        internal bool TryGetLinkedControllerAsset(bool forPortrait, out RuntimeAnimatorController controllerAsset)
        {
            var bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.TryGetLinkedControllerAsset(out controllerAsset);
        }

        internal bool IsAdaptable(bool forPortrait)
        {
            var bca = forPortrait ? portraitOrDefaultAdaptation : landscapeAdaptation;
            return bca.Adaptable;
        }
#endif
    }
}