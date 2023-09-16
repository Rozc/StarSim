namespace Script.BuffLogic
{
    public enum BuffProperty
    {
        None = 0,
        MaxHealthFixed,
        MaxHealthPercentage,
        HealthFixed,
        HealthPercentage,
        AttackFixed,
        AttackPercentage,
        DefenceFixed,
        DefencePercentage,
        SpeedFixed,
        SpeedPercentage,
        EffectHitRate,
        EffectResistance,
        CritRate,
        CritDamage,
        BreakEffect,
        HealingBoost,
        EnergyRegenRatio,
        DamageReduce,
        HitFactor,
        // 伤害类型
        DamageBoostAll,
        DamageBoostPhsy,
        DamageBoostFire,
        DamageBoostIce,
        DamageBoostLitn,
        DamageBoostWind,
        DamageBoostQutm,
        DamageBoostImag,
        DamageBoostBasicAttack,
        DamageBoostSkillAttack,
        DamageBoostUltimate,
        DamageBoostFollowup,
        DamageBoostDot,
        // 抗性类型
        ResistanceAll,
        ResistancePhsy,
        ResistanceFire,
        ResistanceIce,
        ResistanceLitn,
        ResistanceWind,
        ResistanceQutm,
        ResistanceImag,
        ResistanceBasicAttack,
        ResistanceSkillAttack,
        ResistanceUltimate,
        ResistanceFollowup,
        ResistanceDot,
        // 易伤/减伤
        DamageReduceAll,
        DamageReducePhsy,
        DamageReduceFire,
        DamageReduceIce,
        DamageReduceLitn,
        DamageReduceWind,
        DamageReduceQutm,
        DamageReduceImag,
        DamageReduceBasicAttack,
        DamageReduceSkillAttack,
        DamageReduceUltimate,
        DamageReduceFollowup,
        DamageReduceDot,

    }
}

//   * "HP: 100, HP%: 12, ATK: 34, DMG1: 20, DMG6: 50, ..."
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