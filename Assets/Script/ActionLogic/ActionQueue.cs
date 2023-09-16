using System.Collections.Generic;
using System.Linq;
using Script.ActionLogic;
using Script.Enums;
using Script.Tools;

namespace Script.ActingLogic
{
    // 本文件定义了用于当前回合的行动的行动序列类
    public class ActionQueue // 行动序列
    {
        private LinkedList<Action> _list; // 双向链表，每个节点对应一个 Action
        public ActionQueue(int capacity = 16)
        {
            _list = new LinkedList<Action>();
        }
        public int Count => _list.Count;

        /// <summary>
    /// Push an action into the queue.
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Return true if the action is inserted at the head of the queue.</returns>
        public bool Push(Action action)
        {
            LinkedListNode<Action> newNode = new LinkedListNode<Action>(action); // 新建节点
            _list.AddLast(newNode); // 将节点添加到链表尾部
            int currentTopUAID = _list.First.Value.UAID;
            Advance(newNode); // 向上更新节点位置
            if (currentTopUAID != _list.First.Value.UAID) // 如果新节点在链表头部，返回 true
            {
                return true;
            }
            else // 否则返回 false
            {
                return false;
            }
        }

        public Action Pop() // 返回链表头部节点对应 Action 并出队
        {
            Action action = _list.First.Value;
            _list.RemoveFirst();
            return action;
        }
        public Action Top() // 返回链表头部节点对应 Action
        {
            return _list.First.Value;
        }

        public void Advance(LinkedListNode<Action> node)
        {
            if (node == null)
            {
                return;
            }
            LinkedListNode<Action> prev = node.Previous;
            while (prev != null && prev.Value.Insertable && node.Value < prev.Value)
            {
                prev = prev.Previous;
            }
            _list.Remove(node);
            if (prev == null)
            {
                _list.AddFirst(node);
            }
            else
            {
                _list.AddAfter(prev, node);
            }
        }

        public void Remove(int uaid) // 移除 UAID 对应的行动，用于行动撤销
        {
            LinkedListNode<Action> node = _list.First;
            while (node != null)
            {
                if (node.Value.UAID == uaid)
                {
                    _list.Remove(node);
                    return;
                }
                node = node.Next;
            }
        }

        public void Clear() // 清空行动序列
        {
            _list.Clear();
        }

        public Dictionary<string, ActionPriority> DisplayDict() // 返回 (ActorID <=> Priority) 的字典，已按优先级升序排列
        {
            var dict = new Dictionary<string, ActionPriority>();
            foreach (var item in _list)
            {
                dict.Add(item.Actor.BaseData.Name + item.UAID, item.Priority);
            }
            dict = dict.OrderBy(o => o.Value).ToDictionary(o => o.Key, p => p.Value); // 是否需要排序存疑
            return dict;
        }

        public override string ToString() // 返回已排列的额外行动序列字典的字符串表示
        {
            string str = "";
            Dictionary<string, ActionPriority> dict = DisplayDict();
            foreach (var item in dict)
            {
                str += item.Key + " <=> " + item.Value + "\r\n";
            }
            return str;
        }

    }


}
