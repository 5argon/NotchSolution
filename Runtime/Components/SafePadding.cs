//#define DEBUG_NOTCH_SOLUTION

using UnityEngine;
#if DEBUG_NOTCH_SOLUTION
using System.Linq;
#endif

namespace E7.NotchSolution
{
    /// <summary>
    ///     <para>
    ///         Make the <see cref="RectTransform"/> of object with this component
    ///         driven into full stretch to its immediate parent, then apply padding according to
    ///         device's reported <see cref="Screen.safeArea"/>.
    ///     </para>
    ///     <para>
    ///         Therefore makes an area inside this object safe for input-receiving components.
    ///         Then it is possible to make other objects safe area responsive by anchoring their
    ///         positions to this object's edges while being a child object.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Safe area defines an area on the phone's screen where it is safe to place your game-related input
    ///         receiving components without colliding with other on-screen features on the phone.
    ///         Usually this means it is also "visually safe" as all possible notches should be outside of safe area.
    ///     </para>
    ///     <para>
    ///         The amount of padding is a <see cref="Screen.safeArea"/> interpolated into <see cref="RectTransform"/>
    ///         of root <see cref="Canvas"/> found traveling up from this object. It should be a direct child of
    ///         top canvas, or deeper child of some similarly full stretch rect in order to look right,
    ///         although in reality it just pad in the shape of <see cref="Screen.safeArea"/>
    ///         regardless of its parent rectangle size.
    ///     </para>
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [HelpURL("https://exceed7.com/notch-solution/components/ui-behaviour/safe-padding.html")]
    public class SafePadding : NotchSolutionUIBehaviourBase
    {
        protected override void UpdateRect()
        {
            var selectedOrientation =
                orientationType == SupportedOrientations.Dual
                    ? NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ? landscapePaddings :
                    portraitOrDefaultPaddings
                    : portraitOrDefaultPaddings;

            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform,
                (LockSide(selectedOrientation.left) ? DrivenTransformProperties.AnchorMinX : 0) |
                (LockSide(selectedOrientation.right) ? DrivenTransformProperties.AnchorMaxX : 0) |
                (LockSide(selectedOrientation.bottom) ? DrivenTransformProperties.AnchorMinY : 0) |
                (LockSide(selectedOrientation.top) ? DrivenTransformProperties.AnchorMaxY : 0) |
                (LockSide(selectedOrientation.left) && LockSide(selectedOrientation.right)
                    ? DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.AnchoredPositionX
                    : 0) |
                (LockSide(selectedOrientation.top) && LockSide(selectedOrientation.bottom)
                    ? DrivenTransformProperties.SizeDeltaY | DrivenTransformProperties.AnchoredPositionY
                    : 0)
            );

            bool LockSide(EdgeEvaluationMode saem)
            {
                switch (saem)
                {
                    case EdgeEvaluationMode.On:
                    case EdgeEvaluationMode.Balanced:
                    case EdgeEvaluationMode.Off:
                        return true;
                    //When "Unlocked" is supported, it will be false.
                    default:
                        return false;
                }
            }

            //Lock the anchor mode to full stretch first.

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            var topRect = GetCanvasRect();
            var safeAreaRelative = SafeAreaRelative;

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"Top {topRect} safe {safeAreaRelative} min {safeAreaRelative.xMin} {safeAreaRelative.yMin}");
#endif

            var relativeLDUR = new Vector4(
                safeAreaRelative.xMin,
                safeAreaRelative.yMin,
                1 - (safeAreaRelative.yMin + safeAreaRelative.height),
                1 - (safeAreaRelative.xMin + safeAreaRelative.width)
            );

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"SafeLDUR {string.Join(" ", relativeLDUR.Select(x => x.ToString()))}");
#endif

            // fixed: sometimes relativeLDUR will be NAN when start at some android devices. 
            // if relativeLDUR is NAN then sizeDelta will be NAN, the safe area will be wrong.
            if (float.IsNaN(relativeLDUR[0]))
            {
                relativeLDUR[0] = 0;
            }

            if (float.IsNaN(relativeLDUR[1]))
            {
                relativeLDUR[1] = 0;
            }

            if (float.IsNaN(relativeLDUR[2]))
            {
                relativeLDUR[2] = 0;
            }

            if (float.IsNaN(relativeLDUR[3]))
            {
                relativeLDUR[3] = 0;
            }

            var currentRect = rectTransform.rect;

            //TODO : Calculate the current padding relative, to enable "Unlocked" mode. (Not forcing zero padding)
            var finalPaddingsLDUR = new Vector4();

            switch (selectedOrientation.left)
            {
                case EdgeEvaluationMode.On:
                    finalPaddingsLDUR[0] = topRect.width * relativeLDUR[0];
                    break;
                case EdgeEvaluationMode.Balanced:
                    finalPaddingsLDUR[0] = relativeLDUR[3] > relativeLDUR[0]
                        ? topRect.width * relativeLDUR[3]
                        : topRect.width * relativeLDUR[0];
                    break;
            }

            switch (selectedOrientation.right)
            {
                case EdgeEvaluationMode.On:
                    finalPaddingsLDUR[3] = topRect.width * relativeLDUR[3];
                    break;
                case EdgeEvaluationMode.Balanced:
                    finalPaddingsLDUR[3] = relativeLDUR[0] > relativeLDUR[3]
                        ? topRect.width * relativeLDUR[0]
                        : topRect.width * relativeLDUR[3];
                    break;
            }

            switch (selectedOrientation.bottom)
            {
                case EdgeEvaluationMode.On:
                    finalPaddingsLDUR[1] = topRect.height * relativeLDUR[1];
                    break;
                case EdgeEvaluationMode.Balanced:
                    finalPaddingsLDUR[1] = relativeLDUR[2] > relativeLDUR[1]
                        ? topRect.height * relativeLDUR[2]
                        : topRect.height * relativeLDUR[1];
                    break;
            }

            switch (selectedOrientation.top)
            {
                case EdgeEvaluationMode.On:
                    finalPaddingsLDUR[2] = topRect.height * relativeLDUR[2];
                    break;
                case EdgeEvaluationMode.Balanced:
                    finalPaddingsLDUR[2] = relativeLDUR[1] > relativeLDUR[2]
                        ? topRect.height * relativeLDUR[1]
                        : topRect.height * relativeLDUR[2];
                    break;
            }

            //Apply influence to the calculated padding
            finalPaddingsLDUR[0] *= influence;
            finalPaddingsLDUR[1] *= influence;
            finalPaddingsLDUR[2] *= influence;
            finalPaddingsLDUR[3] *= influence;

            if (flipPadding)
            {
                float remember = 0;
                finalPaddingsLDUR[0] = remember;
                finalPaddingsLDUR[0] = finalPaddingsLDUR[3];
                finalPaddingsLDUR[3] = remember;

                finalPaddingsLDUR[1] = remember;
                finalPaddingsLDUR[1] = finalPaddingsLDUR[2];
                finalPaddingsLDUR[2] = remember;
            }

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"FinalLDUR {string.Join(" ", finalPaddingsLDUR.Select(x => x.ToString()))}");
#endif

            //Combined padding becomes size delta.
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = -(finalPaddingsLDUR[0] + finalPaddingsLDUR[3]);
            sizeDelta.y = -(finalPaddingsLDUR[1] + finalPaddingsLDUR[2]);
            rectTransform.sizeDelta = sizeDelta;

            //The rect remaining after subtracted the size delta.
            var rectWidthHeight = new Vector2(topRect.width + sizeDelta.x, topRect.height + sizeDelta.y);

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"RectWidthHeight {rectWidthHeight}");
#endif

            //Anchor position's answer is depending on pivot too. Where the pivot point is defines where 0 anchor point is.
            var zeroPosition =
                new Vector2(rectTransform.pivot.x * topRect.width, rectTransform.pivot.y * topRect.height);
            var pivotInRect = new Vector2(rectTransform.pivot.x * rectWidthHeight.x,
                rectTransform.pivot.y * rectWidthHeight.y);

#if DEBUG_NOTCH_SOLUTION
            Debug.Log($"zeroPosition {zeroPosition}");
#endif

            //Calculate like zero position is at bottom left first, then diff with the real zero position.
            rectTransform.anchoredPosition3D = new Vector3(
                finalPaddingsLDUR[0] + pivotInRect.x - zeroPosition.x,
                finalPaddingsLDUR[1] + pivotInRect.y - zeroPosition.y,
                rectTransform.anchoredPosition3D.z);
        }
#pragma warning disable 0649
        [SerializeField]
        private SupportedOrientations orientationType;

        [SerializeField]
        private PerEdgeEvaluationModes portraitOrDefaultPaddings;

        [SerializeField]
        private PerEdgeEvaluationModes landscapePaddings;

        [Tooltip("Scale down the resulting value read from an edge to be less than an actual value.")]
        [SerializeField] [Range(0f, 1f)]
        private float influence = 1;

        [Tooltip(
            "The value read from all edges are applied to the opposite side of a RectTransform instead. Useful when you have rotated or negatively scaled RectTransform.")]
        [SerializeField]
        private bool flipPadding;
#pragma warning restore 0649
    }
}