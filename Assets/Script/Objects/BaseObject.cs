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
        [field: SerializeField] public RealtimeData Data { get; private set; }
        [field: SerializeField] public bool isAlive = true;
        

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

        
        // ====================================== OnStage ======================================
        protected virtual void OnSpawn() {}

        protected virtual void OnTurnBegin()
        {
            // 检查 BuffList
            foreach (var buff in Data.BuffList.Where(buff => buff.Data.CheckAtTurnBegin))
            {
                buff.CheckPointPassed = true;
            }
            // TODO 检查 DoTList 和 CtrlList
            
        }
        protected virtual void OnActionBegin() {}

        protected virtual void OnMainActionEnd()
        {
            // 检查 BuffList
            foreach (var buff in Data.BuffList.Where(buff => buff.Data.CheckAtMainActionEnd))
            {
                buff.CheckPointPassed = true;
            }
            EC.TriggerEvent(EventID.ActionEnd, this);
        }

        protected virtual void OnActionEnd()
        {
            EC.TriggerEvent(EventID.ActionEnd, this);
        }

        protected virtual void OnTurnEnd()
        {
            // 检查 BuffList 并结算
            var dyingBuffList = new List<Buff>();
            foreach (var buff in Data.BuffList)
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

        
        
        // ====================================== Action ======================================
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
        
        
        
        // ====================================== Damage ======================================
        public virtual void ReceiveDamage(float value, BaseObject actor, bool trigger = true)
        {
            // Data...
            // TODO Data...
            if (trigger) EC.TriggerEvent(EventID.ObjectOnHit, this, actor);
        }
        public virtual void ReceiveHealing(float value, BaseObject actor, bool trigger = true)
        {
            // Data...
            // TODO Data...
            if (trigger) EC.TriggerEvent(EventID.ObjectOnHeal, this, actor);
        }
        
        
        
        // ===================================== Buff =======================================
        public virtual void RemoveABuff(BuffType buffType)
        {
            // 遍历所有 Debuff，保存第一个找到的
            // 继续遍历，如果遇到了需要首先被移除的，就替换然后停止遍历，清掉这个 Debuff
            // 如果遍历完了都没找到需要首先被移除的，就清掉第一个找到的
            // 反向遍历
            Buff buffToRemove = null;
            bool found = false;
            for (int i = Data.BuffList.Count - 1; i >= 0; i--)
            {
                if (Data.BuffList[i].Data.BuffType == buffType)
                {
                    if (!found)
                    {
                        buffToRemove = Data.BuffList[i];
                        found = true;
                    }
                    if (Data.BuffList[i].Data.NeedToBeRemovedFirst)
                    {
                        buffToRemove = Data.BuffList[i];
                        break;
                    }
                }
            }

            if (found)
            {
                RemoveBuff(buffToRemove);
            }
        }
        public virtual void RemoveTheBuff(int buffID)
        {
            if (HasBuff(buffID, out var buff))
            {
                RemoveBuff(buff);
            }
        }
        public virtual void ReceiveBuff(Buff[] buffs)
        {
            
            foreach (var buff in buffs)
            {
                // Done: 检查身上是否已有该 Buff，如果有，检查是否可叠加，如果可叠加则叠加，否则仅更新持续时间
                // 有些 Buff 叠层之后效果也会叠加，考虑去掉原来的 Buff 然后加一个新的？
                // 要保证添加 Buff 和移除 Buff 是一个互补的操作
                if (buff is null) continue;
                // 对速度和距离做特殊处理, 需要在 UI 体现，并且距离的影响是即时的，且在 Buff 移除时也不会返还
                // 或者说 距离本就不是 Buff，行动提前/延后只是对对象的一种瞬时操作。
                if (buff.PropertyDict.TryGetValue("Distance", out var value))
                {
                    Distance += (int)(value * 0.01f * Distance);
                    EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
                    buff.PropertyDict.Remove("Distance");
                }
                
                if (HasBuff(buff.Data.BuffID, out Buff buffed))
                {
                    // TODO 这里可能还有得优化，不过先这样做了，也能在 UI 体现被更新的 Buff 会移到最前面
                    Data.BuffList.Remove(buffed);
                    if (buff.Data.Stackable)
                    {
                        buffed.CurrentStack = Mathf.Min(buffed.CurrentStack + buff.Data.StackAtATime, buff.Data.MaxStack);
                    }
                    buffed.DurationLeft = buff.Data.Duration;
                    Data.BuffList.Add(buffed);
                }
                else
                {
                    Data.BuffList.Add(buff);
                }
                if (buff.PropertyDict.ContainsKey("Speed")) EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
            }
            
        }
        protected void RemoveBuff(Buff buff)
        {
            Data.BuffList.Remove(buff);
            if (buff.PropertyDict.ContainsKey("Speed")) EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
        }
        public bool HasBuff(int buffID, out Buff buff)
        {
            foreach (var t in Data.BuffList.Where(t => t.Data.BuffID == buffID))
            {
                buff = t;
                return true;
            }

            buff = null;
            return false;
        }
        public void TriggerBuff(int buffID) {}
        
        
        
        // ====================================== Message =====================================
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
                case Message.MainActionEnd:
                    OnMainActionEnd();
                    break;
                case Message.ActionEnd:
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
