using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DebugRect : MonoBehaviour
{
    [SerializeField] private Text debugText;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image borderColor;

    // public void Update()
    // {
    //     PlaceItselfAtScreenRect(rect);
    // }

    public void PlaceItselfAtScreenRect(Rect screenRect, Color color = default)
    {
        borderColor.color = color == default ? Color.red : color;
        var parentRect = transform.parent.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenRect.position, Camera.main,
            out var localPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenRect.size, Camera.main,
            out var localSize);

        localSize = localSize + parentRect.sizeDelta / 2;

        debugText.text = $"{screenRect}";

        rectTransform.localPosition = localPoint;
        rectTransform.sizeDelta = localSize;
    }
}