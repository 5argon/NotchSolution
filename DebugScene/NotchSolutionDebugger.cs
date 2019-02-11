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

        sb.AppendLine($"Safe area : {Screen.safeArea}\n");
        PlaceRect(Screen.safeArea, Color.red);

#if UNITY_2019_2_OR_NEWER
        sb.AppendLine($"Cutouts : {string.Join(" / ", Screen.cutouts.Select(x => x.ToString()))} \n");
        foreach (Rect r in Screen.cutouts)
        {
            PlaceRect(r, Color.blue);
        }
#endif

        sb.AppendLine($"Current resolution : {Screen.currentResolution}\n");
        sb.AppendLine($"All Resolutions : {string.Join(" / ", Screen.resolutions.Select(x => x.ToString()))}\n");
        sb.AppendLine($"DPI : {Screen.dpi} WxH : {Screen.width}x{Screen.height} Orientation : {Screen.orientation}\n");
        var props = typeof(SystemInfo).GetProperties(BindingFlags.Public | BindingFlags.Static);
        foreach(var p in props)
        {
            sb.AppendLine($"{p.Name} : {p.GetValue(null)}");
        }
        debugText.text = sb.ToString(); 
    }

    private List<DebugRect> debugRects = new List<DebugRect>();
    public void ClearRects()
    {
        foreach(var dbr in debugRects)
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
        dbr.PlaceItselfAtScreenRect(rct);
        debugRects.Add(dbr);
    }

}
