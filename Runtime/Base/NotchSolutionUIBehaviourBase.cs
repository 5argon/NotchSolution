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

        //INotchSimulatorTarget
        public void SimulatorUpdate(Rect simulatedSafeArea, Rect[] simulatedCutouts)
        {
            UpdateRectBase();
        }

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
