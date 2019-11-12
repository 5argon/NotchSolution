using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace E7.NotchSolution
{
    /// <summary>
    /// A base class to derive from if you want to make a notch-aware <see cref="UIBehaviour"/> component.
    /// 
    /// <see cref="UpdateRect"/> will be called at the "correct moment".
    /// You change the <see cref="rectTransform"/> as you like in there.
    /// </summary>
    /// <remarks>
    /// It helps you store the simulated values from Notch Simulator and expose them as `protected` fields.
    /// 
    /// Plus you can use <see cref="GetCanvasRect"/> to travel to the closest <see cref="Canvas"/> that is 
    /// this component's parent. Usually you will want to do something related to the "entire screen".
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class NotchSolutionUIBehaviourBase : UIBehaviour, ILayoutSelfController, INotchSimulatorTarget
    {
        private protected abstract void UpdateRect();

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
        void INotchSimulatorTarget.SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
        {
            this.storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
            this.storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
            UpdateRectBase();
        }

        /// <summary>
        /// Already taken account whether should trust Notch Simulator or Unity's [Device Simulator package](https://docs.unity3d.com/Packages/com.unity.device-simulator@latest/).
        /// </summary>
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

        /// <summary>
        /// Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            DelayedUpdate();
        }

        /// <summary>
        /// Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        /// <summary>
        /// Overrides <see cref="UIBehaviour"/>
        /// 
        /// This doesn't work when flipping the orientation to opposite side (180 deg). It only works for 90 deg. rotation because that
        /// makes the rect transform changes dimension.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRectBase();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
        }

        /// <summary>
        /// Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                DelayedUpdate();
            }
        }
#endif
        void ILayoutController.SetLayoutHorizontal()
        {
            UpdateRectBase();
        }

        void ILayoutController.SetLayoutVertical()
        {
        }

        private void UpdateRectBase()
        {
            if (!(enabled && gameObject.activeInHierarchy)) return;
            UpdateRect();
        }

        private WaitForEndOfFrame eofWait = new WaitForEndOfFrame();

        private void DelayedUpdate() => StartCoroutine(DelayedUpdateRoutine());
        private IEnumerator DelayedUpdateRoutine()
        {
            yield return eofWait;
            UpdateRectBase();
        }
    }
}
