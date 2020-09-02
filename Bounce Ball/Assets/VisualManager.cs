using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualManager : MonoBehaviour
{
    public static Player player { get => Player.instance; }
    public static VisualManager instance;

    public List<ParticleSystem> confetti;

    [Header("Задание цвета")]

    public MeshRenderer ballRenderer;
    public Camera backgroundRenderer;
    public TrailRenderer trailRenderer;
    public ParticleSystemRenderer particleRenderer;

    public Image levelFrame;
    public Image sliderFill;
    public Image sliderHandle;

    public Color chargedColor;
    public Color chargedEmission;

    public float speedMin = 9.5f;
    public float clearColorDuration = 0.3f;
    public float lastValue;
    public Coroutine clearcolorRoutine;

    public static Color ballColor;
    public static Color backgroundColor;

    public List<Color> ballColors;
    public List<Color> backgroundColors;


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        lastValue = player.fallSpeed;
    }

    void Update()
    {
        UpdateBallColor();
    }

    void UpdateBallColor()
    {
        if (player.fallSpeed > lastValue)
        {
            if (clearcolorRoutine != null)
            {
                StopCoroutine(clearcolorRoutine);
                clearcolorRoutine = null;
            }

            float delta = player.fallSpeedMax - speedMin;
            float temp = player.fallSpeed - speedMin;

            float factor = Mathf.Clamp(temp / delta, 0, 1);

            if (factor > 0)
            {
                #region Вычисление нового цвета

                Vector3 colorBasic = new Vector3(ballColor.r, ballColor.g, ballColor.b);
                Vector3 colorCharged = new Vector3(chargedColor.r, chargedColor.g, chargedColor.b);
                Vector3 colorVector = colorCharged - colorBasic;
                Vector3 color = colorBasic + colorVector * factor;
                Color newColor = new Color(color.x, color.y, color.z, 1);

                #endregion

                #region Вычисление нового цвета свечения

                Vector3 emissionVector = new Vector3(chargedEmission.r, chargedEmission.g, chargedEmission.b);
                Vector3 emission = emissionVector * factor;
                Color newEmission = new Color(emission.x, emission.y, emission.z, 1);

                #endregion


                instance.ballRenderer.material.color = newColor;
                instance.trailRenderer.material.color = newColor;
            }
        }
        else if (player.fallSpeed < lastValue)
        {
            ClearBallColor();
        }

        lastValue = player.fallSpeed;
    }

    void ClearBallColor()
    {
        clearcolorRoutine = StartCoroutine(ClearColorRoutine(clearColorDuration));
    }

    IEnumerator ClearColorRoutine(float overTime)
    {
        Color startColor = ballRenderer.material.color;
        Vector3 colorStart = new Vector3(startColor.r, startColor.g, startColor.b);
        Vector3 colorTarget = new Vector3(ballColor.r, ballColor.g, ballColor.b);
        Vector3 colorVector = colorTarget - colorStart;


        var timer = 0f;
        var speed = 1 / overTime;

        while (timer < overTime)
        {
            timer += Time.deltaTime;
            var factor = timer / overTime;
            Vector3 color = colorStart + colorVector * factor;

            Color newColor = new Color(color.x, color.y, color.z, 1);
            instance.ballRenderer.material.color = newColor;
            instance.trailRenderer.material.color = newColor;
            yield return null;
        }

        instance.ballRenderer.material.color = ballColor;
        instance.trailRenderer.material.color = ballColor;

        clearcolorRoutine = null;
    }

    public static void PlayConfetti()
    {
        foreach (var particleSystem in instance.confetti)
            particleSystem.Play();
    }
    public static void ClearConfetti()
    {
        foreach (var particleSystem in instance.confetti)
            particleSystem.Clear();
    }

    public static void SwitchColors()
    {
        int count = instance.ballColors.Count;
        int colorId = Level.level - (Level.level / count) * count;
        ballColor = instance.ballColors[colorId];
        backgroundColor = instance.backgroundColors[colorId];

        instance.ballRenderer.material.color = ballColor;
        instance.trailRenderer.material.color = ballColor;
        instance.particleRenderer.material.color = ballColor;

        instance.levelFrame.color = ballColor;
        instance.sliderHandle.color = ballColor;
        instance.sliderFill.color = backgroundColor;
        instance.backgroundRenderer.backgroundColor = backgroundColor;
    }
}
