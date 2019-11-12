﻿using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.NotchSolution
{
    /// <summary>
    /// Holds 2 <see cref="AnimationClip"/>. With any weight `float` given,
    /// it performs an adaptation with Playables API in <see cref="Adapt(float, Animator)"/>, you
    /// provide the target <see cref="Animator"/> each time you use it.
    /// </summary>
    /// <remarks>
    /// It is an "adaptor" because it is the one that cause an adaptation.
    /// </remarks>
    [Serializable]
    internal class BlendedClipsAdaptor
    {
#pragma warning disable 0649
        [SerializeField] AnimationClip normalState;
        [SerializeField] AnimationClip fullyAdaptedState;

        [Tooltip("A curve which maps to the current blend percentage between normal and adapted state it should be (Y axis, 0 to 1, 1 for fully adapted state")]
        [SerializeField] AnimationCurve adaptationCurve;
#pragma warning restore 0649

#if UNITY_EDITOR
        public void AssignAdaptationClips(AnimationClip normalState, AnimationClip adaptedState)
        {
            this.normalState = normalState;
            this.fullyAdaptedState = adaptedState;
        }

        /// <summary>
        /// Check if both clips are nested on the same controller asset or not.
        /// </summary>
        public bool TryGetLinkedControllerAsset(out RuntimeAnimatorController controllerAsset) 
        {
            controllerAsset = null;
            var normalControllerPath = AssetDatabase.GetAssetPath(normalState);
            var adaptedControllerPath = AssetDatabase.GetAssetPath(fullyAdaptedState);
            bool linked = Adaptable && (normalControllerPath == adaptedControllerPath);
            
            if(linked)
            {
                controllerAsset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(normalControllerPath);
            }
            return linked;
        }
#endif

        public BlendedClipsAdaptor(AnimationCurve defaultCurve)
        {
            this.adaptationCurve = defaultCurve;
        }

        /// <summary>
        /// Check if all required data are not `null` : the curve, and the 2 <see cref="AnimationClip"/>.
        /// </summary>
        internal bool Adaptable => !(adaptationCurve == null || normalState == null || fullyAdaptedState == null);

        /// <summary>
        /// Use animation Playables API to control keyed values, blended between the first frame of 2 animation clips.
        /// </summary>
        /// <param name="valueForAdaptationCurve">A value to evaluate into adaptation curve producing a real blend value for 2 clips.</param>
        /// <param name="animator">The control target for animation playables. The clips used must be able to control the keyed fields traveling down from this animator component.</param>
        public void Adapt(float valueForAdaptationCurve, Animator animator)
        {
            if (!Adaptable) return;

            float blend = adaptationCurve.Evaluate(valueForAdaptationCurve);

            //Connect up a playable graph, evaluate once, then we're done with them.
            PlayableGraph pg = PlayableGraph.Create("AdaptationGraph");
            pg.SetTimeUpdateMode(DirectorUpdateMode.Manual);

            var mixer = AnimationMixerPlayable.Create(pg, 2, normalizeWeights: true);
            //Not sure if the mixer should be "cross fade" like this, or should we do 0~1 weight over 1 weight?
            //But I think that's for AnimationLayerMixerPlayable ?
            mixer.SetInputWeight(inputIndex: 0, weight: 1 - blend);
            mixer.SetInputWeight(inputIndex: 1, weight: blend);


            var normalStateAcp = AnimationClipPlayable.Create(pg, normalState);
            var fullyAdaptedStateAcp = AnimationClipPlayable.Create(pg, fullyAdaptedState);
            pg.Connect(normalStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 0);
            pg.Connect(fullyAdaptedStateAcp, sourceOutputPort: 0, mixer, destinationInputPort: 1);

            var output = AnimationPlayableOutput.Create(pg, "AdaptationGraphOutput", animator);
            output.SetSourcePlayable(mixer);

            pg.Evaluate();
            pg.Destroy();
        }
    }

}