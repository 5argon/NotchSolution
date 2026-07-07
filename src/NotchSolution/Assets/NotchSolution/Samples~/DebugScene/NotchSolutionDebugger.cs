using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Device.Screen;
using SystemInfo = UnityEngine.Device.SystemInfo;

/// <summary>
///     Displays the current safe area, cutouts, and device information reported by
///     <see cref="UnityEngine.Device.Screen"/> and <see cref="UnityEngine.Device.SystemInfo"/>.
/// </summary>
internal class NotchSolutionDebugger : MonoBehaviour
{
    [SerializeField] private Text debugText;
    [SerializeField] private GameObject debugRectPrefab;
    [SerializeField] private RectTransform rootRect;

    private readonly List<DebugRect> debugRects = new List<DebugRect>();
    private readonly StringBuilder sb = new StringBuilder();

    private void Update()
    {
        sb.Clear();
        ClearRects();

        var safeArea = Screen.safeArea;
        PlaceRect(safeArea, Color.red);
        sb.AppendLine($"Safe area : {safeArea}\n");

        var cutouts = Screen.cutouts;
        foreach (var cutout in cutouts)
        {
            PlaceRect(cutout, Color.blue);
        }

        sb.AppendLine($"Cutouts : {string.Join(" / ", cutouts.Select(x => x.ToString()))} \n");

        sb.AppendLine($"Current resolution : {Screen.currentResolution}\n");
        sb.AppendLine($"All resolutions : {string.Join(" / ", Screen.resolutions.Select(x => x.ToString()))}\n");
        sb.AppendLine(
            $"DPI : {Screen.dpi} WxH : {Screen.width}x{Screen.height} Orientation : {Screen.orientation}\n");

        var joinedProps = string.Join(" / ",
            typeof(SystemInfo).GetProperties(BindingFlags.Public | BindingFlags.Static)
                // Reading graphicsDeviceType under the Device Simulator logs a "could not pick" warning, so skip it.
                .Where(x => x.Name != "graphicsDeviceType")
                .Select(x => $"{x.Name} : {x.GetValue(null)}"));
        sb.AppendLine(joinedProps);

        debugText.text = sb.ToString();
    }

    private void ClearRects()
    {
        foreach (var dbr in debugRects)
        {
            Destroy(dbr.gameObject);
        }

        debugRects.Clear();
    }

    private void PlaceRect(Rect screenRect, Color color)
    {
        var go = Instantiate(debugRectPrefab, rootRect);
        go.transform.localScale = Vector3.one;
        var dbr = go.GetComponent<DebugRect>();
        dbr.PlaceItselfAtScreenRect(screenRect, color);
        debugRects.Add(dbr);
    }
}
