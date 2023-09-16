using System;
using Script.Data.ActionData;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using Script.Tools;
using Tools;
using UnityEngine;
using Action = Script.ActionLogic.Action;

namespace Script.Objects
{
    public class Friendly : BaseObject
    {
        
        [field: SerializeField] public ActionDataBase BasicAttackData { get; protected set; } 
        [field: SerializeField] public ActionDataBase SkillAttackData { get; protected set; }
        [field: SerializeField] public ActionDataBase UltimateData { get; protected set; }
        
        [field: SerializeField] protected CommandStatus CommandState;

        protected override void OnTurnBegin()
        {
            base.OnTurnBegin();
            EC.TriggerEvent(EventID.FriendlyTurnBegin, this);
            CommandState = CommandStatus.BasicAttack;
            UI.SetInteractable(ButtonID.BasicAttack, false);
        }

        
        protected virtual void BasicAttack()
        {
            SetTarget(GM.CurrentTarget);
            ActionDetail ad = new ActionDetail(this, _target, BasicAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Attack, TargetForm.Single);
        }
        
        // 默认实现占位符，请勿使用，没做 ActionDetail
        protected virtual void SkillAttack()
        {
            SetTarget(GM.CurrentTarget);
            GM.GetMessageFromActor(Message.ActionPrepared);
            DoAction(SkillType.Attack, TargetForm.Blast);
        }
        protected virtual void Ultimate()
        {
            SetTarget(GM.CurrentTarget);
            GM.GetMessageFromActor(Message.ActionPrepared);
            DoAction(SkillType.Attack, TargetForm.AoeEnemy);
        }

        public void AskUltimate()
        {
            // if (CurrentEnergy < MaxEnergy)
            // {
            //     return null;
            // } else
            {
                Action action = new(
                    this,
                    ActionType.Extra,
                    ActionPriority.Ultimate,
                    0);
                GM.RequireExtraAction(action);
            }
        }


        public virtual void GetInputFromManager(KeyCode input)
        {
            Debug.Log("Friendly Get Input");
            switch (ActType: _currentAction.ActionType, CommandState, input)
            {
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.Q):
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.Space):
                    BasicAttack();
                    break;
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.E):
                    CommandState = CommandStatus.SkillAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, true);
                    UI.SetInteractable(ButtonID.SkillAttack, false);
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Q):
                    CommandState = CommandStatus.BasicAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, false);
                    UI.SetInteractable(ButtonID.SkillAttack, true);
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.E):
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Space):
                    SkillAttack();
                    break;
                case (ActionType.Extra, CommandStatus.Release, KeyCode.Space):
                    Ultimate();
                    break;

            }
        }

        public override void GetAction(Action action)
        {
            if (action.Actor.Position != this.Position)
            {
                // Debug.LogError(Data.BaseData.Name + " Get Action Error: Not My Action! Actor ID: " + action.Actor.ID + " My ID: " + ID);
                return;
            }
            _currentAction = action;
            switch (_currentAction.ActionType)
            {
                case ActionType.Base:
                    CommandState = CommandStatus.BasicAttack;
                    break;
                case ActionType.Extra: // Here Only Ultimate
                    if (_currentAction.ExtraActCode != 0)
                    {
                        // Debug.LogError(Data.BaseData.Name + " Get Action Error: Extra Action except Ultimate should be handled in Child Class!");
                    }
                    CommandState = CommandStatus.Release;
                    break;
                case ActionType.Followup:
                    // Debug.LogError(Data.BaseData.Name + " Get Action Error: Follow-up Action should be handled in Child Class!");
                    break;
                default:
                    // Debug.LogError(Data.BaseData.Name + " Get Action Error: Unknown Action Type!");
                    break;
            }
            
        }

        protected override void ActionInterrupt()
        {
            base.ActionInterrupt();
            CommandState = CommandStatus.None;
        }


    }
}

