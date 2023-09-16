using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    private Renderer render;
    private bool Moving;

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

    public void Show()
    {
        render.enabled = true;
    }
    public void Hide()
    {
        render.enabled = false;
    }
    public bool Visible
    {
        get { return render.enabled; }
    }
    public void Move(int Direction)
    {
        transform.position = transform.position + new Vector3(Direction*2, 0, 0);
    }
}
