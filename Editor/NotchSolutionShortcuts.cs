using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace E7.NotchSolution
{
    internal static class NotchSolutionShortcuts
    {
        private const string notchSolutionPrefPrefix = "Notch Solution/";
        internal const string toggleSimulationShortcut = notchSolutionPrefPrefix + "Toggle Notch Simulator";
        internal const string switchNarrowestWidestShortcut = notchSolutionPrefPrefix + "Switch narrowest-widest aspect";

        /// <summary>
        /// Switch between narrowest and widest aspect specified in the preferences to validate design. Switch to narrowest if currently on neither aspects.
        /// </summary>
        [Shortcut(switchNarrowestWidestShortcut, null, KeyCode.M, ShortcutModifiers.Alt)]
        static void SwitchNarrowestWidest()
        {

            var gameView = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            var sizeIndex = gameView.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var window = EditorWindow.GetWindow(gameView);

            int currentIndex = (int)sizeIndex.GetValue(window);
            currentIndex = currentIndex == NotchSolutionUtility.NarrowestAspectIndex ? NotchSolutionUtility.WidestAspectIndex : NotchSolutionUtility.NarrowestAspectIndex;
            sizeIndex.SetValue(window, currentIndex, null);

            NotchSimulator.UpdateAllMockups();
            NotchSimulator.UpdateSimulatorTargets();
        }

        [Shortcut(toggleSimulationShortcut, null, KeyCode.N, ShortcutModifiers.Alt)]
        static void ToggleSimulation()
        {
            NotchSimulatorUtility.enableSimulation = !NotchSimulatorUtility.enableSimulation;
            NotchSimulator.UpdateAllMockups();
            NotchSimulator.UpdateSimulatorTargets();
            NotchSimulator.win?.Repaint();
        }
    }
}
