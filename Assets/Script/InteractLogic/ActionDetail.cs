using Script.BuffLogic;
using Script.Data;
using Script.Objects;
using Script.Tools;

namespace Script.InteractLogic
{
    /// <summary>
    /// 此类用于实现游戏对象之间的交互逻辑，主要是行动细节
    /// 行动细节表示本次行动对哪些对象造成了哪些影响
    /// 影响包括：伤害、治疗、增益、减益等
    /// </summary>
    [System.Serializable]
    public class ActionDetail
    {
        public BaseObject Actor;
        public BaseObject Target;
        public ActionDataBase Data;

        public ActionDetail(BaseObject actor, BaseObject target, ActionDataBase data)
        {
            Actor = actor;
            Target = target;
            Data = data;
        }

        
        // 以下是数值字段
        // 当影响为伤害时，需要填写所有字段
        // 当影响为治疗时，需要填写至 Boost 字段
        // 当影响为增益和减益时，不需要填写任何字段
        // 影响可以组合，但伤害和治疗显然不能组合
        // 也就是伤害/治疗可以附带增益/减益
        public int BaseValue; // 基础值，即攻击力、防御力、生命值等，
        public int Multiple; // 倍率
        public int Boost; // 增伤乘区 或 治疗量加成
        public int DefencePenetration; // 防御穿透
        public int ResistancePenetration; // 抗性穿透
        public int CritRate; // 暴击率
        public int CritDamage; // 暴击伤害

    }
}