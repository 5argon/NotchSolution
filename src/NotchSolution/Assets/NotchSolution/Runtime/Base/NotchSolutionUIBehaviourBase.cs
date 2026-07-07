using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace E7.NotchSolution
{
    /// <summary>
    ///     A base class to derive from if you want to make a notch-aware <see cref="UIBehaviour"/> component.
    ///     <see cref="UpdateRect"/> will be called at the "correct moment".
    ///     You change the <see cref="rectTransform"/> as you like in there.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="GetCanvasRect"/> to travel to the closest <see cref="Canvas"/> that is this component's
    ///     parent. Usually you will want to do something related to the "entire screen".
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class NotchSolutionUIBehaviourBase : UIBehaviour, ILayoutSelfController
    {
        private readonly WaitForEndOfFrame eofWait = new WaitForEndOfFrame();

        [NonSerialized]
        private RectTransform m_Rect;

        protected DrivenRectTransformTracker m_Tracker;

        /// <summary>
        ///     Safe area in 0~1 values relative to the screen size.
        /// </summary>
        protected Rect SafeAreaRelative => NotchSolutionUtility.ScreenSafeAreaRelative;

        protected RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }

                return m_Rect;
            }
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            DelayedUpdate();
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>
        /// </summary>
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        ///     This doesn't work when flipping the orientation to opposite side (180 deg).
        ///     It only works for 90 deg. rotation because that makes the rect transform changes dimension.
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            UpdateRectBase();
        }

        void ILayoutController.SetLayoutHorizontal()
        {
            UpdateRectBase();
        }

        void ILayoutController.SetLayoutVertical()
        {
        }

#if UNITY_EDITOR
        private Rect lastCheckedSafeArea;

        /// <summary>
        ///     In edit mode, re-run the layout when the reported safe area changes, for example when switching
        ///     devices in the Device Simulator. Running inside the player loop keeps the simulated
        ///     <see cref="UnityEngine.Device.Screen"/> values active.
        /// </summary>
        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var safeArea = SafeAreaRelative;
            if (safeArea == lastCheckedSafeArea)
            {
                return;
            }

            lastCheckedSafeArea = safeArea;
            UpdateRectBase();
        }
#endif

        protected abstract void UpdateRect();

        protected Rect GetCanvasRect()
        {
            var topLevelCanvas = GetTopLevelCanvas();
            var topRectSize = topLevelCanvas.GetComponent<RectTransform>().sizeDelta;
            return new Rect(Vector2.zero, topRectSize);

            Canvas GetTopLevelCanvas()
            {
                var canvas = GetComponentInParent<Canvas>();
                var rootCanvas = canvas.rootCanvas;
                return rootCanvas;
            }
        }

        private void UpdateRectBase()
        {
            if (!(enabled && gameObject.activeInHierarchy))
            {
                return;
            }

            UpdateRect();
        }

        private void DelayedUpdate()
        {
            StartCoroutine(DelayedUpdateRoutine());

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // In Edit Mode, coroutines don't always work but we also can't call UpdateRectBase directly because it spams warning messages
                // when called directly from OnValidate. So we can wait for 1 editor frame in EditorApplication.update instead
                UnityEditor.EditorApplication.update += DelayedEditorUpdate;

                void DelayedEditorUpdate()
                {
                    UnityEditor.EditorApplication.update -= DelayedEditorUpdate;
                    if (this == null) return;
                    UpdateRectBase();
                };
            }
#endif
        }

        private IEnumerator DelayedUpdateRoutine()
        {
            yield return eofWait;
            UpdateRectBase();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
        }

        /// <summary>
        ///     Overrides <see cref="UIBehaviour"/>.
        /// </summary>
        protected override void OnValidate()
        {
            if (gameObject.activeInHierarchy)
            {
                DelayedUpdate();
            }
        }
#endif
    }
}
