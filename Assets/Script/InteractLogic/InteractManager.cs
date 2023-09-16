using Script.Enums;
using Script.Objects;
using Script.Tools;


namespace Script.InteractLogic
{
    /// <summary>
    /// 伤害交互管理器
    /// 职责：接收行动细节并转发给目标
    /// 主要功能在于将扩散伤害/Aoe伤害 *拆* 成对每个目标的伤害然后发送给受击目标
    /// 也就是这边需要拿到所有对象脚本的引用
    /// </summary>
    public class InteractManager : SingletonBase<InteractManager>
    {
        public void Process(ActionDetail ad)
        {
            // TODO
            // 根据行动细节计算影响
            // 通知目标
            switch (ad.Data.TargetForm)
            {
                case TargetForm.Single:
                    Single(ad);
                    break;
            }
        }
        
        // 干脆把整个 ad 发给目标让人自己算得了
        private void Single(ActionDetail ad)
        {
            ad.Target.OnTargeted(ad);
        }
        private void Blast(ActionDetail ad)
        {
            
        }
        private void Bounce(ActionDetail ad)
        {
            
        }
        private void Aoe(ActionDetail ad)
        {
            
        }
    }
}