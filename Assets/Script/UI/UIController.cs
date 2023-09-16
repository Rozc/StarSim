using System.Collections;
using System.Collections.Generic;
using Script.Enums;
using Script.Event;
using Script.Tools;
using Tools;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private UIDocument UIDoc;
    private VisualElement Root;
    private Label labelTurnQ;
    private Label labelActQ;
    private Label labelActor;
    private Button buttonBattleStart;
    private Button buttonBasicAtk;
    private Button buttonSkillAtk;
    private Button buttonSpace;
    private Button[] buttonsUltimate;
    private Button buttonLeft;
    private Button buttonRight;

    private GameManager GM;
    private EventCenter EC;
    // Start is called before the first frame update
    void Start()
    {
        GM = GameManager.Instance;
        EC = EventCenter.Instance;
        UIDoc = GetComponent<UIDocument>();
        Root = UIDoc.rootVisualElement;

        labelTurnQ = Root.Q<Label>("Lab_TurnQ");
        labelActQ = Root.Q<Label>("Lab_ActQ");
        labelActor = Root.Q<Label>("Lab_Actor");
        
        buttonBasicAtk = Root.Q<Button>("Btn_Basic");
        buttonSkillAtk = Root.Q<Button>("Btn_Skill");
        buttonBattleStart = Root.Q<Button>("Btn_BattleStart");
        buttonSpace = Root.Q<Button>("Btn_Space");

        buttonLeft = Root.Q<Button>("Btn_Left");
        buttonRight = Root.Q<Button>("Btn_Right");

        buttonsUltimate = new Button[4];
        buttonsUltimate[0] = Root.Q<Button>("Btn_Ultimate1");
        buttonsUltimate[1] = Root.Q<Button>("Btn_Ultimate2");
        buttonsUltimate[2] = Root.Q<Button>("Btn_Ultimate3");
        buttonsUltimate[3] = Root.Q<Button>("Btn_Ultimate4");

        // labelActQ.text = GM.GetActQ();
        buttonBasicAtk.RegisterCallback<ClickEvent>(evt =>
        {
            GM.GetInputFromUI(KeyCode.Q);
        });
        buttonSkillAtk.RegisterCallback<ClickEvent>(evt =>
        {
            GM.GetInputFromUI(KeyCode.E);
        });
        buttonSpace.RegisterCallback<ClickEvent>(evt =>
        {
            GM.GetInputFromUI(KeyCode.Space);
        });
        buttonLeft.RegisterCallback<ClickEvent>(evt =>
        {
            GM.GetInputFromUI(KeyCode.A);
        });
        buttonRight.RegisterCallback<ClickEvent>(evt =>
        {
            GM.GetInputFromUI(KeyCode.D);
        });


        buttonBattleStart.RegisterCallback<ClickEvent>(evt =>
        {
            // GM.GetInputFromUI(InputMessage.BattleStart);
            GM.GetInputFromUI(InputMessage.BattleStart);
        });
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            buttonsUltimate[i].RegisterCallback<ClickEvent>(evt =>
            {
                GM.GetInputFromUI((KeyCode)(KeyCode.Alpha1 + j));
            });
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GM.GetInputFromUI(KeyCode.Q);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GM.GetInputFromUI(KeyCode.E);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GM.GetInputFromUI(KeyCode.Space);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            GM.GetInputFromUI(KeyCode.A);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            GM.GetInputFromUI(KeyCode.D);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GM.GetInputFromUI(KeyCode.Alpha1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GM.GetInputFromUI(KeyCode.Alpha2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GM.GetInputFromUI(KeyCode.Alpha3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GM.GetInputFromUI(KeyCode.Alpha4);
        }

    }

    public void SetInteractable(ButtonID BtnID, bool interactable)
    {
        Button button = GetButtonByID(BtnID);
        if (button is null)
        {
            Debug.Log("Button not found");
            return;
        }
        button.SetEnabled(interactable);
    }

    private Button GetButtonByID(ButtonID buttonID)
    {
        switch (buttonID)
        {
            case ButtonID.BasicAttack:
                return buttonBasicAtk;
            case ButtonID.SkillAttack:
                return buttonSkillAtk;
            case ButtonID.BattleStart:
                return buttonBattleStart;
            case ButtonID.CursorMoveLeft:
                return buttonLeft;
            case ButtonID.CursorMoveRight:
                return buttonRight;
            case ButtonID.Space:
                return buttonSpace;
            default:
                return null;
        }
    }

    public void UpdateActQLabel()
    {
        labelActQ.text = GM.GetActQ();
    }

    public void UpdateActorName()
    {
        labelActor.text = GM.GetActorName();
    }
    public void UpdateTurnQLabel()
    {
        labelTurnQ.text = GM.GetTurnQ();
    }
}
