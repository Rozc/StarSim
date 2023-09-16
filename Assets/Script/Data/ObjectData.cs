using System.Collections.Generic;
using Script.Enums;
using Script.Tools;
using Tools;
using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "newObjectData", menuName = "Data/Object Data/Object Data")]
    public class ObjectData : ScriptableObject
    {
        [field: SerializeField] public int CharacterID { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public BattleType BattleType { get; private set; }
        [field: SerializeField] public PathType Path { get; private set; }
        [field: SerializeField] public string[] PropertyStrings { get; private set; }

    }

}
