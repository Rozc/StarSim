using System.Linq;
using Script.ActionLogic;
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
        [field: SerializeField] private ActionDataBase OnSpawnBuffActionData;

        public override void GetAction(Action action)
        {
            if (action.Actor.Position != Position)
            {
                Debug.LogError( "Clara Get Action Error: Not My Action! Actor ID: " + action.Actor.Position + " My ID: " + Position);
                return;
            }
            _currentAction = action;
            switch (action.ActionType)
            {
                case ActionType.Followup:
                    Counter();
                    break;
                case ActionType.Extra when action.ExtraActCode == 0:
                    // Ultimate
                    ReadyTo(CommandStatus.Release);
                    GM.TargetLock(this);
                    break;
                default:
                    base.GetAction(action);
                    break;
            }
        }
        protected override void OnSpawn()
        {
            base.OnSpawn();
            ActionDetail ad = new ActionDetail(this, this, OnSpawnBuffActionData);
            IM.Process(ad);
        }
        
        /*protected override void BasicAttack()
        {   
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _target, BasicAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Attack, TargetForm.Single);
        }*/

        protected override void SkillAttack()
        {
            Act(SkillAttackData, afterIMProcessed: SkillAttackAttachment);
        }

        private void SkillAttackAttachment()
        {
            // 对于具有反击标记的敌人，额外造成一次伤害，并清除其反击标记
            var ads = (from enemy in GM.EnemyObjects where enemy.HasBuff(1000103, out _)
                select new ActionDetail(this, enemy, skillOnFlagData)).ToList();
            foreach (var aad in ads)
            {
                IM.Process(aad);
            }
        }


        protected override void Ultimate()
        {
            enhancedCounterCount = 2;
            Act(UltimateData);
        }

        private void Counter()
        {
            if (counterTarget is null)
            {
                Debug.LogError("Clara: counterTarget is null!");
                return;
            }
            SetTarget(counterTarget);
            counterTarget = null; // 重置反击目标
            // 给敌人上反击标记
            if (enhancedCounterCount > 0)
            {
                enhancedCounterCount--;
                ActionDetail ad = new ActionDetail(this, _target, enhancedCounterData);
                // 执行强化反击
                GM.GetMessageFromActor(Message.ActionPrepared);
                IM.Process(ad);
                if (HasBuff(1000102, out var buff))
                {
                    buff.CurrentStack--;
                    if (buff.CurrentStack <= 0)
                    {
                        Data.BuffList.Remove(buff);
                    }
                }
                
                DoAnimation(SkillType.Attack, TargetForm.Blast);
            }
            else
            {
                // 暂时有点冗余，考虑一下修改 DoAction 函数
                ActionDetail ad = new ActionDetail(this, _target, counterData);
                GM.GetMessageFromActor(Message.ActionPrepared);
                IM.Process(ad);
                DoAnimation(SkillType.Attack, TargetForm.Single);
            }
        }
        
        protected override void RegisterEvent()
        {
            EC.SubscribeEvent(EventID.ObjectOnHit, EventTryCounter);
        }

        private void EventTryCounter(BaseObject sender, BaseObject other)
        {
            // 如果实现机制保证克拉拉在生成反击行动后不会再生成反击行动，那就设定 TargetID 就行了，接收到事件时先检测 TargetID 是不是 -1
            // 同时也能解决在生成反击行动但尚未执行时，又被打了的情况
            // 关于记录反击标记的问题，如果敌方位置发生变化，那怎么办呢
            // 应该记录反击目标的 BaseObject 引用，而不是 Position
            if (counterTarget != null) return;
            if (sender is not Friendly) return;
            if (sender != this && enhancedCounterCount <= 0) return;
            Action action = new(this, 
                ActionType.Followup, 
                ActionPriority.Clara_Counter_Self, 
                enhancedCounterCount > 0 ? 2 : 1);
            counterTarget = other;
            GM.RequireExtraAction(action);
        }
    }
}