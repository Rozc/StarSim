namespace Script.BuffLogic
{
    [System.Serializable]
    public class BuffProperty
    {
        public string propName;
        public BuffValueBase valueBasedOn;
        public float value;
        
        public BuffValueBase maxBasedOn;
        public string maxPropName;
        public float max;
        
        public BuffValueBase minBasedOn;
        public string minPropName;
        public float min;
    }
}