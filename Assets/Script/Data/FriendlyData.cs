using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "newFriendlyData", menuName = "Data/Object Data/Friendly Data")]
    public class FriendlyData : ObjectData
    {
        [field: SerializeField] public PathType Path { get; private set; }
    }
}