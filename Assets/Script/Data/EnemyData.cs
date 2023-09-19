using System.Collections.Generic;
using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    public class EnemyData : ObjectData
    {
        [field: SerializeField] public List<BattleType> WeaknessList { get; protected set; }
        [field: SerializeField] public int WeaknessValue { get; protected set; }
    }
}