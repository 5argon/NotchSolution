using UnityEngine;

namespace E7.NotchSolution
{
    public enum SimulationDevice
    {
        [InspectorName("Apple/ iPhone X")] iPhoneX,
        [InspectorName("Apple/ iPad Pro")] iPadPro,
        [InspectorName("OnePlus/ 6T")] OnePlus6T,
        [InspectorName("Huawei/ Mate 20 Pro")] HuaweiMate20Pro,
        [InspectorName("Samsung/ S10+")] SamsungS10Plus,
        [InspectorName("Samsung/ S10")] SamsungS10
    }
}