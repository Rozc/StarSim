using Script.BuffLogic;
using UnityEngine;

namespace Script.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "newDoT", menuName = "DoT", order = 0)]
    public class DoTData : BuffData
    {
        // Dot 类型需要在回合开始结算，结算后如果持续时间为 0 则移除
        [field: SerializeField] public DotType DotType { get; private set; }
        [field: SerializeField] public float Multiple { get; private set; }
    }
}