using System.Collections;
using System.Collections.Generic;
using Script.Objects;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    private Renderer render;
    private bool Moving;
    public int CurrentPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<Renderer>();

        Moving = false;
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Initialize()
    {
        render.enabled = false;
        CurrentPosition = 6;
        transform.position = GameManager.Instance.PosDict[6].transform.position;
    }

    public void Show()
    {
        render.enabled = true;
    }
    public void Hide()
    {
        render.enabled = false;
    }
    public bool Visible => render.enabled;

    public void Move(int direction)
    {
        while (true)
        {
            // 移动时检测要移动的方向是否还有目标, 查看 GM 的列表
            if (GameManager.Instance.PosDict.TryGetValue(CurrentPosition + direction, out BaseObject obj))
            {
                if (!obj.isAlive)
                {
                    // 如果目标已经倒下, 则继续检测下一个目标
                    direction = direction + (int)Mathf.Sign(direction);
                    continue;
                }

                CurrentPosition += direction;
                transform.position = obj.transform.position;
            }

            break;
        }
    }

    public void SelectingFriendlyTarget()
    {
        
    }

    public void SelectingEnemyTarget()
    {
        
    }

    public void MoveTo(BaseObject obj)
    {
        transform.position = obj.transform.position;
    }


}
