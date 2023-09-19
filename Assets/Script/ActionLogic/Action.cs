using Script.Enums;
using Script.Objects;


// Action 类型
// 每次对象进行行动时，都会生成一个 Action 对象


// TODO 重载的用于比较的运算符仅用于比较两个行动的优先级关系，不代表行动是否相等！考虑改用其他方法
// 判断行动是否相等使用 UAID，UAID 由 GameManager 实时分配，使用 Equals 方法


namespace Script.ActionLogic
{
    [System.Serializable]
    public class Action
    {

        public int UAID = -1; // Unique Action ID
        public BaseObject Actor;
        public ActionType ActionType;
        public ActionPriority Priority; // 优先级
        
        // 以下字段仅在行动为额外行动是有效
        public bool Insertable; // 是否可被插队
        public int ExtraActCode; // 额外行动代码，在角色有自定义额外行动时自行实现，0 为终结技，-1 为主行动

    
        public Action(
            BaseObject actor, 
            ActionType actionType = ActionType.Base,
            ActionPriority priority = ActionPriority.Base,
            int extraActCode = -1,
            bool insertable = true)
        {
            Actor = actor;
            ActionType = actionType;
            Priority = priority;
            ExtraActCode = extraActCode;
            Insertable = insertable;
        }
    

    

        public static bool operator >(Action left, Action right)
        {
            return left.Priority > right.Priority;
        }
        public static bool operator <(Action left, Action right)
        {
            return left.Priority < right.Priority;
        }
        public static bool operator ==(Action left, Action right)
        {
            return left.Priority == right.Priority;
        }
        public static bool operator !=(Action left, Action right)
        {
            return left.Priority != right.Priority;
        }
        public static bool operator >=(Action left, Action right)
        {
            return left.Priority >= right.Priority;
        }
        public static bool operator <=(Action left, Action right)
        {
            return left.Priority <= right.Priority;
        }

    
        public bool Equals(Action other)
        {
            return UAID == other.UAID;
        }
    }
}
