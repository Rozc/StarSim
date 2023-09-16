
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
    public List<Friendly> FriendlyObjects;
    public List<Enemy> EnemyObjects;
    public GameObject FriendlyCenter;
    public GameObject EnemyCenter;
    
    private CursorController TargetCursor;
    private UIController UI;
    private EventCenter EC;
    private TurnQueue _turnQ;
    private ActionQueue _actionQ;

    public GMStatus State = GMStatus.BeforeInit;
    private BaseObject _currentTurnOf;
    private BaseObject _currentActionOf;
    private Action _currentAction;
    
    private int _currentUAID;
    public int CurrentTargetID { get; private set; }
    public BaseObject CurrentTarget => ObjDict[CurrentTargetID];


    private void Initialize()
    {
        if (State != GMStatus.BeforeInit)
        {
            Debug.Log("Logic Error: Unexpected Message, Battle Already Started.");
            return;
        }
        State = GMStatus.Idle;

        FriendlyCenter = GameObject.Find("FriendlyCenter");
        EnemyCenter = GameObject.Find("EnemyCenter");

        ObjDict = new Dictionary<int, BaseObject>();
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

            ObjDict.Add(obj.Position, obj);
        }
        UI.SetInteractable(ButtonID.BattleStart, false);
        
        
        // 这里可以处理一些秘技等战斗初始化动作
        CurrentTargetID = 5;
        
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

    /*private void GameProcess()
    {
        NextTurn();
        while (_actionQ.Count > 0)
        {
            Action action = NextAction();
            _currentActionOf.GetAction(action);
            // TODO: 这里需要处理一下行动中断的问题
            // 这里需要等待用户输入，怎么处理呢
        }
    }*/
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
        Debug.Log("Into Action of " + _actionQ.Top().Actor.BaseData.Name);
        Action action = _actionQ.Top();
        _currentActionOf = ObjDict[action.Actor.Position];
        AdjustButtonAndCursor();
        UI.UpdateActorName();
        UI.UpdateActQLabel();
        State = GMStatus.WaitingAct;
        // return action;
        _currentActionOf.GetAction(action);
    }
    private void ActionDone()
    {
        if (State != GMStatus.WaitingActDone)
        {
            Debug.LogError("Logic Error: Unexpected Message, GM is not Waiting for ActionDone");
            return;
        }
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
            State = GMStatus.Idle;
            NextAction();
            return;
        }
        
        _currentTurnOf.GetMessageFromGM(Message.TurnEnd);
        TurnEnd();

    }

    private void TurnEnd()
    {
        Console.WriteLine("In TurnEnd()");
        State = GMStatus.Idle; // �� GM ״̬��Ϊ����
        NextTurn();
    }

    // ===================================== ����Ϊ������Ϣ���ݵ� Public ���� =====================================

    public void GetInputFromUI(KeyCode input)
    {
        // �� UI ��ȡ����
        switch (input)
        {
            case KeyCode.Alpha1:
            case KeyCode.Alpha2:
            case KeyCode.Alpha3:
            case KeyCode.Alpha4:
                Debug.Log("Ultimate Button Clicked");
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
                Debug.Log("Wrong Input");
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
    public BaseObject GetObjectByID(int id)
    {
        return ObjDict[id];
    }
    public void GetMessageFromActor(Message msg, int SenderID = -1)
    {
        if (_currentActionOf is null)
        {
            Debug.Log("Invalid Game Object"); return;
        }
        switch (msg)
        {
            case Message.ActionPrepared:
                State = GMStatus.WaitingActDone;
                _currentAction = _actionQ.Pop();
                break;
            case Message.ActionDone:
                ActionDone();
                break;
        }

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
        if (_actionQ.Push(action) && State == GMStatus.WaitingAct)
        {
            Debug.Log("额外行动插入，现行动打断，执行额外行动");
            _currentActionOf.GetMessageFromGM(Message.Interrupt);
            NextAction();
        }
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
            str += ObjDict[item.Key].BaseData.Name + " <=> " + item.Value + "\r\n";
        }
        return str;
    }

    public string GetActorName()
    {
        return _currentActionOf.BaseData.Name;
    }

    // ===================================== ����Ϊ Private ���� =====================================

    private void TryFriendlyUltimate(int id)
    {
        (ObjDict[id] as Friendly).AskUltimate();
    }
    private void TryMoveCursor(KeyCode k)
    {
        if (!TargetCursor.Visible) return;
        if (k == KeyCode.A)
        {
            if (CurrentTargetID <= 5)
            {
                // �Ѿ����������
                // 
                return;
            }
            else
            {
                CurrentTargetID--;
                TargetCursor.Move(-1);
            }
        }
        else if (k == KeyCode.D)
        {
            if (CurrentTargetID >= 5 + EnemyObjects.Count - 1)
            {
                // �Ѿ������ұ���
                return;
            }
            else
            {
                CurrentTargetID++;
                TargetCursor.Move(1);
            }

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

  
  