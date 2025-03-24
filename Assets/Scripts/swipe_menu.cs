using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class swipe_menu : MonoBehaviour
{
    public GameObject scrollbar;
    private float scroll_pos = 0;
    private float[] pos;

    void Start()
    {
        if (scrollbar == null)
        {
            Debug.LogError("Scrollbar is not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (scrollbar != null)
        {
            pos = new float[transform.childCount];
            float distance = 1f / (pos.Length - 1f);
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * i;
            }

            // Check if the new Input System is available and that either Mouse or Touch is present
            if ((Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed))
            {
                scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
                Debug.Log("Mouse or touch detected. Scroll position: " + scroll_pos);
            }
            else
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                    {
                        scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                    }
                }
            }

            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                    for (int a = 0; a < pos.Length; a++)
                    {
                        if (a != i)
                        {
                            transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Scrollbar reference is null.");
        }
    }
}
