using System.Collections.Generic;
using Script.ActionLogic;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using UnityEngine;

namespace Script.Objects
{
    public class Enemy : BaseObject
    {
        [field: SerializeField] protected bool RandomTarget;
        [field: SerializeField] protected GameObject PresetTarget = null;
        [field: SerializeField] protected ActionDataBase BasicAttackData;

        
        public override void GetMessageFromGM(Message msg)
        {
            switch (msg)
            {
                case Message.TurnBegin:
                    OnTurnBegin();
                    break;
            }
        }

        public override void GetAction(Action action) // 怪物在接收到行动后立即执行，这是个占位符
        {
            // ActionBegin 阶段在此执行
            _currentAction = action;
            if (RandomTarget)
            {
                SetTarget(GM.GetRandomObject(false));
            }
            else
            {
                SetTarget(PresetTarget.GetComponent<BaseObject>());
            }

            
            ActionDetail ad = new ActionDetail(this, _target, BasicAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            InteractManager.Instance.Process(ad);
            // 默认实现占位符

            DoAnimation(SkillType.Attack, TargetForm.Single);
        }
    }
}
