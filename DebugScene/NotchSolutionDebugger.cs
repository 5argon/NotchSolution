using E7.NotchSolution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class NotchSolutionDebugger : MonoBehaviour
{
    public Text debugText;
    public GameObject debugRectPrefab;
    public RectTransform rootRect;
    private StringBuilder sb = new StringBuilder();

    void Update()
    {
        sb.Clear();
        ClearRects();

        sb.AppendLine($"<b>-- PLEASE ROTATE THE DEVICE TO GET BOTH ORIENTATION'S DETAILS! --</b>\n");

#if UNITY_EDITOR
        var safeArea = RealSize(NotchSolutionUtility.SimulateSafeAreaRelative);
#else
        var safeArea = Screen.safeArea;
#endif
        PlaceRect(safeArea, Color.red);
        if (Screen.orientation != NotchSolutionUtility.GetCurrentOrientation())
            safeArea.Set(Screen.width - safeArea.x, Screen.height - safeArea.y, safeArea.width, safeArea.height);
        sb.AppendLine($"Safe area : {safeArea}\n");

#if UNITY_2019_2_OR_NEWER
#if UNITY_EDITOR
        var relativeCutouts = NotchSolutionUtility.SimulateCutoutsRelative;
        List<Rect> rectCutouts = new List<Rect>();
        foreach (Rect rect in relativeCutouts) rectCutouts.Add(RealSize(rect));
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
#endif

        sb.AppendLine($"Current resolution : {Screen.currentResolution}\n");
        sb.AppendLine($"All Resolutions : {string.Join(" / ", Screen.resolutions.Select(x => x.ToString()))}\n");
        sb.AppendLine($"DPI : {Screen.dpi} WxH : {Screen.width}x{Screen.height} Orientation : {Screen.orientation}\n");
        var joinedProps = string.Join(" / ", typeof(SystemInfo).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(x => $"{x.Name} : {x.GetValue(null)}"));
        sb.AppendLine(joinedProps);
        debugText.text = sb.ToString();
    }

    Rect RealSize(Rect relative)
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
        foreach (var dbr in debugRects)
        {
            GameObject.Destroy(dbr.gameObject);
        }
        debugRects.Clear();
    }

    public void PlaceRect(Rect rct, Color c)
    {
        var go = GameObject.Instantiate(debugRectPrefab, rootRect);
        go.transform.localScale = Vector3.one;
        //go.transform.SetAsFirstSibling();
        var dbr = go.GetComponent<DebugRect>();
        dbr.PlaceItselfAtScreenRect(rct, c);
        debugRects.Add(dbr);
    }
}
