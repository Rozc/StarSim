
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

// DailyLog: 

// 重新考虑 Action 的作用
// 是否要将伤害管理单独拿出来
// 考虑做一个 Action Poll 来提高性能，降低开销

// 行动逻辑上，将 Turn 与 Action 分离
// 现在 Action 好像没啥用，只有 ExtraAction 在用
/* 另外，整个行动逻辑应该分为以下阶段
 * 1. Turn Begin，回合开始阶段，这个阶段用于结算 Buff，对于 Enemy 对象，应在 TurnBegin 中直接执行行动
 * 2. Action Begin，不同于 TurnBegin，这个阶段存在于每个 Action 的开始，作用是进行一些判断
 *      比如 青雀：需要在 ActionBegin 阶段判断是否暗杠
 *      另外，若在 Action 等待输入时插入了一个立即执行的 ExtraAction，那么在这个 ExtraAction 结束后
 *      应该重新执行 ActionBegin 阶段
 * 3. Action-Doing
 * 4. Action End，该阶段用于结算一些 Buff
 * 5. TurnEnd，该阶段用于结算一些 Buff，同时开始下一回合
 *
 * 是否需要一个统一的阶段管理函数来避免回调地狱的问题？
 */ 
 
 /*
  * 青雀行为逻辑
  * 1. 在释放终结技后一定会紧随其后生成一个额外行动，但这个额外行动仅用于展示暗杠动画
  * 2. 在释放 E 后立即生成一个紧随其后的额外行动，该额外行动开始时检测是否满足暗杠条件，若满足则进入暗杠状态并播放动画，
  *     并在同一个行动中等待玩家的输入
  * 3. 若在进入主行动之前就检测到暗杠状态，则立即在主行动前生成一个额外行动，但这个额外行动仅用于展示暗杠动画
  *
  * 由于青雀的额外行动可以被大招插队，且此时该额外行动已经出队列并发送给了青雀，这与实际逻辑冲突
  * 我们需要的是，如果一个额外行动需要输入，那么它应该被 Top 出来然后发送给行动者
  * 在行动者确认输入并执行行动后，再将其 Dequeue
  * 并且在接收到其他额外行动时，如果发生其他额外行动比当前额外行动优先级高的情况，
  * 应该停止当前行动并转入优先级更高的额外行动的执行。
  */

/*
 * 关于重新考虑如何处理输入
 * 游戏对象需要在不同的状态下接受输入，因此我们将状态做成低16位的Bitmap 输入做成高16位的Bitmap
 * 然后用 switch - case 来处理输入
 * case 表达式 Input | State
 * 并且让每个角色自行维护状态列表
 * 什么？你说如果状态/输入不止16个怎么办？那有点离谱了
 * 可以换个大点的Bitmap或者用子状态
 */

/*
 * GameManager 还是需要一个状态来判断行动是否正在进行中（也就是完成了输入，进入动画状态，但还没 ActDone
 * 来判断新加入的额外行动的执行时机
 * 在 WaitingAct 阶段立即执行，
 * 在 WaitingActDone 阶段等待主行动结束后执行
 *
 * WaitingExtraAct 插入的话还得实现额外行动回收逻辑
 *
 * 一个新的想法：不区分主行动和额外行动，利用 Action 类里面的标记区分是否结算 buff，是否触发 ActionEnd 状态；
 * 把主行动也加入到 ExtraAction 的优先级机制中
 * 当然，TurnQueue 里跑的还是游戏对象，这个不影响
 * 还是之前的想法，GM 把行动发给游戏对象，游戏对象执行行动，行动结束后通知 GM
 * 到了某人的回合之后，GM 在 ActionQueue 里生成一个主行动，然后 Top 给游戏对象但是不出队
 * 如果新来的行动优先级高于正在等待的行动，那么 GM 给游戏对象发送一个 Interrupt 信号，
 * 如果是主行动的话再加一个标记，表示 ActionBegin 已经执行过了（这里存疑，ActionBegin 阶段到底有没有用来结算什么buff
 * 还是只是用于判断
 * 说不定就算是主行动， ActionBegin 也需要再次执行
 */

/*
 * Action 类里的 NeedInput 可能可以被 ActionType 代替
 * Base 和 Extra 应当被定义为需要输入的行动
 * 而 Followup 则是不需要输入的行动
 *
 * 考虑到 Action 需要有个 Target
 * 可以在 SetTarget 函数里由行动者补全 Target
 */

/*
 * 明日计划：
 * 现有 Bug: （第一次出现在某次Playmode插入终结技后）回合列表炸了，后来就一直都炸了，好像是 UI 问题 - fixed
 * TODO-Done: 实现行动中断，即处于等待中的行动被新的行动打断，当然新的行动优先级需要高于等待中的行动
 */
/*
 * 行动等待时 GM 的状态是 WaitingAct，此时对应的行动处于 actionQ 的顶部，当 GM 接收到一个额外行动时，将其加入 actionQ 并根据返回值判断
 * 新加入的行动是否被直接提前到了顶部，如果是的话，GM 应该向 currentActor 发送行动中断信号，currentActor 应当清除本次行动带来的所有影响
 * 然后 GM 转入执行新的行动
 *
 * 接收到回合之后应该根据回合内容，改变状态以等待接收输入
 */
/*
 * 基础行动框架支持应该告一段落了
 * 青雀的机制也基本写完了
 * TODO: 下一步是完成 Buff 机制，伤害机制，以及 Data 的规范管理
 * 还需要完成的系统包括
 * 1. 装备系统，包括光锥和遗器
 * 2. 完善 UI 表现
 * 3. 秘技，即在 BattleInit 时需要执行的动作
 *
 * Buff 机制的话，应该会把 Buff 写成一个接口，然后每个 Buff 都是一个类，这样可以方便地实现各种 Buff
 * 在想能不能用结合 ScriptableObject，把 Buff 做成某种数据，然后
 * 把 ActionData 里的 ImpairType 改成 Buff 列表（？）
 * 最大的问题是怎么实现 Buff 里一些需要执行的函数，
 * 可以在 Buff 里用 string[] 写函数名，然后在 GameObject 里反射调用
 * 然后把 Buff 拖进去就行了
 * 如果要这样做的话其实可以和伤害结合，伤害做成一个类，伤害里带有 ImpairType
 * 然后用来结算 Buff
 *
 * TODO: 现在先把现有的 4 个角色昨晚，然后做系统，然后可以把饮月和物主这两个具有特殊形式输入的也做了
 *
 * TODO: 考虑要不要改一下 DoAction 函数，增加参数，让角色告诉他应该做哪个动作
 *
 * 对于事件系统的思考
 * 事件的参数除了 SenderID 和 OtherID 还需要什么
 * SenderID 就是发送者的 ID
 * OtherID 则是如果对应事件是一个交互事件，比如 XXX 被 YYY 攻击了，那么 SenderID = XXX，OtherID = YYY
 *
 * 考虑到刃这种造成攻击力倍率+生命值倍率伤害的机制存在，Action Detail 里的 int 型 BaseValue 等可能需要是一个数组
 *
 * 然后 Buff 系统，每个 Buff 应该是一个 *类*，而不是一个数据
 * 因为要有 角色被挂上 Buff 之后做出的响应，也就是
 * OnAdd
 * OnRemove
 * 比如停运对目标释放 E 技能后挂上的 Buff：攻击力提高，且释放终结技后获得加速
 * 也就是角色需要注册一个事件，这个事件是自己释放了终结技，然后在这个事件的响应里面，对自己挂上一个 Buff
 * tmd 有点怪
 * 那岂不是要专门写一个响应函数来响应这个事件？而这个事件只是一个 Buff 带来的，并不专属于某个角色
 * 能不能把影响范围限定在 Buff 管理里面
 *
 * 对于可以叠加的 Buff，要先检查 BuffList 里有没有这个 Buff，然后再决定是加一个新的还是更新已有的
 *
 * 每个角色对象持有一个自己可能施加的bufflist，也就是预先加载资源。
 *
 * 0913 今天做了 Buff 系统的基本框架，但是上面提到的检查还没写，回调函数还没写
 * 然后 BaseObject 的数据结构把增伤，抗性，减伤的每个属性都写出来了，现在很长
 * 而且处理 Buff 的函数也用了 switch + 一大串 case，不知道有没有什么比较优美的实现
 *
 * TODO 明天计划完成 Buff 系统，完成 InteractManager 系统，也就是 Buff 和伤害管理
 * 这些做完后就可以实现一些角色了
 * 再然后就可以写一些 UI 和动画，这样应该可以作为一个小的 Demo 了
 * 至少这玩意全是自己写的，不是跟着做的。
 *
 * BUG 乱按的时候发现一个 UI 问题，在敌人行动时按下键盘按键会报错，明天修一下
 * TODO 这个等以后写 UI 再说
 *
 * 0914 今日 把 BuffProperties 改成 string[]，然后利用反射获取数据，这样就不用写一大串 switch 了
 * 修改了 Buff 数据的存放逻辑，现在存放在 ActionData 的 BuffDataList 里，也就是什么行动给目标上什么 Buff 是写在 ActionData 里的
 *
 * 关于进场自带的一些行为，比如说克拉拉给自己上减伤 buff，或者秘技，这些应该是在 BattleInit 里面写，也视为一次交互，即也需要一个 ActionDetail 来表达
 *
 * 现在修改一下 GetAction 和 DoAction 的实现
 * GetAction 主要做以下几件事：
 * 1. 将接收到的 Action 保存到 _currentAction 里
 * 2. 修改对象的 CommandState
 * 3. 如果是 Follow-up 行动，则调用 ExtraActCode 对应的函数
 * GetAction 应该在角色自己的脚本里接收角色特有的额外行动，就是 ExtraActCode 大于 0 的
 * BaseAction 和 终结技(ExtraActCode = 0) 就可以交给基类函数处理
 *
 * DoAction 函数更像是一个动画处理过程，当所有数据处理都已经完成后，进入动画
 * 当然这里要考虑的是，动画进行到一半时才应该爆数字和改血条，这是 UI 实现需要考虑的
 * 整理好了 GetAction 和 DoAction
 * 现在每个对象需要对自己的额外行动处理 GetAction，对于基本的行动就交给基类的函数
 * 然后 DoAction 现在是一个纯粹的动画处理过程，在动画结束时发送 ActionDone 消息
 * 另外重构过程中发现了一个很重要的事情，就是如果角色要在行动结束后立刻进入一个额外行动
 * 那么务必在 ActionPrepared 后再向 GM 请求行动，否则会出现当前行动被打断的问题
 * TODO 伤害管理，即 InteractManager
 *
 * 需要构建对象池以降低开销的类：Action，ActionDetail，Buff
 *  // TargetID == 0 表示这是一个无目标的行动 // 例如：罗刹的结界
 *  //          == -1  表示这是一个对友方全体的行动
 *  //          == -2  表示这是一个对敌方全体的行动
 *
 * TODO 将 buff 管理拆成一个单独的脚本，然后挂在每个角色身上，以降低单脚本代码长度    
 * 关于 buff 的更新和移除
 * 目前有些内有些 buff 需要经过一个完整的主要行动才会结算一次
 * Buff 消耗时间点应该是 3 个，
 *
 * Dot类负面 Buff：利用 CallBack 列表，注册事件，在回合开始阶段触发的事件，生成 ActionDetail 然后结算伤害
 * 然后持续时间直接 -1，如果持续时间为 0 那就直接 Remove
 *
 * 贝罗伯格进行曲 buff 持续时间 2 回合
 * 青雀不摸牌，也就是过了 TurnBegin 和 ActionBegin, 但是没 InAction 时，bronya 直接开大
 * 摸牌前：2 回合，摸牌后：2 回合，攻击并进入下一个角色的回合后：1 回合
 * 这个 Buff 的结算点可能在 MainActionEnd 阶段
 * 摸牌后再开大，开大后：2 回合，攻击并进入下一个角色的回合后：2 回合
 * 这个 Buff 的计算点可以确定时 MainActionEnd 阶段
 * 不对，在下一回合中，青雀摸牌后也不消耗回合数
 * 也就是说 可能是需要 MainActionEnd + TurnEnd 才消耗一次
 *
 * 判定点可能是 TurnBegin 和 MainActionEnd，而结算点仅在 TurnEnd
 * 在判定点时检索 BuffList，如果该 Buff 需要在该判定点时判定，则将其 “已经过判定点“标志位改为 true
 * 在结算点时检索 BuffList，如果该 buff 的“已经过判定点”标志位为 true，则将其持续时间 -1，若其持续时间归零，则将其移除
 * 同时移除所有 Stack = 0 的 buff，用于结算存在次数限制的 buff
 *
 * 按照判定点 + 结算点的逻辑完成了 Buff 系统，TODO 但是 Dot 和 控制类负面效果的逻辑还没做
 * 还有一个要考虑的是”持续到本回合结束“的 Buff，考虑把 Duration 设置成 1 然后在挂 Buff 的时候直接打上 CheckPointPass 的标记？
 * 还是做了一个 Must Be Removed At Turn End 的标记，需要的 Buff 标上就行
 *
 * 问题来了 在哪里处理 基础概率 和 固定概率呢
 * 比如青雀 4 魂：释放战技时 24% 的概率获得 Buff：不求人
 * 算可以在战斗管理器算，那是不是要给 Buff 数据加一个 ”命中概率“ 和 Fixed 概率的 bool 属性呢
 *
 * BUG 极其严重的 BUG： 青雀抽牌抽多了会直接 StackOverFlow，估计是同一个 Turn 里回合太多了，一直在反复调用函数
 * TODO 必须得搞个主流程函数来处理回合内容，可以用 while 循环，要及时退出函数。
 * 好吧，其实不至于，是逻辑实现出问题了，先放着吧，主流程不知道怎么做
 *
 * 角色数据可以分成 基础数据 行迹数据 光锥数据 遗器数据，这四个部分构成固定数据，然后固定数据与 Buff 数据结合，构成最终实时数据
 *
 * 要不改成 Dictionary，key 是 string，就是 属性类型的名字，value 是 float，就是属性值
 * 然后几个 Dictionary 分别表示各种数据
 * 然后用一个 Get 函数处理所有属性
 * 好 就这么写
 * Done
 *
 * Buff 系统除了 Dot 和 控制类负面效果之外的东西已经完成
 * 今晚完成 Dot 和控制吧
 * 然后还要处理 Aoe 的目标选择情况
 *
 * Doing Aoe 目标选择
 * Aoe 的情况可以把动画目标设为敌方中心点，然后 ad 的 target 字段就填 null 就好了
 *
 * Buff 系统还有一个重要的点没有做：CallBack
 * 对于一些特殊的 Buff，比如在行动后、特定行为后进行某些操作的情况，需要实现。
 *
 */
  
  