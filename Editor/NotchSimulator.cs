using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;

namespace E7.NotchSolution
{
    public class NotchSimulator : EditorWindow
    {
        [MenuItem("Window/General/Notch Simulator")]

        public static void ShowWindow()
        {
            var win = EditorWindow.GetWindow(typeof(NotchSimulator));
            win.name = "Notch Simulator";
        }

        void OnGUI()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = 18;
            EditorGUILayout.LabelField("Notch Simulator", style, GUILayout.Height(30));
            EditorGUILayout.Separator();

            bool isEnableSimulation = NotchSolutionUtility.enableSimulation;
            NotchSolutionUtility.enableSimulation = EditorGUILayout.BeginToggleGroup("Enable Simulation", NotchSolutionUtility.enableSimulation);

            EditorGUILayout.EndToggleGroup();

            if (isEnableSimulation || (!isEnableSimulation && NotchSolutionUtility.enableSimulation))
            {
                var nps = GameObject.FindObjectsOfType<UIBehaviour>().OfType<INotchSimulatorTarget>();
                foreach (var np in nps)
                {
                    np.SimulatorUpdate();
                }
            }
        }
    }
}