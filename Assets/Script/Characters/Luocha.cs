using System.Linq;
using Script.ActionLogic;
using Script.Data;
using Script.Enums;
using Script.InteractLogic;
using Script.Objects;
using UnityEngine;

namespace Script.Characters
{
    public sealed class Luocha : Friendly
    {
        // 罗刹需要在失去自动战技冷却中的 Buff 时立刻检测当前生命值最低的角色的生命值是否低于 50%
        // 并且当多个角色生命值同时降至 50% 以下时, 其需要选择生命值最低的角色施放自动战技
        // 做起来还挺麻烦的, 而且还要给除了自己以外的队友上 不随回合减少的 Buff, 但是自己的 Buff 又是随回合减少的
        // 结界回血的触发条件应该是我方释放了攻击型技能
        // 从控制类负面效果中恢复后好像也会触发急救

        // ExtraActCode
        // 1: Field
        // 2: Auto Skill

        [field: SerializeField] private ActionDataBase FieldDeployData;
        [field: SerializeField] private ActionDataBase FieldDeployOtherData;
        [field: SerializeField] private ActionDataBase AutoSkillData;
        [field: SerializeField] private ActionDataBase AutoSkillCDData;
        [field: SerializeField] private ActionDataBase FieldHealMainData;
        [field: SerializeField] private ActionDataBase FieldHealOtherData;
        
        [field: SerializeField] private int _currentStackOfAbyssFlower = 0;
        [field: SerializeField] private bool _isFieldActive = false;
        [field: SerializeField] private bool _isAutoSkillInCD = false;
        
        protected override void SkillAttack()
        {
            if (!_isFieldActive)
            {
                _currentStackOfAbyssFlower++;
            }
            Act(SkillAttackData, afterInteractDone: CheckField);
        }

        protected override void Ultimate()
        {
            if (!_isFieldActive)
            {
                _currentStackOfAbyssFlower++;
            }
            Act(UltimateData, afterInteractDone: CheckField);
        }

        private void CheckField()
        {
            Debug.Log("Luocha: CheckField, at afterInteractDone");
            if (_currentStackOfAbyssFlower == 2)
            {
                _currentStackOfAbyssFlower = 0;
                Action action = new(
                    this,
                    ActionType.Followup,
                    ActionPriority.Luocha_Field,
                    1);
                GM.RequireExtraAction(action);
            }
        }

        private void DeployField()
        {
            _isFieldActive = true;
            // 给自己上一个持续两回合的 Buff
            // 给自己以外的队友上一个不随回合减少的 Buff
            // 当自己的 Buff 结束时，移除队友身上的这个 Buff
            Act(FieldDeployData, this, afterIMProcessed: DeployFieldAttachment);
        }

        private void DeployFieldAttachment()
        {
            foreach (var friendly in GM.FriendlyObjects.Where(o => o.Position is >= 1 and <= 4))
            {
                if (friendly == this) continue;
                ActionDetail ad = new ActionDetail(this, friendly, FieldDeployOtherData);
                IM.Process(ad);
            }
        }

        private void TryAutoSkill()
        {
            float lowestHPRatio = 1;
            Friendly target = null;
            foreach (var friendly in GM.FriendlyObjects.Where(o => o.Position is >= 1 and <= 4))
            {
                float ratio = friendly.Data.CurrentHealth / friendly.Data.Get("Health");
                if (ratio <= 0.5 && ratio < lowestHPRatio)
                {
                    lowestHPRatio = ratio;
                    target = friendly;
                }
            }

            if (target is not null)
            {
                AutoSkill(target);
            }
            else
            {
                // TODO 把行动取消可以包装一下
                _currentAction = null;
                _target = null;
                GM.GetMessageFromActor(Message.ActionCanceled);
            }
        }
        private void AutoSkill(BaseObject target)
        {
            _isAutoSkillInCD = true;
            // 给自己上一个持续两回合的 Buff，进入 CD
            // 当 Buff 结束时，修改 CD 标记
            // 找到生命值百分比最低的角色
            if (!_isFieldActive) _currentStackOfAbyssFlower++;
            Act(AutoSkillData, target, afterIMProcessed: AutoSkillAttachment, afterInteractDone: CheckField);
        }
        private void AutoSkillAttachment()
        {
            ActionDetail ad = new ActionDetail(this, this, AutoSkillCDData);
            IM.Process(ad);
        }

        public void OnFieldRemove(BaseObject _)
        {
            _isFieldActive = false;
            foreach (var friendly in GM.FriendlyObjects.Where(o => o.Position is >= 1 and <= 4))
            {
                if (friendly.HasBuff(1000301, out var buff))
                {
                    friendly.RemoveTheBuff(buff);
                }
            }
        }

        public void OnAutoSkillReady()
        {
            _isAutoSkillInCD = false;
            TryAutoSkill();
        }

        public void CallBackField(BaseObject _)
        {
            EC.SubscribeEvent(EventID.FriendlyAttack, EventFriendlyAttack);
        }

        private void EventFriendlyAttack(BaseObject sender, BaseObject other)
        {
            if (!_isFieldActive) return;
            IM.Process(new ActionDetail(this, sender, FieldHealMainData));
            foreach (var friendly in GM.FriendlyObjects.Where(o => o.Position is >= 1 and <= 4))
            {
                if (friendly == sender) continue;
                IM.Process(new ActionDetail(this, friendly, FieldHealOtherData));
            }
        }

        public override void GetAction(Action action)
        {
            if (action.Actor.Position != Position)
            {
                Debug.LogError( "Luocha Get Action Error: Not My Action! Actor ID: " + action.Actor.Position + " My ID: " + Position);
                return;
            }
            _currentAction = action;
            switch (action.ExtraActCode)
            {
                case 1:
                    DeployField();
                    break;
                case 2:
                    TryAutoSkill();
                    break;
                default:
                    base.GetAction(action);
                    break;
            }
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            EC.SubscribeEvent(EventID.HPDownTo50Precent, EventHPDownTo50Precent);
        }
        private void EventHPDownTo50Precent(BaseObject sender, BaseObject other)
        {
            if (_isAutoSkillInCD || sender is not Friendly) return;
            // 不记录释放目标，在释放时寻找我方生命值百分比最低的角色
            GM.RequireExtraAction(new Action(
                this,
                ActionType.Followup,
                ActionPriority.Luocha_AutoSkill,
                2));
        }
    }
}