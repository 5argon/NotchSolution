using System;
using UnityEngine;

namespace E7.NotchSolution
{
    [Serializable]
    public class AdaptationByAnimationClips
    {
#pragma warning disable 0649
        [SerializeField] internal AnimationClip normalState;
        [SerializeField] internal AnimationClip fullyAdaptedState;

        [Tooltip("A curve which maps to the current blend percentage between normal and adapted state it should be (Y axis, 0 to 1, 1 for fully adapted state")]
        [SerializeField] internal AnimationCurve adaptationCurve;
#pragma warning restore 0649
    }

}