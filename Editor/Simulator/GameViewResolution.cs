using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    public static class GameViewResolution
    {
        private const string TEMPORARY_RESOLUTION_LABEL = "Notch Simulator Device";
        private const string PREVIOUS_RESOLUTION_PREF = "NotchSimPrevRes";

        private static object m_customResolution;

        private static int mockupRefreshTime;

        private static object GameViewSizes =>
            GetType("GameViewSizes").FetchProperty("instance").FetchProperty("currentGroup");

        private static object FixedResolutionMode => Enum.Parse(GetType("GameViewSizeType"), "FixedResolution");
        private static object AspectRatioMode => Enum.Parse(GetType("GameViewSizeType"), "AspectRatio");

        private static object CustomResolution
        {
            get
            {
                if (m_customResolution != null)
                {
                    if ((int) GameViewSizes.CallMethod("IndexOf", m_customResolution) < 0)
                    {
                        m_customResolution = null;
                    }
                }
                else
                {
                    var totalSizeCount = (int) GameViewSizes.CallMethod("GetTotalCount");
                    var builtinSizeCount = (int) GameViewSizes.CallMethod("GetBuiltinCount");
                    for (var i = totalSizeCount - 1; i >= builtinSizeCount; i--)
                    {
                        var size = GameViewSizes.CallMethod("GetGameViewSize", i);
                        if ((string) size.FetchProperty("baseText") == TEMPORARY_RESOLUTION_LABEL)
                        {
                            m_customResolution = size;
                            break;
                        }
                    }
                }

                return m_customResolution;
            }
            set => m_customResolution = value;
        }

        private static int CustomResolutionIndex => m_customResolution == null
            ? -1
            : (int) GameViewSizes.CallMethod("IndexOf", m_customResolution) +
              (int) GameViewSizes.CallMethod("GetBuiltinCount");

        public static void SetResolution(int width, int height, bool aspectRatioMode)
        {
            var gameView = GetGameView();
            if (!gameView)
            {
                if (!NotchSolutionUtilityEditor.UnityDeviceSimulatorActive)
                {
                    var sceneView = SceneView.lastActiveSceneView
                        ? SceneView.lastActiveSceneView
                        : SceneView.currentDrawingSceneView;
                    if (sceneView)
                    {
                        sceneView.CallMethod("SetMainPlayModeViewSize", new Vector2(width, height));
                        RefreshMockups();
                    }
                }

                return;
            }

            var customResolution = CustomResolution;
            if (customResolution != null)
            {
                var isModified = false;

                if (customResolution.FetchProperty("sizeType").Equals(AspectRatioMode) != aspectRatioMode)
                {
                    customResolution.ModifyProperty("sizeType",
                        aspectRatioMode ? AspectRatioMode : FixedResolutionMode);
                    isModified = true;
                }

                if ((int) customResolution.FetchProperty("width") != width)
                {
                    customResolution.ModifyProperty("width", width);
                    isModified = true;
                }

                if ((int) customResolution.FetchProperty("height") != height)
                {
                    customResolution.ModifyProperty("height", height);
                    isModified = true;
                }

                if (!isModified)
                {
                    return;
                }

                if (CustomResolutionIndex == (int) gameView.FetchProperty("selectedSizeIndex"))
                {
                    ZoomOutGameView();
                    RefreshMockups();

                    return;
                }
            }
            else
            {
                CustomResolution = customResolution = GetType("GameViewSize")
                    .CreateInstance(aspectRatioMode ? AspectRatioMode : FixedResolutionMode, width, height,
                        TEMPORARY_RESOLUTION_LABEL);
                GameViewSizes.CallMethod("AddCustomSize", customResolution);
            }

            PlayerPrefs.SetInt(PREVIOUS_RESOLUTION_PREF, (int) gameView.FetchProperty("selectedSizeIndex"));
            gameView.CallMethod("SizeSelectionCallback", CustomResolutionIndex, null);

            ZoomOutGameView();
            RefreshMockups();
        }

        public static void ClearResolution()
        {
            if (CustomResolution != null)
            {
                var gameView = GetGameView();
                var customResolutionActive =
                    gameView && CustomResolutionIndex == (int) gameView.FetchProperty("selectedSizeIndex");

                GameViewSizes.CallMethod("RemoveCustomSize", CustomResolutionIndex);
                CustomResolution = null;

                if (customResolutionActive)
                {
                    gameView.CallMethod("SizeSelectionCallback",
                        Mathf.Clamp(PlayerPrefs.GetInt(PREVIOUS_RESOLUTION_PREF), 0,
                            (int) GameViewSizes.CallMethod("GetTotalCount") - 1), null);

                    ZoomOutGameView();
                    RefreshMockups();
                }
            }
        }

        private static EditorWindow GetGameView()
        {
            var windows = Resources.FindObjectsOfTypeAll(GetType("GameView"));
            return windows.Length > 0 ? (EditorWindow) windows[0] : null;
        }

        private static void ZoomOutGameView()
        {
            var gameView = GetGameView();

            // Find the currently active tab on Game view's panel
            var activeTab = gameView.FetchField("m_Parent")?.FetchProperty("actualView") as EditorWindow;

            gameView.Focus();
            gameView.CallMethod("UpdateZoomAreaAndParent");
            gameView.CallMethod("SnapZoom", (float) gameView.FetchProperty("minScale"));

            // Restore the active tab
            if (activeTab && activeTab != gameView)
            {
                activeTab.Focus();
            }
        }

        private static void RefreshMockups()
        {
            // Sometimes we need to respawn the mockups after a couple of frames in order to synchronize the mockups
            // with the new game view resolution
            if (!NotchSimulator.IsOpen)
            {
                mockupRefreshTime = 2;

                EditorApplication.update -= RefreshMockupsInternal;
                EditorApplication.update += RefreshMockupsInternal;
            }
        }

        private static void RefreshMockupsInternal()
        {
            if (--mockupRefreshTime <= 0)
            {
                EditorApplication.update -= RefreshMockupsInternal;
                NotchSimulator.RespawnMockup();
            }
        }

        #region Reflection Functions

        private static Type GetType(string type)
        {
            return typeof(EditorWindow).Assembly.GetType("UnityEditor." + type);
        }

        private static object FetchField(this Type type, string field)
        {
            return type.GetFieldRecursive(field, true).GetValue(null);
        }

        private static object FetchField(this object obj, string field)
        {
            return obj.GetType().GetFieldRecursive(field, false).GetValue(obj);
        }

        private static object FetchProperty(this Type type, string property)
        {
            return type.GetPropertyRecursive(property, true).GetValue(null, null);
        }

        private static object FetchProperty(this object obj, string property)
        {
            return obj.GetType().GetPropertyRecursive(property, false).GetValue(obj, null);
        }

        private static void ModifyProperty(this object obj, string property, object value)
        {
            obj.GetType().GetPropertyRecursive(property, false).SetValue(obj, value, null);
        }

        private static object CallMethod(this object obj, string method, params object[] parameters)
        {
            return obj.GetType().GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(obj, parameters);
        }

        private static object CreateInstance(this Type type, params object[] parameters)
        {
            Type[] parameterTypes;
            if (parameters == null)
            {
                parameterTypes = null;
            }
            else
            {
                parameterTypes = new Type[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameterTypes[i] = parameters[i].GetType();
                }
            }

            return type.GetConstructor(parameterTypes).Invoke(parameters);
        }

        private static FieldInfo GetFieldRecursive(this Type type, string field, bool isStatic)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |
                        (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            do
            {
                var fieldInfo = type.GetField(field, flags);
                if (fieldInfo != null)
                {
                    return fieldInfo;
                }

                type = type.BaseType;
            } while (type != null);

            return null;
        }

        private static PropertyInfo GetPropertyRecursive(this Type type, string property, bool isStatic)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly |
                        (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            do
            {
                var propertyInfo = type.GetProperty(property, flags);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                type = type.BaseType;
            } while (type != null);

            return null;
        }

        #endregion
    }
}