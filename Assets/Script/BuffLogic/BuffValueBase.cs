namespace Script.BuffLogic
{
    public enum BuffValueBase
    {
        None,
        Fixed, // 固定值
        SenderRealtime, // 基于施加者的当前数值的百分比
        TargetRealtime, // 基于目标的当前数值的百分比
        SenderFixed, // 基于施加者的固定值的百分比
        TargetFixed, // 基于目标的固定值的百分比
    }
}