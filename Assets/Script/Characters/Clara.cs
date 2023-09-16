using System.Collections.Generic;
using System.Linq;
using Script.ActionLogic;
using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using Script.Objects;
using UnityEngine;

namespace Script.Characters
{
    public class Clara : Friendly
    {
        // Extra Act Code
        // 0: Ultimate
        // 1: Counter
        // 2: EnhancedCounter
        
        [field: SerializeField] private int enhancedCounterCount;
        [field: SerializeField] private BaseObject counterTarget = null;
        [field: SerializeField] private ActionDataBase counterData;
        [field: SerializeField] private ActionDataBase enhancedCounterData;
        [field: SerializeField] private ActionDataBase skillOnFlagData;
        [field: SerializeField] private BuffData OnSpawnBuff;

        public override void GetAction(Action action)
        {
            if (action.Actor.Position != Position)
            {
                Debug.LogError( "Clara Get Action Error: Not My Action! Actor ID: " + action.Actor.Position + " My ID: " + Position);
                return;
            }
            if (action.ActionType == ActionType.Followup) // 由于接收到追加行动后会立即执行，因此这里不需要保存行动 BUG 由于 DoAction 是从 _currentAction 里拿行动信息的，因此还是要保存
            {
                _currentAction = action;
                Counter();
            }
            else
            {
                base.GetAction(action);
            }
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            AddBuff(new Buff(OnSpawnBuff, 1));
        }

        protected override void BasicAttack()
        {   
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _Target, BasicAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Attack, TargetForm.Single);
        }

        protected override void SkillAttack()
        {
            // TODO
            // 对于具有反击标记的敌人，额外造成一次伤害，并清除其反击标记
            List<ActionDetail> ads = new List<ActionDetail>();
            foreach (var enemy in GM.EnemyObjects)
            {
                if (enemy.HasBuff(1000103, out var _))
                {
                    ads.Add(new ActionDetail(this, enemy, skillOnFlagData)); 
                }
            }
            
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _Target, SkillAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            foreach (var aad in ads)
            {
                IM.Process(aad);
            }
            DoAction(SkillType.Attack, TargetForm.Aoe);
        }


        protected override void Ultimate()
        {
            // 给自己上 Buff
            // 对自己的 Enhance 行动
            SetTarget(this);
            ActionDetail ad = new ActionDetail(this, this, UltimateData);

            GM.GetMessageFromActor(Message.ActionPrepared);
            enhancedCounterCount = 2;
            IM.Process(ad);
            DoAction(SkillType.Enhance, TargetForm.Single);
            
        }

        private void Counter()
        {
            // TODO 确认目标，根据 ID 跟 GM 拿
            SetTarget(counterTarget);
            counterTarget = null; // 重置反击目标
            // 给敌人上反击标记
            if (enhancedCounterCount > 0)
            {
                enhancedCounterCount--;
                ActionDetail ad = new ActionDetail(this, _Target, enhancedCounterData);
                // 执行强化反击
                GM.GetMessageFromActor(Message.ActionPrepared);
                IM.Process(ad);
                // TODO 强化反击 Buff 的次数减少一次 写的优美一点
                foreach (var buff in BuffList.Where(buff => buff.Data.BuffID == 1000102))
                {
                    buff.CurrentStack--;
                    if (buff.CurrentStack == 0)
                    {
                        RemoveBuff(buff);
                        break;
                    }
                }
                
                DoAction(SkillType.Attack, TargetForm.Blast);
            }
            else
            {
                // 暂时有点冗余，考虑一下修改 DoAction 函数
                ActionDetail ad = new ActionDetail(this, _Target, counterData);
                GM.GetMessageFromActor(Message.ActionPrepared);
                IM.Process(ad);
                DoAction(SkillType.Attack, TargetForm.Single);
            }
        }
        
        protected override void RegisterEvent()
        {
            EC.SubscribeEvent(EventID.ObjectOnHit, EventTryCounter);
        }

        private void EventTryCounter(BaseObject sender, BaseObject other)
        {
            // TODO 如果是受到了 Aoe 攻击，每个人都发了一个事件怎么办？
            // 如果实现机制保证克拉拉在生成反击行动后不会再生成反击行动，那就设定 TargetID 就行了，接收到事件时先检测 TargetID 是不是 -1
            // 同时也能解决在生成反击行动但尚未执行时，又被打了的情况
            // 关于记录反击标记的问题，如果敌方位置发生变化，那怎么办呢
            // 应该记录反击目标的 BaseObject 引用，而不是 Position
            if (counterTarget != null) return;
            if (sender is Friendly)
            {
                if (sender == this || enhancedCounterCount > 0)
                {
                    Action action = new(this, 
                        ActionType.Followup, 
                        ActionPriority.Clara_Counter_Self, 
                        enhancedCounterCount > 0 ? 2 : 1);
                    counterTarget = other;
                    GM.RequireExtraAction(action);
                }
                
            }
        }
    }
}