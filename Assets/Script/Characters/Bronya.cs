using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using Script.Objects;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

namespace Script.Characters
{
    public class Bronya : Friendly
    {
        [field: SerializeField] private ActionDataBase SkillCallBackAction;
        
        protected override void BasicAttack()
        {
            base.BasicAttack();
        }

        protected override void SkillAttack()
        {
            // 先上 Buff
            // 然后解除负面效果
            // 最后使目标立即行动
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _target, SkillAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            // 拉条
            DoAction(SkillType.Enhance, TargetForm.Single);
        }

        protected override void Ultimate()
        {
            // TODO 在 Buff String 里实现一定程度的表达式计算
            SetTarget(null, true, true);
            ActionDetail ad = new ActionDetail(this, _target, UltimateData);
            base.Ultimate();
        }

        
        private BaseObject _lastSkillTarget = null;
        public void CallBackSkillBuff(BaseObject target)
        {
            _lastSkillTarget = target;
            EC.SubscribeEvent(EventID.ActionEnd, SkillTargetActionEnd);
        }

        private void SkillTargetActionEnd(BaseObject sender, BaseObject _)
        {
            if (sender != _lastSkillTarget) return;
            EC.UnsubscribeEvent(EventID.ActionEnd, SkillTargetActionEnd);
            ActionDetail ad = new ActionDetail(this, sender, SkillCallBackAction);
            IM.Process(ad);
        }
    }
}