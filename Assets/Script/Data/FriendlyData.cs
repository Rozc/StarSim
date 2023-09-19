using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    public class FriendlyData : ObjectData
    {
        [field: SerializeField] public PathType Path { get; private set; }
    }
}