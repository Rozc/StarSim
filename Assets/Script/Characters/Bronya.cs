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
        

        /*protected override void SkillAttack()
        {
            // 先上 Buff
            // 然后解除负面效果
            // 最后使目标立即行动
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _target, SkillAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Support, TargetForm.Single);
        }*/

        /*protected override void Ultimate()
        {
            SetTarget(null, true, true);
            ActionDetail ad = new ActionDetail(this, _target, UltimateData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Support, TargetForm.Aoe);
        }*/

        
        private BaseObject _lastSkillTarget = null;
        public void CallBackSkillBuff(BaseObject target)
        {
            _lastSkillTarget = target;
            EC.SubscribeEvent(EventID.ActionEnd, SkillTargetActionEnd);
        }

        private void SkillTargetActionEnd(BaseObject sender, BaseObject _)
        {
            if (_lastSkillTarget is null)
            {
                Debug.LogError("Bronya: _lastSkillTarget is null!");
                return;
            } 
            if (sender != _lastSkillTarget) return;
            _lastSkillTarget = null;
            EC.UnsubscribeEvent(EventID.ActionEnd, SkillTargetActionEnd);
            ActionDetail ad = new ActionDetail(this, sender, SkillCallBackAction);
            IM.Process(ad);
        }
    }
}