using Script.BuffLogic;
using UnityEngine;

namespace Script.Data
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "newBuff", menuName = "Data/Buff", order = 0)]
    public class BuffData : ScriptableObject
    {
        [field: SerializeField] public bool Show { get; protected set; }
        [field: SerializeField] public int BuffID { get; protected set; }
        [field: SerializeField] public string BuffName { get; protected set; }
        [field: SerializeField] public string Description { get; protected set; }
        
        [field: SerializeField] public BuffType BuffType { get; protected set; }
        [field: SerializeField] public bool DurationBaseOnTurn { get; protected set; }
        [field: SerializeField] public int Duration { get; protected set; }
        [field: SerializeField] public bool Stackable { get; protected set; }
        [field: SerializeField] public int StackAtATime { get; protected set; }
        [field: SerializeField] public int MaxStack { get; protected set; }        
        [field: SerializeField] public bool NeedToBeRemovedFirst { get; protected set; }

        [field: SerializeField] public bool CheckAtTurnBegin { get; protected set; }
        [field: SerializeField] public bool CheckAtMainActionEnd { get; protected set; }
        [field: SerializeField] public bool MustBeRemovedAtTurnEnd { get; protected set; }
        
        [field: SerializeField] public float Probability { get; protected set; }
        [field: SerializeField] public bool FixedProbability { get; protected set; }
        
        // TODO: Add Icon 
        
        [field: SerializeField] public bool HasCallBack { get; protected set; }
        [field: SerializeField] public string[] CallBackList { get; protected set; }
        [field: SerializeField] public bool HasProperty { get; protected set; }
        // [field: SerializeField] public string[] BuffProperties { get; protected set; } 
        [field: SerializeField] public BuffProperty[] BuffPropertyList { get; protected set; }
    }
}

/* HP, HP%   : Health
 * ATK, ATK% : Attack
 * DEF, DEF% : Defence
 * SPD, SPD% : Speed
 * EHIT      : EftHitRate
 * ERES      : EftRes
 * CRAT      : CritRate
 * CDMG      : CritDamage
 * BEFT      : BreakEft
 * HBST      : HealBoost
 * ERR       : EnergyRegenRatio
 * DMG       : DamageBoost
 * RES       : Resistance
 * DRED      : DamageReduce
 * HFAC      : HitFactor
*      DmgType      All  = 0
 *                  Phsy = 1,
                    Fire = 2,
                    Ice  = 3,
                    Litn = 4,
                    Wind = 5,
                    Qutm = 6,
                    Imag = 7,
                    BasicAttack = 10
                    SkillAttack = 11
                    Ultimate    = 12
                    Followup    = 13
                    Dot         = 14
 */
 /*
  * "HP: 100, HP%: 12, ATK: 34, DMG: {1@20|4@20}, ..."
  */