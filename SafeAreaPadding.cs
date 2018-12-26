using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace E7.NotchSolution
{
    /// <summary>
    /// Make the panel into full stretch and apply padding to the panel according to reported `Screen.safeArea`.
    /// The `Screen.safeArea` will be interpolated into top level `RectTransform`'s size.
    /// Currently the thing with this component should be direct child of top canvas, or deeper child of some similarly full stretch rect.
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPadding : UIBehaviour, ILayoutSelfController, INotchSimulatorTarget
    {
        private Rect GetScreenSafeAreaRelative()
        {
            //Debug.Log($"{Screen.width} {Screen.height} {Screen.currentResolution.width} {Screen.currentResolution.height}");
#if UNITY_EDITOR
            return NotchSolutionUtility.enableSimulation ? NotchSolutionUtility.SimulatorSafeArea : new Rect(0, 0, 1, 1);
#else
            var screen = new Rect(0, 0, Screen.width, Screen.height);
            var pixelSafeArea = Screen.safeArea;
            return new Rect(
                pixelSafeArea.x / screen.width,
                pixelSafeArea.y / screen.height,
                pixelSafeArea.width / screen.width,
                pixelSafeArea.height / screen.height
            );
#endif
        }

        private Rect GetTopLevelRect()
        {
            Vector2 topRectSize = transform.root.GetComponent<RectTransform>().sizeDelta;
            return new Rect(Vector2.zero, topRectSize);
        }

        [SerializeField] SafeAreaPaddingOrientationType orientationType;
        [SerializeField] SafeAreaPaddingSides portraitOrDefaultPaddings;
        [SerializeField] SafeAreaPaddingSides landscapePaddings;

        [System.NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            DelayUpdate();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRect();
        }

        public void SimulatorUpdate()
        {
            UpdateRect();
        }

        private void UpdateRect()
        {
            if (!IsActive()) return;

            SafeAreaPaddingSides selectedOrientation = 
            orientationType == SafeAreaPaddingOrientationType.DualOrientation ?
            NotchSolutionUtility.GetCurrentOrientation() == ScreenOrientation.Landscape ?
            landscapePaddings : portraitOrDefaultPaddings
            : portraitOrDefaultPaddings;

            m_Tracker.Clear();
            m_Tracker.Add(this, rectTransform,
                (LockSide(selectedOrientation.left) ? DrivenTransformProperties.AnchorMinX : 0) |
                (LockSide(selectedOrientation.right) ? DrivenTransformProperties.AnchorMaxX : 0) |
                (LockSide(selectedOrientation.bottom) ? DrivenTransformProperties.AnchorMinY : 0) |
                (LockSide(selectedOrientation.top) ? DrivenTransformProperties.AnchorMaxY : 0) |
                (LockSide(selectedOrientation.left) && LockSide(selectedOrientation.right) ? (DrivenTransformProperties.SizeDeltaX | DrivenTransformProperties.AnchoredPositionX) : 0) |
                (LockSide(selectedOrientation.top) && LockSide(selectedOrientation.bottom) ? (DrivenTransformProperties.SizeDeltaY | DrivenTransformProperties.AnchoredPositionY) : 0)
            );

            bool LockSide(SafeAreaPaddingMode sapm)
            {
                switch (sapm)
                {
                    case SafeAreaPaddingMode.Safe:
                    case SafeAreaPaddingMode.SafeBalanced:
                    case SafeAreaPaddingMode.Zero:
                        return true;
                    //When "Unlocked" is supported, it will be false.
                    default:
                        return false;
                }
            }

            //Lock the anchor mode to full stretch first.

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            var topRect = GetTopLevelRect();
            var safeAreaRelative = GetScreenSafeAreaRelative();

            //Debug.Log($"Top {topRect} safe {safeAreaRelative} min {safeAreaRelative.xMin} {safeAreaRelative.yMin}");

            var safeAreaPaddingsRelativeLDUR = new float[4]
            {
                safeAreaRelative.xMin,
                safeAreaRelative.yMin,
                1 - (safeAreaRelative.yMin + safeAreaRelative.height),
                1 - (safeAreaRelative.xMin + safeAreaRelative.width),
            };

            //Debug.Log($"SafeLDUR {string.Join(" ", safeAreaPaddingsRelativeLDUR.Select(x => x.ToString()))}");

            var currentRect = rectTransform.rect;

            //TODO : Calculate the current padding relative, to enable "Unlocked" mode. (Not forcing zero padding)
            var finalPaddingsLDUR = new float[4]
            {
                0,0,0,0
            };

            switch (selectedOrientation.left)
            {
                case SafeAreaPaddingMode.Safe:
                    finalPaddingsLDUR[0] = topRect.width * safeAreaPaddingsRelativeLDUR[0];
                    break;
                case SafeAreaPaddingMode.SafeBalanced:
                    finalPaddingsLDUR[0] = safeAreaPaddingsRelativeLDUR[3] > safeAreaPaddingsRelativeLDUR[0] ?
                        topRect.width * safeAreaPaddingsRelativeLDUR[3] :
                        topRect.width * safeAreaPaddingsRelativeLDUR[0];
                    break;
            }

            switch (selectedOrientation.right)
            {
                case SafeAreaPaddingMode.Safe:
                    finalPaddingsLDUR[3] = topRect.width * safeAreaPaddingsRelativeLDUR[3];
                    break;
                case SafeAreaPaddingMode.SafeBalanced:
                    finalPaddingsLDUR[3] = safeAreaPaddingsRelativeLDUR[0] > safeAreaPaddingsRelativeLDUR[3] ?
                        topRect.width * safeAreaPaddingsRelativeLDUR[0] :
                        topRect.width * safeAreaPaddingsRelativeLDUR[3];
                    break;
            }

            switch (selectedOrientation.bottom)
            {
                case SafeAreaPaddingMode.Safe:
                    finalPaddingsLDUR[1] = topRect.height * safeAreaPaddingsRelativeLDUR[1];
                    break;
                case SafeAreaPaddingMode.SafeBalanced:
                    finalPaddingsLDUR[1] = safeAreaPaddingsRelativeLDUR[2] > safeAreaPaddingsRelativeLDUR[1] ?
                        topRect.height * safeAreaPaddingsRelativeLDUR[2] :
                        topRect.height * safeAreaPaddingsRelativeLDUR[1];
                    break;
            }

            switch (selectedOrientation.top)
            {
                case SafeAreaPaddingMode.Safe:
                    finalPaddingsLDUR[2] = topRect.height * safeAreaPaddingsRelativeLDUR[2];
                    break;
                case SafeAreaPaddingMode.SafeBalanced:
                    finalPaddingsLDUR[2] = safeAreaPaddingsRelativeLDUR[1] > safeAreaPaddingsRelativeLDUR[2] ?
                        topRect.height * safeAreaPaddingsRelativeLDUR[1] :
                        topRect.height * safeAreaPaddingsRelativeLDUR[2];
                    break;
            }

            //Debug.Log($"FinalLDUR {string.Join(" ", finalPaddingsLDUR.Select(x => x.ToString()))}");

            //Combined padding becomes size delta.
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = -(finalPaddingsLDUR[0] + finalPaddingsLDUR[3]);
            sizeDelta.y = -(finalPaddingsLDUR[1] + finalPaddingsLDUR[2]);
            rectTransform.sizeDelta = sizeDelta;

            //The rect remaining after subtracted the size delta.
            Vector2 rectWidthHeight = new Vector2(topRect.width + sizeDelta.x, topRect.height + sizeDelta.y);

            //Debug.Log($"RectWidthHeight {rectWidthHeight}");

            //Anchor position's answer is depending on pivot too. Where the pivot point is defines where 0 anchor point is.
            Vector2 zeroPosition = new Vector2(rectTransform.pivot.x * topRect.width, rectTransform.pivot.y * topRect.height);
            Vector2 pivotInRect = new Vector2(rectTransform.pivot.x * rectWidthHeight.x, rectTransform.pivot.y * rectWidthHeight.y);

            //Debug.Log($"zeroPosition {zeroPosition}");

            //Calculate like zero position is at bottom left first, then diff with the real zero position.
            rectTransform.anchoredPosition3D = new Vector3(
                finalPaddingsLDUR[0] + pivotInRect.x - zeroPosition.x,
                finalPaddingsLDUR[1] + pivotInRect.y - zeroPosition.y,
            rectTransform.anchoredPosition3D.z);

            rectTransform.rotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private IEnumerator DelayUpdate()
        {
            yield return null;
            UpdateRect();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            StartCoroutine(DelayUpdate());
        }
#endif

        public void SetLayoutHorizontal()
        {
            //Debug.Log($"WOW");
        }

        public void SetLayoutVertical()
        {
            //Debug.Log($"WOW2");
        }
    }
}