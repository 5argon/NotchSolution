#define NOTCH_SOLUTION_DEBUG_TRANSITIONS

using UnityEditor;

namespace E7.NotchSolution
{
    public static class NotchSimulatorPrefs
    {
        [SettingsProvider]
        static SettingsProvider Pref()
        {
            return new SettingsProvider("Preferences/Notch Solution", SettingsScope.User, new string[] { "UI" })
            {
                guiHandler = DrawPref,
            };
        }

        static void DrawPref(string search)
        {
        }
    }
}