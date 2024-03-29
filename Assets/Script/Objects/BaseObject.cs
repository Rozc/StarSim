using System;
using System.Collections.Generic;
using System.Linq;
using Script.BuffLogic;
using Script.Data;
using Script.Enums;
using Script.Event;
using Script.InteractLogic;
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
        [field: SerializeField] public RealtimeData Data { get; protected set; }
        [field: SerializeField] public List<Buff> _receivedBuffList;
        [field: SerializeField] public bool isAlive = true;
        

        // Animation Moving Settings
        protected Vector3 primitivePosition; 
        protected Vector3 primitiveRotation; 
        protected BaseObject _target; 
        protected Vector3 movingTargetPosition;

        [field: SerializeField] public int Position;
        [field: SerializeField] public int UniqueID;
        [field: SerializeField] protected Action _currentAction;
        
        [field: SerializeField] protected MovingStatus MovingState = MovingStatus.Idle;
        
        public int Distance;
        
        public int ActionValue => Distance / (int)(Data.Get("Speed") * 100);
        protected GameManager GM;
        protected UIController UI;
        protected EventCenter EC;
        protected InteractManager IM;

        private CursorController _cursorMain;
        private CursorController _cursorSub;


        protected void MoveTo(Vector3 TargetPosition, float StopDistance)
        {
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
                    _afterInteractDone?.Invoke();
                    _afterInteractDone = null;
                    
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
        
        protected delegate void ExtraLogic();
        protected ExtraLogic _afterInteractDone = null;
        protected virtual void DoAnimation(SkillType skillType, TargetForm targetForm, ExtraLogic afterInteractDone = null)
        {
            _afterInteractDone = afterInteractDone;
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
        public virtual void ReceiveDamage(float value, BaseObject actor, bool trigger = true, int weakness = 0)
        {
            Data.CurrentHealth -= value;
            if (trigger) EC.TriggerEvent(EventID.ObjectOnHit, this, actor);
            
        }
        public virtual void ReceiveHealing(float value, BaseObject actor, bool trigger = true)
        {
            Data.CurrentHealth += value;
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
        public virtual void RemoveTheBuff(Buff buff)
        {
            if (HasBuff(buff, out var buffToRemove))
            {
                RemoveBuff(buffToRemove);
            }
        }

        public virtual void RemoveTheBuff(BuffData buffData)
        {
            if (HasBuff(buffData, out var buffToRemove))
            {
                RemoveBuff(buffToRemove);
            }
        }

        public virtual void ReceiveBuff(IEnumerable<Buff> buffs)
        {
            foreach (var buff in buffs)
            {
                _receivedBuffList.Add(buff);
            }
        }

        public virtual void ApplyBuff()
        {
            foreach (var buff in _receivedBuffList)
            {

                if (buff is null) continue;
                // 对速度和距离做特殊处理, 需要在 UI 体现，并且距离的影响是即时的，且在 Buff 移除时也不会返还
                // 或者说 距离本就不是 Buff，行动提前/延后只是对对象的一种瞬时操作。
                if (buff.PropertyDict.TryGetValue("Distance", out var value))
                {
                    Distance += (int)(value * 0.01f * Distance);
                    EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
                    buff.PropertyDict.Remove("Distance");
                }
                
                if (HasBuff(buff, out Buff buffed))
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
                if (buff.PropertyDict.ContainsKey("Speed")) 
                    EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
            }
            _receivedBuffList.Clear();
            
        }
        protected void RemoveBuff(Buff buff)
        {
            Data.BuffList.Remove(buff);
            string f = buff.Data.OnRemove.Trim();
            if (f != "")
            {
                var mt = buff.Caster.GetType().GetMethod(f);
                if (mt != null) mt.Invoke(buff.Caster, new object[] { this });
                else Debug.LogError("No Such OnRemove Method: " + f + " in " + buff.Caster.Data.Name);
            }
            if (buff.PropertyDict.ContainsKey("Speed")) 
                EC.TriggerEvent(EventID.ActionValueUpdate, this, buff.Caster);
        }
        public bool HasBuff(Buff buffIn, out Buff buffOut)
        {
            foreach (var t in Data.BuffList.Where(t => t == buffIn))
            {
                buffOut = t;
                return true;
            }

            buffOut = null;
            return false;
        }
        public bool HasBuff(BuffData data, out Buff buffOut)
        {
            foreach (var t in Data.BuffList.Where(t => t.Data.BuffID == data.BuffID))
            {
                buffOut = t;
                return true;
            }

            buffOut = null;
            return false;
        }
        public bool HasBuff(int buffID, out Buff buffOut)
        {
            foreach (var t in Data.BuffList.Where(t => t.Data.BuffID == buffID))
            {
                buffOut = t;
                return true;
            }

            buffOut = null;
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
        
        public bool TryGetLeft(out BaseObject obj)
        {
            if (GM.PosDict.TryGetValue(Position - 1, out obj))
            {
                return obj.isAlive || obj.TryGetLeft(out obj);
            }

            return false;
        }
        public bool TryGetRight(out BaseObject obj)
        {
            if (GM.PosDict.TryGetValue(Position + 1, out obj))
            {
                return obj.isAlive || obj.TryGetRight(out obj);
            }

            return false;
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

        public static bool operator ==(BaseObject a, BaseObject b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.UniqueID == b.UniqueID;
        }
        public static bool operator !=(BaseObject a, BaseObject b)
        {
            return !(a == b);
        }
        
        

    }
}
