using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using E7.NotchSolution;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MockupCanvas : MonoBehaviour
{
    public Image mockupImage;
    public bool PrefabStage { private get; set; }

    const float proColor = 40 / 255f;
    const float personalColor = 49 / 255f;

    public void Hide()
    {
        mockupImage.enabled = false;
    }

    public void Show()
    {
        mockupImage.enabled = true;
    }

    // public void UpdateColor(
    // {
    //     //mockupImage.color = 
    // }

    // public void OnDestroy()
    // {
    //     Debug.Log($"DESTROY");
    // }

    // public void OnDisable()
    // {
    //     Debug.Log($"DISABLE");
    // }

    // public void OnEnable()
    // {
    //     Debug.Log($"ENABLE");
    // }

    public void SetMockupSprite(Sprite sprite, ScreenOrientation orientation, bool simulate, bool flipped)
    {
        if (!simulate)
        {
            mockupImage.enabled = false;
        }
        else
        {
            mockupImage.enabled = true;

#if UNITY_EDITOR
            if (sprite == null) mockupImage.color = new Color(0, 0, 0, 0);
            else if (PrefabStage) mockupImage.color = NotchSolutionUtility.PrefabModeOverlayColor;
            else if (EditorGUIUtility.isProSkin) mockupImage.color = new Color(proColor, proColor, proColor, 1);
            else mockupImage.color = new Color(personalColor, personalColor, personalColor, 1);
#endif

            mockupImage.transform.rotation = Quaternion.Euler(0, 0, orientation == ScreenOrientation.Landscape ? 90 : 0);
            Vector2 screenSize = mockupImage.transform.parent.GetComponent<RectTransform>().sizeDelta;
            mockupImage.GetComponent<RectTransform>().sizeDelta = orientation == ScreenOrientation.Landscape ? new Vector2(screenSize.y, screenSize.x) : screenSize;
            mockupImage.sprite = sprite;
            mockupImage.transform.localScale = new Vector3(
                flipped ? -1 : 1,
                flipped ? -1 : 1,
                1
            );

            //Force refreshing the mockup
            mockupImage.enabled = false;
            mockupImage.enabled = true;
        }
    }
}
