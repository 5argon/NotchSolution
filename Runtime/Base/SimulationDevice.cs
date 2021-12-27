using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace E7.NotchSolution
{
    [Serializable]
    internal class SimulationDevice
    {
        public MetaData Meta;
        public ScreenData[] Screens;
        public SystemInfoData SystemInfo;

        public override string ToString()
        {
            return Meta.friendlyName;
        }
    }

    [Serializable]
    internal class MetaData
    {
        public string friendlyName;
        public string overlay;

        //These values aren't supported
        //public Vector4 overlayOffset;
        //public Texture overlayImage;
    }

    [Serializable]
    internal class ScreenData : ISerializationCallbackReceiver
    {
        public int width;
        public int height;
        public int navigationBarHeight;
        public float dpi;
        [SerializeField] private ScreenOrientation[] orientationKeys;
        [SerializeField] private OrientationDependentData[] orientationValues;
        public Dictionary<ScreenOrientation, OrientationDependentData> orientations;

        public void OnBeforeSerialize()
        {
            if (orientations == null)
            {
                return;
            }

            orientationKeys = orientations.Keys.ToArray();
            orientationValues = orientations.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            Assert.AreEqual(orientationKeys.Length, orientationValues.Length);
            orientations = new Dictionary<ScreenOrientation, OrientationDependentData>();
            for (var i = 0; i < orientationKeys.Length; i++)
            {
                orientations.Add(orientationKeys[i], orientationValues[i]);
            }
        }
    }

    [Serializable]
    internal class OrientationDependentData
    {
        public Rect safeArea;
        public Rect[] cutouts;
    }

    [Serializable]
    internal class SystemInfoData
    {
        public string deviceModel;
        public DeviceType deviceType;
        public string operatingSystem;
        public OperatingSystemFamily operatingSystemFamily;
        public int processorCount;
        public int processorFrequency;
        public string processorType;
        public bool supportsAccelerometer;
        public bool supportsAudio;
        public bool supportsGyroscope;
        public bool supportsLocationService;
        public bool supportsVibration;
        public int systemMemorySize;
        public string unsupportedIdentifier;
        public GraphicsDependentSystemInfoData[] GraphicsDependentData;
    }

    [Serializable]
    internal class GraphicsDependentSystemInfoData
    {
        public GraphicsDeviceType graphicsDeviceType;
        public int graphicsMemorySize;
        public string graphicsDeviceName;
        public string graphicsDeviceVendor;
        public int graphicsDeviceID;
        public int graphicsDeviceVendorID;
        public bool graphicsUVStartsAtTop;
        public string graphicsDeviceVersion;
        public int graphicsShaderLevel;

        public bool graphicsMultiThreaded;

        //public RenderingThreadingMode renderingThreadingMode;
        public bool hasHiddenSurfaceRemovalOnGPU;
        public bool hasDynamicUniformArrayIndexingInFragmentShaders;
        public bool supportsShadows;
        public bool supportsRawShadowDepthSampling;
        public bool supportsMotionVectors;
        public bool supports3DTextures;
        public bool supports2DArrayTextures;
        public bool supports3DRenderTextures;
        public bool supportsCubemapArrayTextures;
        public CopyTextureSupport copyTextureSupport;
        public bool supportsComputeShaders;
        public bool supportsGeometryShaders;
        public bool supportsTessellationShaders;
        public bool supportsInstancing;
        public bool supportsHardwareQuadTopology;
        public bool supports32bitsIndexBuffer;
        public bool supportsSparseTextures;
        public int supportedRenderTargetCount;
        public bool supportsSeparatedRenderTargetsBlend;
        public int supportedRandomWriteTargetCount;
        public int supportsMultisampledTextures;
        public bool supportsMultisampleAutoResolve;
        public int supportsTextureWrapMirrorOnce;
        public bool usesReversedZBuffer;
        public NPOTSupport npotSupport;
        public int maxTextureSize;
        public int maxCubemapSize;
        public int maxComputeBufferInputsVertex;
        public int maxComputeBufferInputsFragment;
        public int maxComputeBufferInputsGeometry;
        public int maxComputeBufferInputsDomain;
        public int maxComputeBufferInputsHull;
        public int maxComputeBufferInputsCompute;
        public int maxComputeWorkGroupSize;
        public int maxComputeWorkGroupSizeX;
        public int maxComputeWorkGroupSizeY;
        public int maxComputeWorkGroupSizeZ;
        public bool supportsAsyncCompute;
        public bool supportsGraphicsFence;
        public bool supportsAsyncGPUReadback;
        public bool supportsRayTracing;
        public bool supportsSetConstantBuffer;
        public bool minConstantBufferOffsetAlignment;
        public bool hasMipMaxLevel;
        public bool supportsMipStreaming;
        public bool usesLoadStoreActions;
    }
}