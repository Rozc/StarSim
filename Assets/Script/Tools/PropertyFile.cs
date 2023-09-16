using System.Collections.Generic;

namespace Script.Tools
{
    public static class PropertyFile
    {
        /// <summary>
        /// 0 => BaseCharacter, 1 => Enemy, 2 => Trace, 3 => LightCone, 4 => Relics, 5 => Buffed
        /// </summary>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Dictionary<string, float> Read(string[] content, int type=0)
        {
            var result = new Dictionary<string, float>();
            if (type == 0)
            {
                result.Add("CritRate", 5);
                result.Add("CritDamage", 50);
                result.Add("EnergyRegen", 100);
            }
            foreach (var line in content)
            {
                string[] pair = line.Split(':');
                if (pair.Length == 2 && pair[0].Trim() != "" && float.TryParse(pair[1], out float value))
                {
                    result[pair[0].Trim()] = value;
                }
            }
            
            // Check
            if (type == 0 &&
                !(result.ContainsKey("Health") 
                  && result.ContainsKey("Attack") 
                  && result.ContainsKey("Defence") 
                  && result.ContainsKey("Speed")
                  && result.ContainsKey("MaxEnergy")))
            {
                throw new System.Exception("Health, Attack, Defence, Speed and MaxEnergy are required for a Character.");
            }

            if (type == 1 &&
                !(result.ContainsKey("Health")
                  && result.ContainsKey("Attack")
                  && result.ContainsKey("Defence")
                  && result.ContainsKey("Speed")))
            {
                throw new System.Exception("Health, Attack, Defence and Speed are required for a Enemy.");
            }

            return result;
        }
    }
}

/* 一些属性的默认值
 *              { "CritRate", 5 },
                { "CritDamage", 50 },
                { "BreakEffect", 0 },
                { "HealingBoost", 0 },
                { "EnergyRegen", 100 },
                { "EffectHitRate", 0 },
                { "EffectRes", 0 },
                { "DamageBoostAll", 0 },
                { "DamageBoostPhsy", 0 },
                { "DamageBoostFire", 0 },
                { "DamageBoostIce", 0 },
                { "DamageBoostLitn", 0 },
                { "DamageBoostWind", 0 },
                { "DamageBoostQutm", 0 },
                { "DamageBoostImag", 0 },
                { "DamageReductionAll", 0 },
                { "DamageReductionPhsy", 0 },
                { "DamageReductionFire", 0 },
                { "DamageReductionIce", 0 },
                { "DamageReductionLitn", 0 },
                { "DamageReductionWind", 0 },
                { "DamageReductionQutm", 0 },
                { "DamageReductionImag", 0 },
                { "ResistanceAll", 0 },
                { "ResistancePhsy", 0 },
                { "ResistanceFire", 0 },
                { "ResistanceIce", 0 },
                { "ResistanceLitn", 0 },
                { "ResistanceWind", 0 },
                { "ResistanceQutm", 0 },
                { "ResistanceImag", 0 },
                { "ResistancePenetrationAll", 0 },
                { "ResistancePenetrationPhsy", 0 },
                { "ResistancePenetrationFire", 0 },
                { "ResistancePenetrationIce", 0 },
                { "ResistancePenetrationLitn", 0 },
                { "ResistancePenetrationWind", 0 },
                { "ResistancePenetrationQutm", 0 },
                { "ResistancePenetrationImag", 0 },
                { "DefencePenetration", 0 } // 防御穿透应该不用分属性吧，造成xx伤害时无视xx防御？
 */