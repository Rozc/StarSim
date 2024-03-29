using System.Collections.Generic;
using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    
    public class ObjectData : ScriptableObject
    {
        [field: SerializeField] public int CharacterID { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public BattleType BattleType { get; private set; }
        [field: SerializeField] public string[] PropertyStrings { get; private set; }


        
    }

}
