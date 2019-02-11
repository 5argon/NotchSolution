using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DebugRect : MonoBehaviour
{
    public Text debugText;
    //public Rect rect;
    public RectTransform rectTransform;
    public Image borderColor;

    // public void Update()
    // {
    //     PlaceItselfAtScreenRect(rect);
    // }

    public void PlaceItselfAtScreenRect(Rect screenRect, Color color = default)
    {
        borderColor.color = color == default ? Color.red : color;
        var parentRect = this.transform.parent.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenRect.position, Camera.main, out Vector2 localPoint);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenRect.size, Camera.main, out Vector2 localSize);

        localSize = localSize + (parentRect.sizeDelta /2);

        debugText.text = $"{screenRect}";
        
        rectTransform.localPosition = localPoint;
        rectTransform.sizeDelta = localSize;
        
    }
}
