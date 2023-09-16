namespace Script.Enums
{
    public enum MovingStatus
    {
        Idle = 0,
        MoveAttacking,
        MoveReturning,

    }

    public enum ActionStatus
    {
        None = 0,
        BaseAction,
        Ultimate,
        ExtraAction
    }

    public enum CommandStatus
    {
        None = 0,
        BasicAttack,
        SkillAttack,
        Release,
    }
}