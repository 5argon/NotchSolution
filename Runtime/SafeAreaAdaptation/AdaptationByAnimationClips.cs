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

        [Tooltip("A curve which tells the current blend percentage between normal and adapted state it should be (Y axis, 0 to 1, 1 for fully adapted state), depending on input relative space taken by the safe area of a single side (X). Note that a device with a large cutout like iPhone X has it's screen height taken by only 0.0418 (4.18%) by the top cutout.")]
        [SerializeField] internal AnimationCurve adaptationCurve;
#pragma warning restore 0649
    }

}