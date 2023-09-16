
using System;
using System.Collections.Generic;
using Script.ActingLogic;
using Script.Enums;
using Script.Event;
using Script.Objects;
using Script.Tools;
using UnityEngine;
using Tools;
using Action = Script.ActionLogic.Action;
using Random = UnityEngine.Random;

public class GameManager : SingletonBase<GameManager>
{

    public Dictionary<int, BaseObject> ObjDict; // <ID, Object>
    public Dictionary<int, BaseObject> PosDict; // <Position, Object>
    public List<Friendly> FriendlyObjects;
    public List<Enemy> EnemyObjects;
    public GameObject FriendlyCenter;
    public GameObject EnemyCenter;
    
    private CursorController TargetCursor;
    private UIController UI;
    private EventCenter EC;
    
    private TurnQueue _turnQ;
    private ActionQueue _actionQ;

    private GMStatus _state = GMStatus.BeforeInit;
    private BaseObject _currentTurnOf;
    private BaseObject _currentActionOf;
    private Action _currentAction;
    
    private int _currentUAID;
    public BaseObject CurrentTarget => PosDict[TargetCursor.CurrentPosition];


    private void Initialize()
    {
        if (_state != GMStatus.BeforeInit)
        {
            Debug.LogError("Logic Error: Unexpected Message, Battle Already Started.");
            return;
        }
        _state = GMStatus.Idle;

        FriendlyCenter = GameObject.Find("FriendlyCenter");
        EnemyCenter = GameObject.Find("EnemyCenter");

        ObjDict = new Dictionary<int, BaseObject>();
        PosDict = new Dictionary<int, BaseObject>();
        FriendlyObjects = new List<Friendly>();
        EnemyObjects = new List<Enemy>();
        BaseObject[] objs = UnityEngine.Object.FindObjectsOfType<BaseObject>();

        _turnQ = new TurnQueue();
        _actionQ = new ActionQueue();

        UI = GameObject.Find("UIDocument").GetComponent<UIController>();
        EC = EventCenter.Instance;
        TargetCursor = UnityEngine.Object.FindObjectOfType<CursorController>();

        foreach (var obj in objs)
        {
            switch (obj)
            {
                case Friendly friendly:
                    FriendlyObjects.Add(friendly);
                    break;
                case Enemy enemy:
                    EnemyObjects.Add(enemy);
                    break;
            }

            ObjDict.Add(obj.Data.CharacterID, obj);
            PosDict.Add(obj.Position, obj);
        }
        UI.SetInteractable(ButtonID.BattleStart, false);
        TargetCursor.Initialize();
        EC.SubscribeEvent(EventID.ActionValueUpdate, EventUpdateTurnQ);
        // 这里可以处理一些秘技等战斗初始化动作
        
        BattleInit();
    }

    private void BattleInit() // ��ʼ���ж����У������ƽ�����һ�������ж���ʱ���
    {
        foreach (var obj in ObjDict.Values)
        {
            obj.GetMessageFromGM(Message.Spawn);
        }
        foreach (var obj in ObjDict.Values)
        {
            _turnQ.Push(obj); // �����ж�������ж�����
        }
        _turnQ.PushForward();
        UI.UpdateTurnQLabel();
        NextTurn();
    }
    
    private void NextTurn()
    {
        Debug.Log("Into Turn of " + _turnQ.Top().BaseData.Name);
        _currentTurnOf = _turnQ.Top();
        _currentTurnOf.GetMessageFromGM(Message.TurnBegin);
        
        // Turn Begin 阶段用于结算 buff 等
        // 另一方面可能需要针对 Boss 等在同一个回合里具有多个主行动的情况做特殊处理，比如
        // TODO 在角色数据里加一个 MainActionCount，然后每次主行动执行完 GM 递减 1，直到 Count==0 时才执行主行动结束逻辑
        
        Action action = new Action(_currentTurnOf);
        _actionQ.Push(action);
        NextAction();
    }
    private void NextAction()
    {
        Debug.Log("    Into Action of " + _actionQ.Top().Actor.BaseData.Name);
        Action action = _actionQ.Top();
        _currentActionOf = ObjDict[action.Actor.Data.CharacterID];
        AdjustButtonAndCursor();
        UI.UpdateActorName();
        UI.UpdateActQLabel();
        _state = GMStatus.WaitingAct;
        // return action;
        _currentActionOf.GetAction(action);
    }

    private void InteractDone()
    {
        UI.UpdateActorName();
        UI.UpdateActQLabel();
        UI.UpdateTurnQLabel();
    }
    
    private void ActionDone()
    {
        if (_state != GMStatus.WaitingActDone)
        {
            Debug.LogError("Logic Error: Unexpected Message, GM is not Waiting for ActionDone");
            return;
        }
        // 检测游戏是否结束
        UI.UpdateActorName();
        UI.UpdateActQLabel();
        // 如果是主行动结束，那就要更新回合队列
        if (_currentAction.ActionType == ActionType.Base)
        {
            _currentActionOf.GetMessageFromGM(Message.MainActionDone);
            _turnQ.MoveHeadToTail(10000 * 100);
            UI.UpdateTurnQLabel();
        }
        ClearCurrentObject();
        if (_actionQ.Count > 0)
        {
            _state = GMStatus.Idle;
            NextAction();
            return;
        }
        
        _currentTurnOf.GetMessageFromGM(Message.TurnEnd);
        TurnEnd();

    }

    private void TurnEnd()
    {
        Console.WriteLine("In TurnEnd()");
        _state = GMStatus.Idle; // �� GM ״̬��Ϊ����
        NextTurn();
    }

    // ===================================== Public 消息方法 =====================================

    public void GetInputFromUI(KeyCode input)
    {
        // �� UI ��ȡ����
        switch (input)
        {
            case KeyCode.Alpha1:
            case KeyCode.Alpha2:
            case KeyCode.Alpha3:
            case KeyCode.Alpha4:
                Debug.Log("    Ultimate Button Clicked");
                TryFriendlyUltimate((int)input - (int)KeyCode.Alpha0);
                return;
            case KeyCode.Q:
            case KeyCode.E:
            case KeyCode.X:
            case KeyCode.Space:
                if (_currentActionOf is null) return;
                (_currentActionOf as Friendly).GetInputFromManager(input);
                break;
            case KeyCode.A:
            case KeyCode.D:
                TryMoveCursor(input);
                break;
            default:
                Debug.LogError("Wrong Input");
                return;
        }
        
    }
    public void GetInputFromUI(InputMessage msg)
    {
          // �� UI ��ȡ��Ϣ
        switch (msg)
        {
            case InputMessage.BattleStart:
                Initialize();
                break;

        }
    }
    public BaseObject GetRandomObject(bool Enemy)
    {
        if (Enemy)
        {
            return EnemyObjects[Random.Range(0, EnemyObjects.Count)];
        } else
        {
            return FriendlyObjects[Random.Range(0, FriendlyObjects.Count)];
        }
    }
    public BaseObject GetObjectByPosition(int pos)
    {
        return PosDict[pos];
    }
    public void GetMessageFromActor(Message msg, int SenderID = -1)
    {
        if (_currentActionOf is null)
        {
            Debug.LogError("Invalid Game Object"); return;
        }
        switch (msg)
        {
            case Message.ActionPrepared:
                _state = GMStatus.WaitingActDone;
                _currentAction = _actionQ.Pop();
                break;
            case Message.InteractDone:
                InteractDone();
                break;
            case Message.ActionDone:
                ActionDone();
                break;
        }
    }
    public void AdjustCursor(TargetForm form)
    {
        // AoE: 在所有目标身上生成光标
        // Blast: 在当前目标身上生成光标, 并在其两侧目标上生成一个较小的光标
        // Single: 在当前目标身上生成光标
        // Bounce: 当前目标大光标, 其他目标小光标
        // TODO 情况不紧急, 以后再做
    }

    public int RequireExtraAction(Action action)
    {
        if (action is null)
        {
            Debug.LogError("Invalid Extra Action");
            return -1;
        }

        action.UAID = _currentUAID;
        
        _currentUAID++;
        if (_actionQ.Push(action) && _state == GMStatus.WaitingAct)
        {
            Debug.Log("    额外行动插入，现行动打断，执行额外行动");
            UI.UpdateActQLabel();
            _currentActionOf.GetMessageFromGM(Message.Interrupt);
            NextAction();
        }
        UI.UpdateActQLabel();
        return _currentUAID - 1;
    }
    public string GetActQ()
    {
        string str = _actionQ.ToString();
        return str;
    }

    public string GetTurnQ()
    {
        Dictionary<int, int> dict = _turnQ.DisplayDict();
        string str = "";
        foreach (var item in dict)
        {
            str += ObjDict[item.Key].Data.Name + " <=> " + item.Value + "\r\n";
        }
        return str;
    }

    public string GetActorName()
    {
        return _currentActionOf.BaseData.Name;
    }

    // ===================================== Private =====================================

    private void TryFriendlyUltimate(int id)
    {
        (PosDict[id] as Friendly)?.AskUltimate();
    }
    private void EventUpdateTurnQ(BaseObject sender, BaseObject _)
    {
        _turnQ.Update(sender.Data.CharacterID);
    }
    private void TryMoveCursor(KeyCode k)
    {
        if (!TargetCursor.Visible) return;
        switch (k)
        {
            case KeyCode.A:
                TargetCursor.Move(-1);
                break;
            case KeyCode.D:
                TargetCursor.Move(1);
                break;
        }
    }
    private void ClearCurrentObject()
    {
        // �����ǰ�ж�����
        _currentActionOf = null;
        _currentAction = null;
    }
    private void SetFriendlyButtons(bool b)
    {
        // �����ѷ��ж���ť�Ƿ����
        UI.SetInteractable(ButtonID.BasicAttack, b);
        UI.SetInteractable(ButtonID.SkillAttack, b);
        UI.SetInteractable(ButtonID.CursorMoveLeft, b);
        UI.SetInteractable(ButtonID.CursorMoveRight, b);
        UI.SetInteractable(ButtonID.Space, b);
    }
    private void AdjustButtonAndCursor()
    {
        if (_currentActionOf is Friendly)
        {
            TargetCursor.Show();
            SetFriendlyButtons(true);
        }
        else
        {
            SetFriendlyButtons(false);
            TargetCursor.Hide();
        }
    }

}

  
  