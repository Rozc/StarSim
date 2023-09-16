namespace Script.Enums
{
    public enum Message
    {
        // GM -> Object
        Spawn,
        TurnBegin,
        ActionBegin,
        ActionEnd,
        TurnEnd,
        Death,
        Interrupt,
        // Object -> GM
        ActionPrepared,
        ActionDone,
        MainActionDone,
        InteractDone, // 告知 GM 攻击动作已完成, 接下来是返回动作, 可以进行显示伤害, 调整行动序列等 UI 行为了
    }
}