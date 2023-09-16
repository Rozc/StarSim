namespace Script.Event.EventArgs
{
    public class AttackEventArgs : System.EventArgs
    {
        public int ActorID;
        public int TargetID;
        public AttackEventArgs(int actorID, int targetID = -1)
        {
            ActorID = actorID;
            TargetID = targetID;
        }
    }
}

