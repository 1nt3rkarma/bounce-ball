using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusUI : MonoBehaviour
{
    public static Player player { get => Player.instance; }
    public static BonusUI instance;

    public Animator animator;
    public string animTagPopUp = "popUp";
    public string animTagUpdate = "update";
    public string animTagFade = "fade";

    public Text label;

    int lastValue;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        lastValue = player.bonus;
    }

    void Update()
    {
        if (player.bonus > lastValue)
        {
            if (lastValue == player.bonusMin)
                animator.SetTrigger(animTagPopUp);
            else
                animator.SetTrigger(animTagUpdate);

            label.text = $"x{player.bonus}";
        }
        else if (player.bonus < lastValue)
        {
            animator.SetTrigger(animTagFade);
        }

        lastValue = player.bonus;
    }
}
