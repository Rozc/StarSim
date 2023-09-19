using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Script.BuffLogic;
using Script.Enums;
using Script.Objects;
using Script.Tools;
using UnityEngine;


namespace Script.InteractLogic
{
    /// <summary>
    /// 伤害交互管理器
    /// 职责：接收行动细节并转发给目标
    /// 主要功能在于将扩散伤害/Aoe伤害 *拆* 成对每个目标的伤害然后发送给受击目标
    /// </summary>
    public class InteractManager : SingletonBase<InteractManager>
    {
        private GameManager GM = GameManager.Instance;
        
        //新增加的 Buff 在 Buff 流程处理完之前进入一个缓存队列，这个队列暂时不参与数值的计算
        // 在 Buff 流程结束，伤害流程开启前，通知所有受影响目标将缓存队列中的 Buff 加入到 Buff 队列中
        public void Process(ActionDetail ad)
        {
            // TODO
            // 根据行动细节计算影响
            // 通知目标
            switch (ad.Data.TargetForm)
            {
                case TargetForm.Single:
                    SingleBuff(ad);
                    SingleDamage(ad);
                    break;
                case TargetForm.Aoe:
                    Aoe(ad);
                    break;
            }
        }

        private void SingleBuff(ActionDetail ad, bool isDelegate = false)
        {

            switch (ad.Data.RemoveA)
                {
                    case BuffType.Buff:
                        ad.Target.RemoveABuff(BuffType.Buff);
                        break;
                    case BuffType.Debuff:
                        ad.Target.RemoveABuff(BuffType.Debuff);
                        break;
                    default:
                        break;
                }

            foreach (var buffData in ad.Data.RemoveTheSpecifiedBuff)
            {
                ad.Target.RemoveTheBuff(buffData);
            }
            
            if (ad.Data.BuffDataList.Length > 0)
            {
                Buff[] buffs = new Buff[ad.Data.BuffDataList.Length];
                for (var i = 0; i < ad.Data.BuffDataList.Length; i++)
                {
                    var buffData = ad.Data.BuffDataList[i];
                    // 命中概率计算
                    var probability = buffData.Probability;
                    if (!buffData.FixedProbability)
                    {
                        // TODO 根据效果命中和效果抵抗，利用公式计算最终概率
                    }
                    if (Random.Range(0f, 99.99f) > probability)
                    {
                        // 未命中
                        buffs[i] = null;
                        continue;
                    }
                    
                    // 命中，Buff 数值计算
                    buffs[i] = new Buff(buffData, buffData.StackAtATime)
                    {
                        Caster = ad.Actor
                    };
                    foreach (var prop in buffData.BuffPropertyList)
                    {
                        buffs[i].PropertyDict.TryAdd(prop.propName, 0);
                        if (prop.propName == "Distance")
                        {
                            buffs[i].PropertyDict[prop.propName] += prop.value;
                            continue;
                        }
                        // 计算buff值
                        if (! ToValue(prop.valueBasedOn, ad.Actor, ad.Target, prop.propName, prop.value, out var value))
                        {
                            continue;
                        }
                        // UpperBound
                        if (ToValue(prop.maxBasedOn, ad.Actor, ad.Target, prop.maxPropName, prop.max, out var max))
                        {
                            value = Mathf.Min(value, max);
                        }
                        // LowerBound
                        if (ToValue(prop.minBasedOn, ad.Actor, ad.Target, prop.minPropName, prop.min, out var min))
                        {
                            value = Mathf.Max(value, min);
                        }
                        buffs[i].PropertyDict[prop.propName] += value;
                    }
                    
                    // 处理 CallBack
                    if (buffData.HasCallBack)
                    {
                        foreach (var cb in buffData.CallBackList)
                        {
                            var mt = ad.Actor.GetType().GetMethod(cb);
                            if (mt != null) mt.Invoke(ad.Actor, new object[] { ad.Target });
                            else Debug.LogError("No Such Callback Method: " + cb + " in " + ad.Actor.Data.Name);
                        }
                    }
                }
                ad.Target.ReceiveBuff(buffs);
            }

            if (!isDelegate) ad.Target.ApplyBuff();
        }

        private void SingleDamage(ActionDetail ad)
        {
            switch (ad.Data.SkillType)
            {
                // TODO 处理伤害数值计算
                case SkillType.Attack:
                    ad.Target.ReceiveDamage(1, ad.Actor, !ad.Data.NotAnDiscreteAction);
                    break;
                case SkillType.Restore:
                    ad.Target.ReceiveHealing(1, ad.Actor, !ad.Data.NotAnDiscreteAction);
                    break;
            }
        }

        private bool ToValue(
            BuffValueBase baseOn, 
            BaseObject sender, 
            BaseObject target, 
            string propName, 
            float raw, 
            out float result)
        {
            result = 0;
            switch (baseOn)
            {
                case BuffValueBase.None:
                    return false;
                case BuffValueBase.Fixed:
                    result = raw;
                    return true;
                case BuffValueBase.SenderFixed:
                    result = sender.Data.GetFixed(propName) * 0.01f * raw;
                    return true;
                case BuffValueBase.SenderRealtime:
                    result = sender.Data.Get(propName) * 0.01f * raw;
                    return true;
                case BuffValueBase.TargetFixed:
                    result = target.Data.GetFixed(propName) * 0.01f * raw;
                    return true;
                case BuffValueBase.TargetRealtime:
                    result = target.Data.Get(propName) * 0.01f * raw;
                    return true;
                default:
                    return false;
            }
        }
        private void Blast(ActionDetail ad)
        {
            
        }
        private void Bounce(ActionDetail ad)
        {
            
        }
        
        /// <summary>
        /// 把 AoE 攻击转换为对各个对象的 Single 攻击，并传递数值和 Buff
        /// </summary>
        /// <param name="ad"></param>
        private void Aoe(ActionDetail ad)
        {
            List<ActionDetail> ads = (
                from obj in (ad.Data.TargetSide == TargetSide.Enemy 
                    ? GM.EnemyObjects.Cast<BaseObject>()
                    : GM.FriendlyObjects.Cast<BaseObject>())
                select new ActionDetail(ad.Actor, obj, ad.Data)).ToList();

            foreach (var aad in ads) SingleBuff(aad, true);
            foreach (var add in ads) add.Target.ApplyBuff();
            foreach (var add in ads) SingleDamage(add);

        }
    }
}