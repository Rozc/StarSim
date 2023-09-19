using Script.Data;
using Script.Enums;
using Script.InteractLogic;

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
            Act(BasicAttackData);
        }
        
        protected virtual void SkillAttack()
        {
            Act(SkillAttackData);
        }
        protected virtual void Ultimate()
        {
            Act(UltimateData);
        }

        
        protected virtual void Act(ActionDataBase data, 
            ExtraLogic afterTargetSet = null, 
            ExtraLogic afterActionPrepared = null,
            ExtraLogic afterIMProcessed = null,
            ExtraLogic afterInteractDone = null)
        {
            _afterInteractDone = afterInteractDone;
            if (data.TargetForm == TargetForm.Aoe)
            {
                SetTarget(null, true, data.TargetSide == TargetSide.Friendly);
            }
            else
            {
                SetTarget(GM.CurrentTarget);
            }
            afterTargetSet?.Invoke();
            ActionDetail ad = new ActionDetail(this, _target, data);
            GM.GetMessageFromActor(Message.ActionPrepared);
            afterActionPrepared?.Invoke();
            IM.Process(ad);
            afterIMProcessed?.Invoke();
            DoAnimation(data.SkillType, data.TargetForm, afterInteractDone);
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
                    ReadyTo(CommandStatus.SkillAttack);
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Q):
                    ReadyTo(CommandStatus.BasicAttack);
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
                    ReadyTo(CommandStatus.BasicAttack);
                    break;
                case ActionType.Extra: // Here Only Ultimate
                    if (_currentAction.ExtraActCode != 0)
                    {
                        // Debug.LogError(Data.BaseData.Name + " Get Action Error: Extra Action except Ultimate should be handled in Child Class!");
                    }
                    ReadyTo(CommandStatus.Release);
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
        
        protected virtual void ReadyTo(CommandStatus commandStatus)
        {
            switch (commandStatus)
            {
                case CommandStatus.BasicAttack:
                    CommandState = CommandStatus.BasicAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, false);
                    UI.SetInteractable(ButtonID.SkillAttack, true);
                    GM.SetCursorForm(BasicAttackData.TargetForm, BasicAttackData.TargetSide);
                    break;
                case CommandStatus.SkillAttack:
                    CommandState = CommandStatus.SkillAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, true);
                    UI.SetInteractable(ButtonID.SkillAttack, false);
                    GM.SetCursorForm(SkillAttackData.TargetForm, SkillAttackData.TargetSide);
                    break;
                case CommandStatus.Release:
                    CommandState = CommandStatus.Release;
                    UI.SetInteractable(ButtonID.BasicAttack, false);
                    UI.SetInteractable(ButtonID.SkillAttack, false);
                    GM.SetCursorForm(UltimateData.TargetForm, UltimateData.TargetSide);
                    break;
                case CommandStatus.None:
                    CommandState = CommandStatus.None;
                    GM.SetCursorForm(TargetForm.None, TargetSide.None);
                    break;
            }
        }


    }
}

