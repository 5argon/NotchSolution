using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace E7.NotchSolution.Editor
{
    internal class NotchSolutionPreferences : SettingsProvider
    {

        public NotchSolutionPreferences(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        public override void OnTitleBarGUI()
        {
            EditorGUI.LabelField(new Rect(100, 100, 100, 100), "Notch Solution");
        }

        public override void OnActivate(string searchContext, VisualElement root)
        {
            var padRect = new VisualElement();
            int padding = 10;
            padRect.style.paddingBottom = padding;
            padRect.style.paddingLeft = padding;
            padRect.style.paddingRight = padding;
            padRect.style.paddingTop = padding - 5;
            root.Add(padRect);

            var title = new Label("Notch Solution");
            title.style.fontSize = 15;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            padRect.Add(title);

            var repo = new Label("https://github.com/5argon/NotchSolution");
            repo.style.paddingBottom = 15;
            repo.style.unityFontStyleAndWeight = FontStyle.Bold;
            repo.RegisterCallback((MouseDownEvent ev) =>
            {
                System.Diagnostics.Process.Start("https://github.com/5argon/NotchSolution");
            });
            padRect.Add(repo);

            var devicesPath = new TextField("Device Directory");
            devicesPath.SetEnabled(false);
            devicesPath.value = NotchSimulatorUtility.DevicesFolder;
            padRect.Add(devicesPath);

            var colorField = new ColorField("Prefab mode overlay color");
            colorField.value = Settings.Instance.PrefabModeOverlayColor;
            colorField.RegisterValueChangedCallback(ev =>
            {
                var settings = Settings.Instance;
                settings.PrefabModeOverlayColor = ev.newValue;
                settings.Save();
                NotchSimulator.UpdateAllMockups();
                NotchSimulator.UpdateSimulatorTargets();
            });
            padRect.Add(colorField);
        }

        [SettingsProvider]
        public static SettingsProvider Pref()
        {
            return new NotchSolutionPreferences("Preferences/Notch Solution", SettingsScope.User, new string[] { "UI" });
        }

    }
}
