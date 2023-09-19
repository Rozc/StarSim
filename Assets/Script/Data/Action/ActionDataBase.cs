using Script.BuffLogic;
using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    public class ActionDataBase : ScriptableObject
    {
        [field: SerializeField] public SkillType SkillType { get; private set; }
        [field: SerializeField] public DamageType DamageType { get; private set; }
        [field: SerializeField] public TargetForm TargetForm { get; private set; }
        [field: SerializeField] public TargetSide TargetSide { get; private set; }
        [field: SerializeField] public BuffData[] BuffDataList { get; private set; }
        [field: SerializeField] public BuffType RemoveA { get; private set; }
        [field: SerializeField] public BuffData[] RemoveTheSpecifiedBuff { get; private set; }
        
        /// <summary>
        /// 是否是一次独立的攻击/治疗等等，只有独立的行动才会触发事件
        /// 一般用作附加伤害、额外造成一次伤害等等
        /// </summary>
        [field: SerializeField] public bool NotAnDiscreteAction { get; private set; }
        

    }
}