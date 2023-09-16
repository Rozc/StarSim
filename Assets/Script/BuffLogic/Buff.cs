using System;
using System.Collections.Generic;
using Script.Data;
using UnityEngine;

namespace Script.BuffLogic
{
    [System.Serializable]
    public class Buff
    {
        [field: SerializeField] public BuffData Data;
                
        // Realtime Data
        [field: SerializeField] public bool CheckPointPassed;
        [field: SerializeField] public int CurrentStack;
        [field: SerializeField] public int DurationLeft;

        public Buff(BuffData data, int stack)
        {
            Data = data;
            CurrentStack = stack <= data.MaxStack ? stack : data.MaxStack;
            DurationLeft = data.Duration;
        }
        
        public static Dictionary<string, float> StrToBuffProperty(string str)
        {
            Dictionary<string, float> dict = new Dictionary<string, float>();
            string[] strs = str.Split(',');
            foreach (string s in strs)
            {
                string[] ss = s.Split(':');
                dict.Add(ss[0].Trim(), float.Parse(ss[1]));
            }

            return dict;
        }
        public static BuffProperty TranslateBuffString(string str)
        {
            if (Enum.TryParse(str, out BuffProperty property))
            {
                return property;
            }
            else
            {
                Debug.LogError("Unknown BuffProperty String Error: " + str);
                return BuffProperty.None;
            }
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