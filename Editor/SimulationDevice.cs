using UnityEngine;

namespace E7.NotchSolution
{
    public enum SimulationDevice
    {
#if UNITY_2019_2_OR_NEWER
        [InspectorName("Apple/ iPhone X")] iPhoneX,
        [InspectorName("Apple/ iPad Pro")] iPadPro,
        [InspectorName("OnePlus/ 6T")] OnePlus6T,
        [InspectorName("Huawei/ Mate 20 Pro")] HuaweiMate20Pro,
        [InspectorName("Samsung/ S10+")] SamsungS10Plus,
        [InspectorName("Samsung/ S10")] SamsungS10
#else
        iPhoneX,
        iPadPro,
        OnePlus6T,
        HuaweiMate20Pro,
        SamsungS10Plus,
        SamsungS10
#endif
    }
}