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
        [field: SerializeField] public bool IsWeaknessBroken;

        new void Start()
        {
            Data = new RTEnemyData(BaseData as EnemyData);
            base.Start();
        }
        
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

        protected override void OnActionBegin()
        {
            base.OnActionBegin();
            if (IsWeaknessBroken && Data is RTEnemyData data)
            {
                data.WeaknessValue = data.MaxWeaknessValue;
                Debug.Log(Data.Name + "recovered from weakness broken!");
                IsWeaknessBroken = false;
            }
        }

        public override void ReceiveDamage(float value, BaseObject actor, bool trigger = true, int weakness = 0)
        {
            base.ReceiveDamage(value, actor, trigger, weakness);
            if (trigger && Data is RTEnemyData data)
            {
                if (data.WeaknessList.Contains(actor.Data.BattleType))
                {
                    data.WeaknessValue -= weakness;
                    if (data.WeaknessValue <= 0)
                    {
                        IsWeaknessBroken = true;
                        Debug.Log(Data.Name + "oh no! my weakness is broken!");
                    }
                }
            }
        }
    }
}
