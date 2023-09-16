using Script.Enums;
using Script.InteractLogic;
using Script.Objects;

namespace Script.Characters
{
    public class Bronya : Friendly
    {
        protected override void BasicAttack()
        {
            base.BasicAttack();
        }

        protected override void SkillAttack()
        {
            // 先上 Buff
            // 然后解除负面效果
            // 最后使目标立即行动
            SetTarget(GM.PosDict[1]);
            ActionDetail ad = new ActionDetail(this, _target, SkillAttackData);
            GM.GetMessageFromActor(Message.ActionPrepared);
            IM.Process(ad);
            // 拉条
            DoAction(SkillType.Enhance, TargetForm.Single);
        }

        protected override void Ultimate()
        {
            // TODO 在 Buff String 里实现一定程度的表达式计算
            base.Ultimate();
        }
    }
}