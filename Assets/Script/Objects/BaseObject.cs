using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.Event;
using Script.InteractLogic;
using Script.Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Action = Script.ActionLogic.Action;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


namespace Script.Objects
{
    public abstract class BaseObject : MonoBehaviour // ������Ϸ������
    {
        [field: SerializeField] public ObjectData BaseData { get; private set; }
        // TODO 光锥数据和遗器数据
        
        // Realtime Data
        public RealtimeData Data { get; private set; }
        [field: SerializeField] protected List<Buff> BuffList; // 用 Dict 可能性能更好，但是为了在 Inspector 里方便查看，还是用 List 吧
        [field: SerializeField] protected List<Buff> DoTList;
        [field: SerializeField] protected List<Buff> CtrlList;
        

        // Animation Moving Settings
        protected Vector3 primitivePosition; 
        protected Vector3 primitiveRotation; 
        protected BaseObject _Target; 
        protected Vector3 movingTargetPosition; 
    
        [field: SerializeField] public int Position { get; private set; }
        [field: SerializeField] protected Action _currentAction;
        
        [field: SerializeField] protected MovingStatus MovingState = MovingStatus.Idle;
        
        public int Distance;
        
        // TODO
        public int ActionValue => Distance / (int)(Data.Get("Speed") * 100);
        protected GameManager GM;
        protected UIController UI;
        protected EventCenter EC;
        protected InteractManager IM;


        protected void MoveTo(Vector3 TargetPosition, float StopDistance)
        {
            // ��Ŀ���ƶ����� StopDistance ��Χ��ֹͣ
            if (Vector3.Distance(transform.position, TargetPosition) > StopDistance)
            {
                // Debug.LogFormat("MoveTo Moving, Atk: {0}", Atk);
                transform.LookAt(TargetPosition);
                transform.Translate(10 * Vector3.forward * Time.deltaTime);
                // transform.position = Vector3.MoveTowards(transform.position, TargetPosition, 0.5f);
            }
            else
            {
                // �ƶ��������״̬�л�
                if (MovingState == MovingStatus.MoveAttacking)
                {
                    // Debug.Log("Atk Stop");
                    movingTargetPosition = primitivePosition; // ��Ŀ��λ������Ϊ��ʼλ��
                    MovingState = MovingStatus.MoveReturning; // �л�״̬�� �ƶ���-���س�ʼλ��
                }
                else if (MovingState == MovingStatus.MoveReturning)
                {
                    // Debug.Log("Return Stop");
                    movingTargetPosition = _Target.transform.position; // ��Ŀ��λ������Ϊ����Ŀ��λ��
                    transform.eulerAngles = primitiveRotation; // ��ԭ����
                    MovingState = MovingStatus.Idle; // �л�״̬�� ����
                    MoveDone();
                }
            }
        }
        protected virtual void MoveDone()
        {
            // 清除所有当前回合的数据并通知 GM 行动结束
            _Target = null;
            _currentAction = null;
            GM.GetMessageFromActor(Message.ActionDone, Position);
        }

        protected void Start()
        {
            GM = GameManager.Instance;
            EC = EventCenter.Instance;
            UI = GameObject.Find("UIDocument").GetComponent<UIController>();
            IM = InteractManager.Instance;
            BuffList = new List<Buff>();
            Data = new RealtimeData(BaseData);


            MovingState = MovingStatus.Idle;
            Distance = 10000 * 100;
            
            RegisterEvent();
            
            primitivePosition = transform.position;
            primitiveRotation = transform.eulerAngles;
            movingTargetPosition = transform.position;
            
        }
        protected void Update()
        {
            switch (MovingState)
            {
                case MovingStatus.Idle:
                    break;
                case MovingStatus.MoveReturning:
                    MoveTo(movingTargetPosition, 0.1f);
                    break;
                case MovingStatus.MoveAttacking:
                    MoveTo(movingTargetPosition, 0.5f);
                    break;
            }
        }

        protected virtual void OnSpawn() {}

        protected virtual void OnTurnBegin()
        {
            // 检查 BuffList
            foreach (var buff in BuffList.Where(buff => buff.Data.CheckAtTurnBegin))
            {
                buff.CheckPointPassed = true;
            }
            // TODO 检查 DoTList 和 CtrlList
            
        }
        protected virtual void OnActionBegin() {}

        protected virtual void OnMainActionEnd()
        {
            // 检查 BuffList
            foreach (var buff in BuffList.Where(buff => buff.Data.CheckAtMainActionEnd))
            {
                buff.CheckPointPassed = true;
            }
        }
        protected virtual void OnActionEnd() {}

        protected virtual void OnTurnEnd()
        {
            // 检查 BuffList 并结算
            var dyingBuffList = new List<Buff>();
            foreach (var buff in BuffList)
            {
                if (buff.Data.DurationBaseOnTurn && buff.CheckPointPassed)
                {
                    buff.DurationLeft--;
                    if (buff.DurationLeft <= 0)
                    {
                        dyingBuffList.Add(buff);
                    }
                    else
                    {
                        buff.CheckPointPassed = false;
                    }
                } else if (buff.CurrentStack == 0 || buff.Data.MustBeRemovedAtTurnEnd)
                {
                    dyingBuffList.Add(buff);
                }
            }
            foreach (var buff in dyingBuffList)
            {
                RemoveBuff(buff);
            }
        }
        protected virtual void OnDeath() {}

        public abstract void GetAction(Action action);
        protected virtual void DoAction(SkillType skillType, TargetForm targetForm)
        {
            Debug.Log("DoAction() of " 
                      + BaseData.Name + " | " 
                      + _currentAction.ActionType + " Action | " 
                      + skillType + " Skill | "
                      + targetForm);
            MovingState = MovingStatus.MoveAttacking;
            // GM.GetMessageFromActor(Message.ActionDone, ID);
        }


        protected virtual void ActionInterrupt()
        {
            Debug.Log(BaseData.Name + ": Action Interrupted");
            _currentAction = null;
        }


        
        public virtual void OnTargeted(ActionDetail ad)
        {
            // 先处理 Buff，再处理数值
            // 需要将其中每个 Buff 的效果都应用到自己身上
            if (ad.Data.RemoveABuff)
            {
                
            }

            if (ad.Data.RemoveADebuff)
            {
                
            }

            foreach (var buffID in ad.Data.RemoveTheSpecifiedBuff)
            {
                if (HasBuff(buffID, out int idx))
                {
                    RemoveBuff(BuffList[idx]);
                }
            }
            
            foreach (var buffData in ad.Data.BuffDataList)
            {
                // Done: 检查身上是否已有该 Buff，如果有，检查是否可叠加，如果可叠加则叠加，否则仅更新持续时间
                // 有些 Buff 叠层之后效果也会叠加，考虑去掉原来的 Buff 然后加一个新的？
                // 要保证添加 Buff 和移除 Buff 是一个互补的操作
                
                // TODO 命中概率计算，需要获取施加者的效果命中
                float probability = buffData.Probability;
                if (!buffData.FixedProbability)
                {
                    // TODO 根据效果命中和效果抵抗，利用公式计算最终概率
                }
                if (Random.Range(0f, 99.99f) > probability)
                {
                    // 未命中
                    continue;
                }
                if (HasBuff(buffData.BuffID, out int idx))
                {
                    // TODO 这里可能还有得优化，不过先这样做了，也能在 UI 体现被更新的 Buff 会移到最前面
                    Buff buff = BuffList[idx];
                    RemoveBuff(buff);
                    if (buffData.Stackable)
                    {
                        buff.CurrentStack = Mathf.Min(buff.CurrentStack + buffData.StackAtATime, buffData.MaxStack);
                    }
                    buff.DurationLeft = buffData.Duration;
                    AddBuff(buff);
                }
                else
                {
                    AddBuff(new Buff(buffData, buffData.StackAtATime));
                }
                
            }
            switch (ad.Data.SkillType)
            {
                case SkillType.Attack:
                    // Data.CurrentHealth -= value;
                    if (! ad.Data.NotAnDiscreteAction)
                    {
                        EC.TriggerEvent(EventID.ObjectOnHit, this, ad.Actor);
                    }
                    break;
                case SkillType.Restore:
                    // Data.CurrentHealth += value;
                    if (! ad.Data.NotAnDiscreteAction)
                    {
                        EC.TriggerEvent(EventID.ObjectOnHeal, this, ad.Actor);
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual bool AddBuff(Buff buff)
        {
            foreach (var s in buff.Data.BuffProperties)
            {
                var ss = s.Split(':', '%');
                if (ss.Length >= 2 && ss[0].Trim() != "" && float.TryParse(ss[1], out float value))
                {
                    Data.Add(ss[0].Trim(), value * buff.CurrentStack, ss.Length == 3);
                    Debug.Log(Data.Name + " AddBuff: " + ss[0].Trim() + " " + value + "*" + buff.CurrentStack + " " + (ss.Length == 3 ? "%" : " Fixed") );
                }
            }
            BuffList.Add(buff);
            return true;
        }
        
        protected virtual void RemoveBuff(Buff buff)
        {
            foreach (var s in buff.Data.BuffProperties)
            {
                var ss = s.Split(':', '%');
                if (ss.Length >= 2 && ss[0].Trim() != "" && float.TryParse(ss[1], out float value))
                {
                    Data.Minus(ss[0].Trim(), value * buff.CurrentStack, ss.Length == 3);
                    Debug.Log(Data.Name + " RemoveBuff: " + ss[0].Trim() + " " + value + "*" + buff.CurrentStack + " " + (ss.Length == 3 ? "%" : " Fixed") );
                }
            }
            BuffList.Remove(buff);
        }

        public virtual void GetMessageFromGM(Message msg)
        {
            switch (msg)
            {
                case Message.Spawn:
                    OnSpawn();
                    break;
                case Message.TurnBegin:
                    OnTurnBegin();
                    break;
                case Message.ActionBegin:
                    OnActionBegin();
                    break;
                case Message.MainActionDone:
                    OnMainActionEnd();
                    break;
                case Message.ActionDone:
                    OnActionEnd();
                    break;
                case Message.TurnEnd:
                    OnTurnEnd();
                    break;
                case Message.Death:
                    OnDeath();
                    break;
                case Message.Interrupt:
                    ActionInterrupt();
                    break;
                default:
                    break;
            }
        }
        

        protected virtual void RegisterEvent()
        {
            
        }
        
        /// <summary>
        /// 在 BuffList 中查找是否有某个 Buff
        /// 若有，idx 为该 Buff 在 BuffList 中的索引
        /// 若无，idx = -1
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public bool HasBuff(int buffID, out int idx)
        {
            for (int i = 0; i < BuffList.Count; i++)
            {
                if (BuffList[i].Data.BuffID == buffID)
                {
                    idx = i;
                    return true;
                }
            }

            idx = -1;
            return false;
        }
        
        protected void SetTarget(BaseObject target)
        {
            _Target = target;
            movingTargetPosition = _Target.transform.position;
        }

    }
}
