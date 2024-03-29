using System;
using System.Linq;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using Script.Objects;
using UnityEngine;
using Action = Script.ActionLogic.Action;
using Random = UnityEngine.Random;

namespace Script.Characters
{
    public sealed class Qingque : Friendly
    {
        // Extra Act Code
        // 0: Ultimate
        // 1: Extra Action After E
        // 2: Hidden Hand Animation
        // 3: Autarky
        
        [field: SerializeField] private int[] Jades;
        [field: SerializeField] private bool HiddenHand = false;
        [field: SerializeField] private ActionDataBase EnhancedBasicAttackData;
        [field: SerializeField] private ActionDataBase HiddenHandData;
        [field: SerializeField] private ActionDataBase AutarkyBasicData;
        [field: SerializeField] private ActionDataBase AutarkyEnhancedData;

        private Qingque()
        {
            Jades = new int[] {0, 0, 0, 0};
        }
        

        protected override void OnActionBegin()
        {
            base.OnActionBegin();
            // 实际游戏里可能没做 !HiddenHand 这一个检查，因为即使杠了再放终结技，放完也会有自摸的动画
            if (_currentAction.ActionType is ActionType.Base 
                || _currentAction.ExtraActCode == 1)
            {
                if (HiddenHand)
                {
                    ReadyTo(CommandStatus.BasicAttack);
                }
                else if (CheckJade())
                {
                    Action action = new (
                        this,
                        ActionType.Followup,
                        ActionPriority.Qingque_HiddenHand,
                        2);
                    GM.RequireExtraAction(action);
                }
            }
            
        }

        protected override void OnTurnEnd()
        {
            base.OnTurnEnd();
            HiddenHand = false;
        }

        protected override void BasicAttack()
        {
            if (HiddenHand)
            {
                RemoveAllJades();
            }
            else
            {
                RemoveOneJade();
            }
            
            Act(HiddenHand ? EnhancedBasicAttackData : BasicAttackData, 
                afterInteractDone: (HasBuff(1000002, out _)
                    ? () => GM.RequireExtraAction(new Action(
                        this,
                        ActionType.Followup,
                        ActionPriority.Qingque_Autarky,
                        extraActCode:3)) 
                    : null));
        }
        

        private void Autarky()
        {
            Act(HiddenHand ? AutarkyEnhancedData : AutarkyBasicData);
        }
        protected override void SkillAttack()
        {
            if (HiddenHand)
            {
                Debug.LogError("Qingque: Logic Error: Already Hidden Hand");
                return;
            }
            CollectJades(2);
            
            // 需要在 ActionPrepared 后插入 ExtraAction
            Act(SkillAttackData, afterIMProcessed: () => GM.RequireExtraAction(new Action(
                this,
                ActionType.Extra,
                ActionPriority.Qingque_ExtraAction,
                1)));

        }
        protected override void Ultimate()
        {
            RemoveAllJades();
            Jades[0] = 4;
            Act(UltimateData);
        }

        private void ShowHiddenHand()
        {
            HiddenHand = true;
            ActionDetail ad = new ActionDetail(this, this, HiddenHandData);
            SetTarget(this);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAnimation(SkillType.Enhance, TargetForm.Single);
        }

        public override void GetInputFromManager(KeyCode input)
        {
            switch (ActType: _currentAction.ActionType, CommandState, input)
            {
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.Q):
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.Space):
                case (ActionType.Extra, CommandStatus.BasicAttack, KeyCode.Q):
                case (ActionType.Extra, CommandStatus.BasicAttack, KeyCode.Space):
                    BasicAttack();
                    break;
                case (ActionType.Base, CommandStatus.BasicAttack, KeyCode.E):
                case (ActionType.Extra, CommandStatus.BasicAttack, KeyCode.E):
                    if (HiddenHand)
                    {
                        Debug.Log("暂时无法使用战技");
                    } else
                    {
                        ReadyTo(CommandStatus.SkillAttack);
                        GM.TargetLock(this);
                    }
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Q):
                case (ActionType.Extra, CommandStatus.SkillAttack, KeyCode.Q):
                    ReadyTo(CommandStatus.BasicAttack);
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.E):
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Space):
                case (ActionType.Extra, CommandStatus.SkillAttack, KeyCode.E):
                case (ActionType.Extra, CommandStatus.SkillAttack, KeyCode.Space):
                    if (HiddenHand)
                    {
                        Debug.Log("暂时无法使用战技");
                    }
                    else
                    {
                        SkillAttack();
                    }
                    break;
                case (ActionType.Extra, CommandStatus.Release, KeyCode.Space):
                    if (_currentAction.ExtraActCode == 0) Ultimate();
                    break;
                
            }
        }

        public override void GetAction(Action action)
        {
            if (action.Actor.Position != Position)
            {
                Debug.LogError(BaseData.Name + " Not My Action");
                return;
            }

            _currentAction = action;
            switch (action.ActionType)
            {
                case ActionType.Extra when action.ExtraActCode == 1:
                    ReadyTo(CommandStatus.SkillAttack);
                    GM.TargetLock(this);
                    break;
                case ActionType.Followup when action.ExtraActCode == 2:
                    ShowHiddenHand();
                    break;
                case ActionType.Followup when action.ExtraActCode == 3:
                    Autarky();
                    break;
                default:
                    base.GetAction(action);
                    break;
            }
            
            OnActionBegin();

        }

        protected override void ReadyTo(CommandStatus commandStatus)
        {
            if (commandStatus == CommandStatus.BasicAttack && HiddenHand)
            {
                CommandState = CommandStatus.BasicAttack;
                UI.SetInteractable(ButtonID.BasicAttack, false);
                UI.SetInteractable(ButtonID.SkillAttack, false);
                GM.SetCursorForm(EnhancedBasicAttackData.TargetForm, EnhancedBasicAttackData.TargetSide);
                return;
            }
            base.ReadyTo(commandStatus);
        }

        protected override void RegisterEvent()
        {
            EC.SubscribeEvent(EventID.FriendlyTurnBegin, (_, _) => EventCollectJade());
        }

        private void EventCollectJade()
        {
            // 青雀不关心谁的回合开始了，只知道要抽牌了
            Debug.Log("Event: FriendlyTurnBegin, Qingque Collect Jade");
            CollectJades(1);
        }
        
        private void CollectJades(int count)
        {
            while (count > 0)
            {
                Jades[Random.Range(1, 4)] += 1;
                count -= 1;   
            }

            while (Jades.Sum() > 4)
            {
                Jades[MinJadeType()] -= 1;
            }
        }

        private int MaxJadeType()
        {
            int maxJadeType = 0, maxJadeCount = 0;
            for (int i = 1; i < 4; i++)
            {
                if (Jades[i] > maxJadeCount)
                {
                    maxJadeType = i;
                    maxJadeCount = Jades[i];
                }
            }
            return maxJadeType;
        }

        /// <summary>
        /// The Minimum Jade Type except i=0 or Jades[i] = 0
        /// Return Range: 1 ~ 3;
        /// </summary>
        /// <returns></returns>
        private int MinJadeType()
        {
            int minJadeType = -1, minJadeCount = 8;
            for (int i = 1; i < 4; i++)
            {
                if (Jades[i] > 0 && Jades[i] < minJadeCount)
                {
                    minJadeType = i;
                    minJadeCount = Jades[i];
                }
            }

            if (minJadeType == -1)
            {
                throw new Exception("Qingque: Logic Error: No Jade to Remove");
            }

            return minJadeType;
        }
        private void RemoveAllJades()
        {
            for (int i = 0; i < 4; i++)
            {
                Jades[i] = 0;
            }
        }

        private void RemoveOneJade()
        {
            Jades[MinJadeType()] -= 1;
        }
        private bool CheckJade()
        {
            return Jades.Max() == 4;
        }
    }
}
