using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using E7.NotchSolution;
using UnityEngine;
using UnityEngine.UI;

class NotchSolutionDebugger : MonoBehaviour, INotchSimulatorTarget
{
#pragma warning disable 0649
    public Text debugText;
    public GameObject debugRectPrefab;
    public RectTransform rootRect;
    public Transform export;
#pragma warning restore 0649

    private StringBuilder sb = new StringBuilder();

    public SimulationDevice device;

    public enum Menu { Home, Extracting }
    public Menu menu;

    private Rect storedSimulatedSafeAreaRelative = NotchSolutionUtility.defaultSafeArea;
    private Rect[] storedSimulatedCutoutsRelative = NotchSolutionUtility.defaultCutouts;
    public void SimulatorUpdate(Rect simulatedSafeAreaRelative, Rect[] simulatedCutoutsRelative)
    {
        this.storedSimulatedSafeAreaRelative = simulatedSafeAreaRelative;
        this.storedSimulatedCutoutsRelative = simulatedCutoutsRelative;
    }

    void Update()
    {
        sb.Clear();
        ClearRects();

        switch (menu)
        {
            case Menu.Home:
                export.gameObject.SetActive(true);
                sb.AppendLine($"<b>-- PLEASE ROTATE THE DEVICE TO GET BOTH ORIENTATION'S DETAILS! --</b>\n");

                var safeArea = RelativeToReal(NotchSolutionUtility.ShouldUseNotchSimulatorValue ? storedSimulatedSafeAreaRelative : NotchSolutionUtility.ScreenSafeAreaRelative);

                PlaceRect(safeArea, Color.red);
                if (Screen.orientation != NotchSolutionUtility.GetCurrentOrientation())
                    safeArea.Set(Screen.width - safeArea.x, Screen.height - safeArea.y, safeArea.width, safeArea.height);
                sb.AppendLine($"Safe area : {safeArea}\n");

#if UNITY_EDITOR
                var relativeCutouts = NotchSolutionUtility.ShouldUseNotchSimulatorValue ? storedSimulatedCutoutsRelative : NotchSolutionUtility.ScreenCutoutsRelative;
                List<Rect> rectCutouts = new List<Rect>();
                foreach (Rect rect in relativeCutouts) rectCutouts.Add(RelativeToReal(rect));
                var cutouts = rectCutouts.ToArray();
#else
                var cutouts = Screen.cutouts;
#endif
                foreach (Rect r in cutouts) PlaceRect(r, Color.blue);

                if (Screen.orientation != NotchSolutionUtility.GetCurrentOrientation())
                {
                    foreach (Rect rect in cutouts) rect.Set(Screen.width - rect.x, Screen.height - rect.y, rect.width, rect.height);
                }
                sb.AppendLine($"Cutouts : {string.Join(" / ", cutouts.Select(x => x.ToString()))} \n");

                sb.AppendLine($"Current resolution : {Screen.currentResolution}\n");
                sb.AppendLine($"All Resolutions : {string.Join(" / ", Screen.resolutions.Select(x => x.ToString()))}\n");
                sb.AppendLine($"DPI : {Screen.dpi} WxH : {Screen.width}x{Screen.height} Orientation : {Screen.orientation}\n");
                var joinedProps = string.Join(" / ", typeof(SystemInfo).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => $"{x.Name} : {x.GetValue(null)}"));
                sb.AppendLine(joinedProps);

                break;
            case Menu.Extracting:
                var screen = device.Screens.FirstOrDefault();
                export.gameObject.SetActive(false);

                if (screen.orientations.Count == 4)
                {
                    string path = Application.persistentDataPath + "/" + device.Meta.friendlyName + ".device.json";
                    System.IO.File.WriteAllText(path, JsonUtility.ToJson(device));
                    sb.AppendLine("<b>Done</b>");
                    sb.AppendLine("");
                    sb.AppendLine($"File saved at <i>{path}</i>");
                    StartCoroutine(exportDone());
                }
                else sb.AppendLine("Extracting...");

                break;
        }
        debugText.text = sb.ToString();
    }

    Rect RelativeToReal(Rect relative)
    {
        return new Rect(
            relative.x * Screen.width,
            relative.y * Screen.height,
            relative.width * Screen.width,
            relative.height * Screen.height
        );
    }

    private List<DebugRect> debugRects = new List<DebugRect>();
    public void ClearRects()
    {
        foreach (var dbr in debugRects) Destroy(dbr.gameObject);
        debugRects.Clear();
    }

    public void PlaceRect(Rect rct, Color c)
    {
        var go = Instantiate(debugRectPrefab, rootRect);
        go.transform.localScale = Vector3.one;
        //go.transform.SetAsFirstSibling();
        var dbr = go.GetComponent<DebugRect>();
        dbr.PlaceItselfAtScreenRect(rct, c);
        debugRects.Add(dbr);
    }

    public void Export()
    {
        device = new SimulationDevice();

        device.Meta = new MetaData();
        device.Meta.friendlyName = export.GetComponentInChildren<InputField>().text;

        device.SystemInfo = new SystemInfoData() { GraphicsDependentData = new GraphicsDependentSystemInfoData[1] { new GraphicsDependentSystemInfoData() } };
        foreach (var property in typeof(SystemInfo).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            var prop = typeof(SystemInfoData).GetField(property.Name);
            if (prop != null) prop.SetValue(device.SystemInfo, property.GetValue(null));
            else
            {
                prop = typeof(GraphicsDependentSystemInfoData).GetField(property.Name);
                if (prop != null) prop.SetValue(device.SystemInfo.GraphicsDependentData[0], property.GetValue(null));
            }
        }

        device.Screens = new ScreenData[1];
        for (int i = 0; i < device.Screens.Length; i++)
        {
            var screen = new ScreenData();
            screen.width = Screen.width;
            screen.height = Screen.height;
            //screen.navigationBarHeight = 0;
            screen.orientations = new Dictionary<ScreenOrientation, OrientationDependentData>();
            screen.dpi = Screen.dpi;
            device.Screens[i] = screen;
            StartCoroutine(screenData(screen));
        }

        menu = Menu.Extracting;
    }

    IEnumerator screenData(ScreenData screen)
    {
        var orientation = Screen.orientation;
        for (int i = 1; i < 5; i++)
        {
            Screen.orientation = (ScreenOrientation)i;
            yield return new WaitForSeconds(1);
            if (!screen.orientations.ContainsKey(Screen.orientation))
            {
                var data = new OrientationDependentData()
                {
                    safeArea = Screen.safeArea,
                    cutouts = Screen.cutouts
                };
                screen.orientations.Add(Screen.orientation, data);
            }
        }
        Screen.orientation = orientation;
    }

    IEnumerator exportDone()
    {
        yield return new WaitForSeconds(5);
        menu = Menu.Home;
    }
}