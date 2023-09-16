using System.Collections.Generic;
using Script.Enums;
using Script.Objects;
using Script.Tools;
using Tools;

namespace Script.Event
{
    public class EventCenter : SingletonBase<EventCenter>
    {
        public delegate void Handler(BaseObject sender, BaseObject other);
        private Dictionary<EventID, Handler> eventTable = new ();

        public void SubscribeEvent(EventID eventType, Handler handler)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] += handler;
            } else
            {
                eventTable.Add(eventType, handler);
            }
        }
        public void UnsubscribeEvent(EventID eventType, Handler handler)
        {
            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] -= handler;
            }
        }
        public void TriggerEvent(EventID eventType, BaseObject sender, BaseObject other = null)
        {
            if (eventTable.ContainsKey(eventType))  
            {
                eventTable[eventType]?.Invoke(sender, other);
            }
        }
        public void Clear()
        {
            eventTable.Clear();
        }

    }
}
