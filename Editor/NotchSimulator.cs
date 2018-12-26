using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using System;

namespace E7.NotchSolution
{
    public class NotchSimulator : EditorWindow
    {
        [MenuItem("Window/General/Notch Simulator")]

        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow(typeof(NotchSimulator));
            win.titleContent= new GUIContent("Notch Simulator");
        }

        void OnGUI()
        {
            bool isEnableSimulation = NotchSimulatorUtility.enableSimulation;
            NotchSimulatorUtility.enableSimulation = EditorGUILayout.BeginToggleGroup("Simulate", NotchSimulatorUtility.enableSimulation);
            NotchSimulatorUtility.selectedDevice = (SimulationDevice)EditorGUILayout.EnumPopup(NotchSimulatorUtility.selectedDevice);
            EditorGUILayout.EndToggleGroup();

            if (isEnableSimulation || (!isEnableSimulation && NotchSimulatorUtility.enableSimulation))
            {
                SafeAreaPadding.SimulateSafeAreaRelative = NotchSimulatorUtility.enableSimulation ? NotchSimulatorUtility.SimulatorSafeAreaRelative : new Rect(0, 0, 1, 1);

                var nps = GameObject.FindObjectsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
                foreach (var np in nps)
                {
                    np.SimulatorUpdate();
                }
            }
        }
    }
}