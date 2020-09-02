using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Player player { get => Player.instance; }

    public float shiftMin = 0;
    public float shiftMax = 1.5f;

    public float viewMin = 60;
    public float viewMax = 75;

    public float minSpeed = 9.8f;
    public float resetDuration = 0.3f;

    public Camera camera;

    public float lastValue;
    public float factor;
    public Coroutine focusingRoutine;

    void Start()
    {
        lastValue = player.fallSpeed;
        factor = 0;
    }

    void Update()
    {
        if (player.fallSpeed > lastValue)
        {
            if (focusingRoutine != null)
            {
                StopCoroutine(focusingRoutine);
                focusingRoutine = null;
            }

            var temp = player.fallSpeed - minSpeed;
            var delta = player.fallSpeedMax - minSpeed;
            var factor = temp / delta;

            SetFocus(factor);
        }
        else if (player.fallSpeed < lastValue)
        {
            SetFocus(0, resetDuration);
        }

        lastValue = player.fallSpeed;
    }

    void SetFocus(float factor)
    {
        factor = Mathf.Clamp(factor, 0, 1);

        var localPos = camera.transform.localPosition;
        localPos.z = -(shiftMax - shiftMin) * factor;
        camera.transform.localPosition = localPos;

        camera.fieldOfView = viewMin + (viewMax - viewMin) * factor;
        this.factor = factor;
    }

    void SetFocus(float factor, float overTime)
    {
        focusingRoutine = StartCoroutine(FocusingRoutine(factor, overTime));
    }

    IEnumerator FocusingRoutine(float targetFactor, float overTime)
    {
        var timer = 0f;
        var speed = (targetFactor - factor) / overTime;

        while (timer < overTime)
        {
            timer += Time.deltaTime;
            SetFocus(factor + speed * Time.deltaTime);
            yield return null;
        }
        SetFocus(targetFactor);
        focusingRoutine = null;
    }
}
