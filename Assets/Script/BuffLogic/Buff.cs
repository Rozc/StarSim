using System;
using System.Collections.Generic;
using Script.Data;
using Script.InteractLogic;
using Script.Objects;
using UnityEngine;

namespace Script.BuffLogic
{
    [System.Serializable]
    public class Buff
    {
        [field: SerializeField] public BuffData Data;
                
        // Realtime Data
        [field: SerializeField] public BaseObject Caster;
        [field: SerializeField] public bool CheckPointPassed = false;
        [field: SerializeField] public int CurrentStack;
        [field: SerializeField] public int DurationLeft;
        public Dictionary<string, float> PropertyDict;
        public Buff(BuffData data, int stack)
        {
            Data = data;
            CurrentStack = stack <= data.MaxStack ? stack : data.MaxStack;
            DurationLeft = data.Duration;
            PropertyDict = new Dictionary<string, float>();
        }

        public static bool operator ==(Buff a, Buff b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Data.BuffID == b.Data.BuffID;
        }

        public static bool operator !=(Buff a, Buff b)
        {
            return !(a == b);
        }
    }
}

//   * "HP: 100, HP%: 12, ATK: 34, DMG: 1@20|4@20, ..."
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