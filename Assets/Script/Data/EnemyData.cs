using System.Collections.Generic;
using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(fileName = "newEnemyData", menuName = "Data/Object Data/Enemy Data")]
    public class EnemyData : ObjectData
    {
        [field: SerializeField] public List<BattleType> WeaknessList { get; protected set; }
        [field: SerializeField] public int WeaknessValue { get; protected set; }
    }
}