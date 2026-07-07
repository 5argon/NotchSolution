using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.NotchSolution
{
    /// <summary>
    ///     Editor only utility but in the runtime assembly, since some runtime stuff has editor-only
    ///     code and it cannot reference editor assembly or we would have circular dependency.
    /// </summary>
    internal static class NotchSolutionUtilityEditor
    {
#if UNITY_EDITOR
        internal static (bool landscapeCompatible, bool portraitCompatible) GetOrientationCompatibility()
        {
            var landscapeCompatible = false;
            var portraitCompatible = false;
            switch (PlayerSettings.defaultInterfaceOrientation)
            {
                case UIOrientation.LandscapeLeft:
                case UIOrientation.LandscapeRight:
                    landscapeCompatible = true;
                    break;
                case UIOrientation.Portrait:
                case UIOrientation.PortraitUpsideDown:
                    portraitCompatible = true;
                    break;
                case UIOrientation.AutoRotation:
                    if (PlayerSettings.allowedAutorotateToLandscapeLeft)
                    {
                        landscapeCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToLandscapeRight)
                    {
                        landscapeCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToPortrait)
                    {
                        portraitCompatible = true;
                    }

                    if (PlayerSettings.allowedAutorotateToPortraitUpsideDown)
                    {
                        portraitCompatible = true;
                    }

                    break;
            }

            return (landscapeCompatible, portraitCompatible);
        }
#endif
    }
}
