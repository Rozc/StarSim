using System;
using System.Linq;
using Script.Data;
using Script.Data.ActionData.AttackData;
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

        private new void Start()
        {
            base.Start();
            Jades = new int[] {0, 0, 0, 0};
        }
        

        protected override void OnActionBegin()
        {
            base.OnActionBegin();
            // 实际游戏里可能没做 !HiddenHand 这一个检查，因为即使杠了再放终结技，放完也会有自摸的动画
            // TODO 生成 ad，加buff
            if (_currentAction.ActionType is ActionType.Base 
                || _currentAction.ExtraActCode == 1)
            {
                if (HiddenHand)
                {
                    ReadyTo(CommandStatus.BasicAttack);
                    GM.SetCursorForm(TargetForm.Blast, TargetSide.Enemy);
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
        
        protected override void BasicAttack()
        {
            SetTarget(GM.CurrentTarget);
            ActionDetail ad;
            if (HiddenHand)
            {
                RemoveAllJades();
                ad = new ActionDetail(this, _target, EnhancedBasicAttackData);
            }
            else
            {
                RemoveOneJade();
                ad = new ActionDetail(this, _target, BasicAttackData);
            }
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            bool autarky = HasBuff(1000002, out _);
            if (autarky)
            {
                Action action = new Action(
                    this,
                    ActionType.Followup,
                    ActionPriority.Qingque_Autarky,
                    extraActCode:3);
                GM.RequireExtraAction(action);
            }
            DoAction(SkillType.Attack, HiddenHand ? TargetForm.Blast : TargetForm.Single);
            if (!autarky) HiddenHand = false;
        }

        private void Autarky()
        {
            SetTarget(GM.CurrentTarget);
            ActionDetail ad;
            if (HiddenHand)
            {
                RemoveAllJades();
                ad = new ActionDetail(this, _target, AutarkyEnhancedData);
            }
            else
            {
                RemoveOneJade();
                ad = new ActionDetail(this, _target, AutarkyBasicData);
            }
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Attack, HiddenHand ? TargetForm.Blast : TargetForm.Single);
            HiddenHand = false;
        }
        protected override void SkillAttack()
        {
            if (HiddenHand)
            {
                Debug.LogError("Qingque: Logic Error: Already Hidden Hand");
                return;
            }
            CollectJades(2);

            // Generate Extra Action
            Action action = new (
                this,
                ActionType.Extra,
                ActionPriority.Qingque_ExtraAction,
                1);
            
            SetTarget(this);
            ActionDetail ad = new ActionDetail(this, this, SkillAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            GM.RequireExtraAction(action);
            DoAction(SkillType.Enhance, TargetForm.Single);

        }
        protected override void Ultimate()
        {
            RemoveAllJades();
            Jades[0] = 4;
            SetTarget(null, true);
            ActionDetail ad = new ActionDetail(this, this, UltimateData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Attack, TargetForm.Aoe);
        }

        private void ShowHiddenHand()
        {
            HiddenHand = true;
            ActionDetail ad = new ActionDetail(this, this, HiddenHandData);
            SetTarget(this);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            DoAction(SkillType.Enhance, TargetForm.Single);
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
                    CommandState = CommandStatus.SkillAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, true);
                    UI.SetInteractable(ButtonID.SkillAttack, false);
                    break;
                case (ActionType.Base, CommandStatus.SkillAttack, KeyCode.Q):
                case (ActionType.Extra, CommandStatus.SkillAttack, KeyCode.Q):
                    CommandState = CommandStatus.BasicAttack;
                    UI.SetInteractable(ButtonID.BasicAttack, false);
                    UI.SetInteractable(ButtonID.SkillAttack, true);
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
            if (action.ActionType is ActionType.Extra && action.ExtraActCode == 1)
            {
                UI.SetInteractable(ButtonID.SkillAttack, false);
                CommandState = CommandStatus.SkillAttack;
            }
            else if (action.ActionType is ActionType.Followup && action.ExtraActCode == 2)
            {
                ShowHiddenHand();
            }
            else if (action.ActionType is ActionType.Followup && action.ExtraActCode == 3)
            {
                Autarky();
            }
            else
            {
                base.GetAction(action);
            }
            
            OnActionBegin();

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
