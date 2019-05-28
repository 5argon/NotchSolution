using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace E7.NotchSolution
{
    public class NotchSolutionPreferences : SettingsProvider
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

            var colorField = new ColorField("Prefab mode overlay color");
            colorField.value = NotchSolutionUtility.PrefabModeOverlayColor;
            colorField.RegisterValueChangedCallback(ev =>
            {
                NotchSolutionUtility.PrefabModeOverlayColor = ev.newValue;
                NotchSimulator.UpdateAllMockups();
                NotchSimulator.UpdateSimulatorTargets();
            });
            padRect.Add(colorField);

            string shortcutString = ShortcutManager.instance.GetShortcutBinding(NotchSolutionShortcuts.switchNarrowestWidestShortcut).ToString();
            string text = $"This feature allows you to press {shortcutString} to quick switch between 2 aspect ratio choices. Commonly I would often switch between the longest phone like LG G7 and widest device like an iPad as I design. The index is counted from the top choice where 0 equals Free Aspect.";

            var box = new Box();
            var helpBox = new Label(text);
            helpBox.style.whiteSpace = WhiteSpace.Normal;
            box.style.paddingTop = 5;
            box.style.paddingLeft = 5;
            box.style.paddingRight = 5;
            box.style.paddingBottom = 2;
            box.style.marginTop = 10;
            box.style.marginBottom = 10;
            box.Add(helpBox);
            padRect.Add(box);

            //TODO : Reflect to `GameViewSizes.instance` and request all sizes to draw a nicer drop down menu instead of index number.

            var narrowest = new IntegerField("Narrowest aspect index");
            narrowest.value = NotchSolutionUtility.NarrowestAspectIndex;
            narrowest.RegisterValueChangedCallback(ev =>
            {
                narrowest.value = Mathf.Max(0, ev.newValue); //go to -1 and the game view crashes..
                NotchSolutionUtility.NarrowestAspectIndex = ev.newValue;
                NotchSimulator.UpdateAllMockups();
                NotchSimulator.UpdateSimulatorTargets();
            });
            padRect.Add(narrowest);

            var widest = new IntegerField("Widest aspect index");
            widest.value = NotchSolutionUtility.WidestAspectIndex;
            widest.RegisterValueChangedCallback(ev =>
            {
                widest.value = Mathf.Max(0, ev.newValue);
                NotchSolutionUtility.WidestAspectIndex = ev.newValue;
                NotchSimulator.UpdateAllMockups();
                NotchSimulator.UpdateSimulatorTargets();
            });
            padRect.Add(widest);
        }

        [SettingsProvider]
        public static SettingsProvider Pref()
        {
            return new NotchSolutionPreferences("Preferences/Notch Solution", SettingsScope.User, new string[] { "UI" });
        }

    }
}
