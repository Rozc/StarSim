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
        
        /*public static Dictionary<string, float>[] ExtractBuffDict(ActionDetail ad)
        {
            var bds = ad.Data.BuffDataList;
            var dicts = new Dictionary<string, float>[bds.Length];

            for (var i = 0; i < bds.Length; i++)
            {
                var buffData = bds[i];
                var buffStrs = buffData.BuffProperties;

                foreach (string s in buffStrs)
                {
                    string[] ss = s.Split(':');
                    var propName = ss[0].Trim();
                    var value = 0f;
                    var valueStr = ss[1].Split("%", '<');
                    if (valueStr.Length > 1)
                    {
                        // 当具有表达式属性时，其中数字一定代表百分比
                        // propName: value = (propName < (propName@value
                        // CritDamage: 13.2 (CritDamage
                        // Attack%: 25 < (Attack@50
                        // Attack: 40 = )Shield < )AttackFixed@60
                        // $ => %, @ => :
                        // ( => 施加方属性
                        // ) => 目标属性
                        // < expression => 最大不超过 expression
                    }
                    else
                    {
                        value = float.Parse(valueStr[0]);
                    }

                    if (dicts[i].ContainsKey(propName))
                    {
                        dicts[i][propName] += value;
                    }
                    else
                    {
                        dicts[i][propName] = value;
                    }
                }
            }


            return dicts;
        }*/
        /*public static BuffProperty TranslateBuffString(string str)
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
        }*/
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