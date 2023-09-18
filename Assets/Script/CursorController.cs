using System.Collections;
using System.Collections.Generic;
using Script.Objects;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    private Renderer render;
    private bool _enabled = true;

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
    

    public void Show(bool main, bool friendly)
    {
        render ??= GetComponent<Renderer>();
        transform.localScale = main 
            ? new Vector3(0.5f, 1f, 0.5f) 
            : new Vector3(0.25f, 1f, 0.25f);
        render.material.color = friendly
            ? Color.blue
            : Color.red;
        render.enabled = true;
    }
    public void Hide()
    {
        if (!_enabled) return;
        render ??= GetComponent<Renderer>();
        render.enabled = false;
    }
    public bool Visible => render.enabled;




}
