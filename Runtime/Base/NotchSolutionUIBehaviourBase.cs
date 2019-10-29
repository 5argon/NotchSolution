//#define DEBUG_NOTCH_SOLUTION

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace E7.NotchSolution
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class NotchSolutionUIBehaviourBase : UIBehaviour, ILayoutSelfController, INotchSimulatorTarget
    {
        private protected Rect GetCanvasRect()
        {
            var topLevelCanvas = GetTopLevelCanvas();
            Vector2 topRectSize = topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
            return new Rect(Vector2.zero, topRectSize);

            Canvas GetTopLevelCanvas()
            {
                var canvas = this.GetComponentInParent<Canvas>();
                var rootCanvas = canvas.rootCanvas;
                return rootCanvas;
            }
        }

        private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;
        private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;
        public void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            this.storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            this.storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            UpdateRectBase();
        }

        protected Rect SafeAreaRelative
            => NotchSolutionUtility.ShouldUseNotchSimulatorValue ? storedSimulatedSafeAreaRelative : NotchSolutionUtility.ScreenSafeAreaRelative;

        [System.NonSerialized]
        private RectTransform m_Rect;
        private protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private protected DrivenRectTransformTracker m_Tracker;

        protected override void OnEnable()
        {
            base.OnEnable();
            DelayedUpdate();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        /// <summary>
        /// This doesn't work when flipping the orientation to opposite side (180 deg). It only works for 90 deg. rotation because that
        /// makes the rect transform changes dimension.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRectBase();
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
        }

        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                DelayedUpdate();
            }
        }
#endif


        //ILayoutController
        public void SetLayoutHorizontal()
        {
            //Simulator is already calling SimulatorUpdate but this could be useful in some edge cases?
            UpdateRectBase();
        }

        //ILayoutController
        public void SetLayoutVertical()
        {
        }

        private void UpdateRectBase()
        {
            if (!(enabled && gameObject.activeInHierarchy)) return;
            UpdateRect();
        }

        private protected abstract void UpdateRect();

        private WaitForEndOfFrame eofWait = new WaitForEndOfFrame();

        private void DelayedUpdate() => StartCoroutine(DelayedUpdateRoutine());
        private IEnumerator DelayedUpdateRoutine()
        {
            yield return eofWait;
            UpdateRectBase();
        }
    }
}
