using UnityEngine;

namespace Script
{
    public class CursorController : MonoBehaviour
    {

        private Renderer _render;
        private bool _enabled = true;

        // Start is called before the first frame update
        void Start()
        {
            _render = GetComponent<Renderer>();
            Hide();
        }
    

        public void Show(bool main, bool friendly)
        {
            _render ??= GetComponent<Renderer>();
            transform.localScale = main 
                ? new Vector3(0.5f, 1f, 0.5f) 
                : new Vector3(0.25f, 1f, 0.25f);
            _render.material.color = friendly
                ? Color.blue
                : Color.red;
            _render.enabled = true;
        }
        public void Hide()
        {
            if (!_enabled) return;
            _render ??= GetComponent<Renderer>();
            _render.enabled = false;
        }
        public bool Visible => _render.enabled;




    }
}
