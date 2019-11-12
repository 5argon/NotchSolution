using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace E7.NotchSolution.Editor
{
    internal static class NotchSolutionShortcuts
    {
        private const string notchSolutionPrefPrefix = "Notch Solution/";
        internal const string toggleSimulationShortcut = notchSolutionPrefPrefix + "Toggle Notch Simulator";
        internal const string switchConfigurationShortcut = notchSolutionPrefPrefix + "Switch configuration";

        /// <summary>
        /// Switch between narrowest and widest aspect specified in the preferences to validate design. Switch to narrowest if currently on neither aspects.
        /// </summary>
        [Shortcut(switchConfigurationShortcut, null, KeyCode.M, ShortcutModifiers.Alt)]
        internal static void SwitchConfiguration()
        {
            Settings.Instance.NextConfiguration();

            NotchSimulator.Redraw();
            NotchSimulator.UpdateAllMockups();
            NotchSimulator.UpdateSimulatorTargets();

            // Using shortcut to change aspect ratio actually will not proc the [ExecuteAlways] Update() of adaptation components, unlike using the drop down.
            // But it mostly do so because we always have some uGUI components which indirectly cause those updates on ratio change.
            // While the scene with no uGUI at all maybe rare, it never hurts to proc them manually.. just in case.

            EditorApplication.QueuePlayerLoopUpdate();
        }

        [Shortcut(toggleSimulationShortcut, null, KeyCode.N, ShortcutModifiers.Alt)]
        static void ToggleSimulation()
        {
            var settings  = Settings.Instance;
            settings.EnableSimulation = !settings.EnableSimulation;
            settings.Save();
            NotchSimulator.UpdateAllMockups();
            NotchSimulator.UpdateSimulatorTargets();
            NotchSimulator.Redraw();
        }
    }
}
