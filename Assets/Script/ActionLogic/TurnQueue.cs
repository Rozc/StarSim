using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using Script.Objects;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Experimental.GraphView;

// TODO 将 Position 改成 CharaterID 实现
namespace Tools
{
    // ���ļ��������ж�������
    public class TurnQueue // �ж�����
    {
        private LinkedList<BaseObject> _list; // ˫������ÿ���ڵ��Ӧһ����Ϸ���󣬽ڵ�ֵΪ�ж�ֵ
        private Dictionary<int, LinkedListNode<BaseObject>> _dict; // ��Ϸ����ID <=> �ڵ�
        // private Dictionary<LinkedListNode<float>, int> _revDict; // �ڵ� <=> ��Ϸ����ID


        public TurnQueue(int capacity = 16)
        {
            _list = new LinkedList<BaseObject>();
            _dict = new Dictionary<int, LinkedListNode<BaseObject>>(capacity);
            // _revDict = new Dictionary<LinkedListNode<float>, int>(capacity);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public void Push(BaseObject obj)
        {
            LinkedListNode<BaseObject> newNode = new(obj); // �½��ڵ�
            _list.AddLast(newNode); // ���ڵ���ӵ�����β��
            _dict[obj.Data.CharacterID] = newNode; // ���ڵ���ӵ��ֵ�
            Advance(newNode);
        }

        public BaseObject Pop() // ��������ͷ���ڵ��Ӧ����Ϸ���󣬲��Ƴ��ýڵ�
        {
            BaseObject obj = _list.First.Value; // ��ȡ����ͷ���ڵ��Ӧ����Ϸ����ID
            Remove(obj.Position); // �Ƴ��� ID ��Ӧ�Ľڵ�
            return obj; // ���ظ� ID
        }
        public BaseObject Top() // ��������ͷ���ڵ��Ӧ����Ϸ����ID
        {
            return _list.First.Value;
        }
        public void MoveHeadToTail(int Distance)
        {
            LinkedListNode<BaseObject> head = _list.First;
            head.Value.Distance = Distance;
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
        public void Update(int id)
        {
            if (_dict.ContainsKey(id))
            {
                Update(_dict[id]);
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

        public void Remove(int id) // �Ƴ� ID ��Ӧ�Ľڵ�
        {
            _list.Remove(_dict[id]);
            _dict.Remove(id);

        }

        public void Clear() // ����ж�����
        {
            _list.Clear();
            _dict.Clear();
        }

        public Dictionary<int, int> DisplayDict() // ���� (ID <=> �ж�ֵ) ���ֵ䣬�Ѱ��ж�ֵ��������
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            foreach (var item in _list)
            {
                dict.Add(item.Data.CharacterID, item.ActionValue);
            }
            return dict;
        }

        public override string ToString() // ���������е��ж������ֵ���ַ�����ʾ
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
