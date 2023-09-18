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
    /// 也就是这边需要拿到所有对象脚本的引用
    /// </summary>
    public class InteractManager : SingletonBase<InteractManager>
    {
        public void Process(ActionDetail ad)
        {
            // TODO
            // 根据行动细节计算影响
            // 通知目标
            switch (ad.Data.TargetForm)
            {
                case TargetForm.Single:
                    Single(ad);
                    break;
            }
        }
        
        // 干脆把整个 ad 发给目标让人自己算得了
        // 可能还是的在这里计算
        private void Single(ActionDetail ad)
        {
            switch (ad.Data.SkillType)
            {
                case SkillType.Attack:
                case SkillType.Restore:
                case SkillType.Enhance:
                case SkillType.Support: 
                case SkillType.Defence:
                    if (ad.Data.BuffDataList.Length > 0)
                    {
                        Buff[] buffs = new Buff[ad.Data.BuffDataList.Length];
                        for (var i = 0; i < ad.Data.BuffDataList.Length; i++)
                        {
                            var buffData = ad.Data.BuffDataList[i];
                            // 命中概率计算
                            float probability = buffData.Probability;
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
                            
                            // 命中，数值计算
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
        private void Aoe(ActionDetail ad)
        {
            
        }
    }
}