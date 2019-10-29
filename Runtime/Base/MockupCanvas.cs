using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.NotchSolution
{
#if UNITY_EDITOR
    public class MockupCanvas : MonoBehaviour
    {
        public Image mockupImage;
        public bool PrefabStage { private get; set; }

        const float proColor = 40 / 255f;
        const float personalColor = 49 / 255f;

        public void UpdateMockupSprite(Sprite sprite, ScreenOrientation orientation, bool simulate, bool flipped, Color prefabModeOverlayColor)
        {
            if (!simulate)
            {
                mockupImage.enabled = false;
            }
            else
            {
                mockupImage.enabled = true;

                if (sprite == null) mockupImage.color = new Color(0, 0, 0, 0);
                else if (PrefabStage) mockupImage.color = prefabModeOverlayColor;
                else if (EditorGUIUtility.isProSkin) mockupImage.color = new Color(proColor, proColor, proColor, 1);
                else mockupImage.color = new Color(personalColor, personalColor, personalColor, 1);

                mockupImage.transform.rotation = Quaternion.Euler(0, 0, orientation == ScreenOrientation.Landscape ? 90 : 0);
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
    }
#endif
}
