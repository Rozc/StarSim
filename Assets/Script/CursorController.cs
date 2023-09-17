using System.Collections;
using System.Collections.Generic;
using Script.Objects;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    private Renderer render;
    public int CurrentPosition { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<Renderer>();
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void Show()
    {
        render ??= GetComponent<Renderer>();
        render.enabled = true;
    }
    public void Hide()
    {
        render ??= GetComponent<Renderer>();
        render.enabled = false;
    }
    public bool Visible => render.enabled;




}
