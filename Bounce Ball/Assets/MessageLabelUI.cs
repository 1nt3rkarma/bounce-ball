using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageLabelUI : MonoBehaviour
{
    public Animator animator;

    public Text label;

    public string showTag = "show";
    public string hideTag = "hide";
    public string clearTag = "clear";

    Coroutine showRoutine;

    public void Show(string message)
    {
        Show(message, 3);
    }

    public void Show(string message, float period)
    {
        label.text = message;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine(period));

        animator.SetTrigger(showTag);
    }

    public void Hide()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        animator.SetTrigger(hideTag);
    }

    public void Clear()
    {
        if (showRoutine != null)
            StopCoroutine(showRoutine);

        animator.SetTrigger(clearTag);
    }  
    
    IEnumerator ShowRoutine(float period)
    {
        yield return new WaitForSecondsRealtime(period);
        Hide();
        showRoutine = null;
    }
}
