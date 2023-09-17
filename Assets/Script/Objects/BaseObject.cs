using System;
using System.Collections.Generic;
using System.Linq;
using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.Event;
using Script.InteractLogic;
using UnityEngine;
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
        [field: SerializeField] public bool isAlive = true;
        [field: SerializeField] protected List<Buff> BuffList; // 用 Dict 可能性能更好，但是为了在 Inspector 里方便查看，还是用 List 吧
        [field: SerializeField] protected List<Buff> DoTList;
        [field: SerializeField] protected List<Buff> CtrlList;
        

        // Animation Moving Settings
        protected Vector3 primitivePosition; 
        protected Vector3 primitiveRotation; 
        protected BaseObject _target; 
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

        private CursorController _cursorMain;
        private CursorController _cursorSub;


        protected void MoveTo(Vector3 TargetPosition, float StopDistance)
        {
            // ��Ŀ���ƶ����� StopDistance ��Χ��ֹͣ
            if (Vector3.Distance(transform.position, TargetPosition) > StopDistance)
            {
                transform.LookAt(TargetPosition);
                transform.Translate(10 * Vector3.forward * Time.deltaTime);
            }
            else
            {
                if (MovingState == MovingStatus.MoveAttacking)
                {
                    movingTargetPosition = primitivePosition;
                    MovingState = MovingStatus.MoveReturning;
                    GM.GetMessageFromActor(Message.InteractDone, Data.CharacterID);
                }
                else if (MovingState == MovingStatus.MoveReturning)
                {
                    transform.eulerAngles = primitiveRotation;
                    MovingState = MovingStatus.Idle;
                    MoveDone();
                }
            }
        }
        protected virtual void MoveDone()
        {
            // 清除所有当前回合的数据并通知 GM 行动结束
            _target = null;
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
            Debug.Log("        DoAction() of " 
                      + BaseData.Name + " | " 
                      + _currentAction.ActionType + " Action | " 
                      + skillType + " Skill | "
                      + targetForm);
            MovingState = MovingStatus.MoveAttacking;
            // GM.GetMessageFromActor(Message.ActionDone, ID);
        }


        protected virtual void ActionInterrupt()
        {
            Debug.Log("    " + BaseData.Name + ": Action Interrupted");
            _currentAction = null;
        }


        
        public virtual void OnTargeted(ActionDetail ad)
        {
            // 先处理 Buff，再处理数值
            // 需要将其中每个 Buff 的效果都应用到自己身上
            if (ad.Data.RemoveABuff)
            {
                // 反向遍历
                for (int i = BuffList.Count - 1; i >= 0; i--)
                {
                    if (BuffList[i].Data.BuffType != BuffType.Buff) continue;
                    RemoveBuff(BuffList[i]);
                    break;
                }
            }

            if (ad.Data.RemoveADebuff)
            {
                // 遍历所有 Debuff，保存第一个找到的
                // 继续遍历，如果遇到了需要首先被移除的，就替换然后停止遍历，清掉这个 Debuff
                // 如果遍历完了都没找到需要首先被移除的，就清掉第一个找到的
                // 反向遍历
                Buff debuffToRemove = null;
                bool found = false;
                for (int i = BuffList.Count - 1; i >= 0; i--)
                {
                    if (BuffList[i].Data.BuffType == BuffType.Debuff)
                    {
                        if (!found)
                        {
                            debuffToRemove = BuffList[i];
                            found = true;
                        }
                        if (BuffList[i].Data.NeedToBeRemovedFirst)
                        {
                            debuffToRemove = BuffList[i];
                            break;
                        }
                    }
                }

                if (found)
                {
                    RemoveBuff(debuffToRemove);
                }
                    
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
                var propName = ss[0].Trim();
                if (ss.Length >= 2 && propName != "" && float.TryParse(ss[1], out float value))
                {
                    if (propName == "Speed") EC.TriggerEvent(EventID.ActionValueUpdate, this);
                    if (propName == "Distance")
                    {
                        // 一般表现为行动提前 %，或行动延后 %，并且即时生效
                        Distance += (int)(value * 0.01f * Distance) * buff.CurrentStack;
                        EC.TriggerEvent(EventID.ActionValueUpdate, this);
                        continue;
                    }
                    Data.Add(propName, value * buff.CurrentStack, ss.Length == 3);
                    Debug.Log("        " + Data.Name 
                              + " AddBuff: " 
                              + propName + " " 
                              + value + "*" + buff.CurrentStack + " " 
                              + (ss.Length == 3 ? "%" : " Fixed") );
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
                var propName = ss[0].Trim();
                if (ss.Length >= 2 && propName != "" && float.TryParse(ss[1], out float value))
                {
                    if (propName == "Speed") EC.TriggerEvent(EventID.ActionValueUpdate, this);
                    if (propName == "Distance") continue; // 对于距离的改变，改了就不会还了，所以只需要在加 Buff 时处理

                    Data.Minus(propName, value * buff.CurrentStack, ss.Length == 3);
                    Debug.Log("        " + Data.Name 
                              + " RemoveBuff: " 
                              + propName + " " 
                              + value + "*" + buff.CurrentStack + " " 
                              + (ss.Length == 3 ? "%" : " Fixed") );
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
        
        public void GetCursor(CursorController cursorMain, CursorController cursorSub)
        {
            _cursorMain = cursorMain;
            _cursorSub = cursorSub;
            var position = transform.position;
            cursorMain.transform.position = position;
            cursorSub.transform.position = position;
        }
        
        public void ShowCursor(bool show, bool main = true)
        {
            if (!show)
            {
                _cursorMain.Hide();
                _cursorSub.Hide();
                return;
            }
            if (main)
            {
                _cursorMain.Show();
                _cursorSub.Hide();
            }
            else
            {
                _cursorMain.Hide();
                _cursorSub.Show();
            }
        }
        public bool TryGetLeft(out BaseObject obj)
        {
            return GM.PosDict.TryGetValue(Position - 1, out obj);
        }
        public bool TryGetRight(out BaseObject obj)
        {
            return GM.PosDict.TryGetValue(Position + 1, out obj);
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
        
        protected void SetTarget(BaseObject target, bool aoe = false, bool friendly = false)
        {
            if (aoe)
            {
                _target = null;
                movingTargetPosition = friendly
                    ? GM.FriendlyCenter.transform.position
                    : GM.EnemyCenter.transform.position;
            }
            else if (target is not null)
            {
                _target = target;
                movingTargetPosition = _target.transform.position;
            }
            else
            {
                throw new Exception("SetTarget Error: Target is null");
            }

        }
        
        
        
        

    }
}
