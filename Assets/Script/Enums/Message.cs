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
        MainActionDone
    }
}