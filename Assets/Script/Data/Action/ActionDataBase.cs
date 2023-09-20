using Script.BuffLogic;
using Script.Enums;
using UnityEngine;

namespace Script.Data
{
    [CreateAssetMenu(menuName = "ActionDetailData", fileName = "newActionDetailData", order = 0)]
    public class ActionDataBase : ScriptableObject
    {
        [field: SerializeField] public SkillType SkillType { get; private set; }
        [field: SerializeField] public DamageType DamageType { get; private set; }
        [field: SerializeField] public TargetForm TargetForm { get; private set; }
        [field: SerializeField] public TargetSide TargetSide { get; private set; }
        
        
        [field: Space(10)]
        [field: SerializeField] public BuffData[] BuffDataMain { get; private set; }
        [field: SerializeField] public BuffData[] BuffDataSub { get; private set; }
        [field: SerializeField] public BuffType RemoveA { get; private set; }
        [field: SerializeField] public BuffData[] RemoveTheSpecifiedBuff { get; private set; }
        
        [field: Space(10)]
        [field: SerializeField] public string MultipleBasedOn { get; private set; } = "Attack";
        [field: SerializeField] public int Multiple { get; private set; }
        [field: SerializeField] public int FixedValue { get; private set; }
        [field: SerializeField] public int WeaknessBreak { get; private set; }
        [field: Space(10)]
        [field: SerializeField] public int MultipleSub { get; private set; }
        [field: SerializeField] public int FixedValueSub { get; private set; }
        [field: SerializeField] public int WeaknessBreakSub { get; private set; }
        [field: Space(10)]
        [field: SerializeField] public float BounceDecreaseFactor { get; private set; } = 1;
        /// <summary>
        /// 是否是一次独立的攻击/治疗等等，只有独立的行动才会触发事件
        /// 一般用作附加伤害、额外造成一次伤害等等
        /// </summary>
        [field: SerializeField] public bool TriggerEvent { get; private set; } = true;


    }
}