using System.Collections.Generic;
using Script.Objects;

namespace Script.ActionLogic
{
    public class TurnQueue
    {
        private LinkedList<BaseObject> _list;
        private Dictionary<int, LinkedListNode<BaseObject>> _dict;


        public TurnQueue(int capacity = 16)
        {
            _list = new LinkedList<BaseObject>();
            _dict = new Dictionary<int, LinkedListNode<BaseObject>>(capacity);
        }

        public int Count => _list.Count;

        public void Push(BaseObject obj)
        {
            LinkedListNode<BaseObject> newNode = new(obj);
            _list.AddLast(newNode);
            _dict[obj.Data.CharacterID] = newNode;
            Advance(newNode);
        }

        public BaseObject Pop()
        {
            BaseObject obj = _list.First.Value;
            Remove(obj.Position);
            return obj;
        }
        public BaseObject Top()
        {
            return _list.First.Value;
        }
        public void MoveHeadToTail(int distance)
        {
            // TODO 这里应该让游戏对象自己修改 Distance 值
            LinkedListNode<BaseObject> head = _list.First;
            head.Value.Distance = distance;
            _list.RemoveFirst();
            _list.AddLast(head);
            Advance(head);
            PushForward();
            
        }

        public void PushForward() // Push the queue to the next actor
        {
            LinkedListNode<BaseObject> node = _list.First;
            int headActionValue = node.Value.ActionValue;

            while (node != null)
            {
                node.Value.Distance -= headActionValue * (int)(node.Value.Data.Get("Speed") * 100);
                node = node.Next;
            }
        }
        public void Update(params int[] ids)
        {
            foreach (var id in ids)
            {
                if (_dict.ContainsKey(id))
                {
                    Update(_dict[id]);
                }
            }
        }
        public void Update(LinkedListNode<BaseObject> node)
        {
            if (node == null)
            {
                return;
            }
            if (node.Previous != null && node.Value.ActionValue < node.Previous.Value.ActionValue)
            {
                Advance(node);
            }
            else if (node.Next != null && node.Value.ActionValue > node.Next.Value.ActionValue)
            {
                Delay(node);
            }
        }

        public void Advance(int id)
        {
            if(_dict.ContainsKey(id))
            {
                Advance(_dict[id]);
            }
            
        }
        public void Advance(LinkedListNode<BaseObject> node)
        {
            if(node == null)
            {
                return;
            }
            LinkedListNode<BaseObject> prev = node.Previous;
            while (prev != null && node.Value.ActionValue < prev.Value.ActionValue)
            {
                prev = prev.Previous;
            }
            _list.Remove(node);
            if (prev == null)
            {
                _list.AddFirst(node);
            } else
            {
                _list.AddAfter(prev, node);
            }
        }
        public void Delay(int id)
        {
            if (_dict.ContainsKey(id))
            {
                Delay(_dict[id]);
            }
        }
        public void Delay(LinkedListNode<BaseObject> node)
        {
            if (node == null)
            {
                return;
            }
            LinkedListNode<BaseObject> next = node.Next;
            while (next != null && node.Value.ActionValue > next.Value.ActionValue)
            {
                next = next.Next;
            }
            _list.Remove(node);
            if (next == null)
            {
                _list.AddLast(node);
            }
            else
            {
                _list.AddBefore(next, node);
            }
        }

        public void Remove(int id)
        {
            _list.Remove(_dict[id]);
            _dict.Remove(id);

        }

        public void Clear()
        {
            _list.Clear();
            _dict.Clear();
        }

        public Dictionary<int, int> DisplayDict()
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            foreach (var item in _list)
            {
                dict.Add(item.Data.CharacterID, item.ActionValue);
            }
            return dict;
        }

        public override string ToString()
        {
            string str = "";
            Dictionary<int, int> dict = DisplayDict();
            foreach (var item in dict)
            {
                str += item.Key + " <=> " + item.Value + "\r\n";
            }
            return str;
        }


    }


}
