using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class ScoreEvent : UnityEvent<int> { }

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }

    public float sensitivityKeyboard = 100;
    public float sensitivityTouch = 45;
    [HideInInspector]
    public bool isActive = true;

    public float fallSpeedMin = 8;
    public float fallSpeedMax = 12;
    public float fallAxeleration = 9.8f;
    public float fallSpeed;

    public bool isFalling = false;
    public float fallTargetHeight;

    [Header("Бонус от падения")]
    public int bonusMin = 1;
    public int bonusMax = 10;
    public int bonus;

    public Animator animator;
    public TrailRenderer trailRenderer;
    
    //Счет игрока
    private static int score = 0;
    public static int Score
    {
        get => score;

        set
        {
            instance?.onScoreChanged.Invoke(value);
            score = value;
        }
    }
    public static int HighScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);

        set => PlayerPrefs.SetInt("HighScore", value);
    }

    static bool SecterFound
    {
        get => PlayerPrefs.GetInt("secterFound", 0) == 1 ? true : false;
         
        set => PlayerPrefs.SetInt("secterFound", value ? 1 : 0);
    }

    public ScoreEvent onScoreChanged;
    public UnityEvent onPlayerVictory;
    public UnityEvent onPlayerDefeat;
    public UnityEvent onPlayerFall;

    void Awake()
    {
        instance = this;

        bonus = bonusMin;
        fallSpeed = fallSpeedMin;

        //onScoreChanged = new ScoreEvent();
    }

    void Update()
    {
        if (isActive)
        {
            ControllsTouch();
            ControllsKeyboard();
        }

        if (isFalling)
        {
            if (transform.position.y > fallTargetHeight)
            {
                if (fallSpeed < fallSpeedMax)
                    fallSpeed += fallAxeleration * Time.deltaTime;
                fallSpeed = Mathf.Clamp(fallSpeed, fallSpeedMin, fallSpeedMax);

                float move = fallSpeed * Time.deltaTime;

                if (move > transform.position.y - fallTargetHeight)
                    move = transform.position.y - fallTargetHeight;

                transform.position += Vector3.down * move;
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
            SwitchTimeScale();
    }

    void ControllsTouch()
    {
        Touch touch;
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                float direction = touch.deltaPosition.x;

                Vector3 rotation = new Vector3(0, direction 
                    * sensitivityTouch * Time.deltaTime, 0);
                transform.Rotate(rotation);
            }
        }
    }

    void ControllsKeyboard()
    {
        int direction = 0;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction += 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction += -1;

        Vector3 rotation = new Vector3(0, direction 
            * sensitivityKeyboard * Time.deltaTime, 0);
        transform.Rotate(rotation);
    }

    public void Init()
    {
        isActive = true;

        // Задание цветовой схемы
        VisualManager.SwitchColors();

        // Установка игрока в начальное положение и вращение
        transform.position = new Vector3(0, Level.floorCount * 2 - 1, 0);
        transform.eulerAngles = new Vector3(0, 0, 0);

        animator.speed = 1;
        animator.SetTrigger("restart");
    }

    public void Defeat()
    {
        score = 0;

        onPlayerDefeat.Invoke();

        AudioManager.PlayDefeatJingle();

        Stop();
    }

    public void Victory()
    {
        VisualManager.PlayConfetti();

        AudioManager.PlayVictoryJingle();

        onPlayerVictory.Invoke();
        Stop();
    }

    public void Hit (Segment segment)
    {
        switch (segment.type)
        {
            case SegmentTypes.empty:
                Fall(segment.transform.parent);
                break;
            case SegmentTypes.trap:
                if (fallSpeed >= fallSpeedMax)
                {
                    Debug.Log("Ломай его");
                    FakeFloor(segment.floor);
                    DestroyFloor(segment.floor);
                    Jump();
                }
                else
                    Defeat();
                break;
            case SegmentTypes.finish:
                Victory();
                break;
            default:
                if (fallSpeed >= fallSpeedMax)
                {
                    Debug.Log("Ломай его");
                    FakeFloor(segment.floor);
                    DestroyFloor(segment.floor);
                }
                Jump();
                break;
        }
    }

    void Jump()
    {
        AudioManager.PlayJumpJingle();
        animator.speed = 1;
        if (isFalling)
            StopFall();
    }

    void Stop()
    {
        if (isFalling)
            StopFall();
        isActive = false;
        animator.speed = 0;
        DisableTrail();
    }

    /// <summary>
    /// Запускает плавное перемещение камеры на этаж ниже
    /// и разрушение текущей платформы.
    /// </summary>
    /// <param name="fromFloor"> Трансформ этажа 
    /// (к которому привязаны отдельные сегменты),
    /// который нужно разрушить
    /// </param>
    public void Fall(Transform fromFloor)
    {
        animator.speed = 0;

        if (isFalling && bonus < bonusMax)
            bonus++;
        Score += bonus;

        if (Score > HighScore)
            HighScore = Score;

        fallTargetHeight = fromFloor.position.y - 1;
        isFalling = true;
        DestroyFloor(fromFloor);

        onPlayerFall.Invoke();
    }

    void StopFall()
    {
        bonus = bonusMin;
        fallSpeed = fallSpeedMin;
        isFalling = false;
        transform.position = Vector3.up * fallTargetHeight;
    }

    void FakeFloor(Transform floor)
    {
        Level.CreateEmptyFloor(floor.position);
    }

    void DestroyFloor(Transform floor)
    {
        foreach (Transform segmentTransform in floor)
        {
            var segment = segmentTransform.GetComponent<Segment>();
            if (segment)
                segment.Destroy();
        }
        Destroy(floor.gameObject);
    }

    void SwitchTimeScale()
    {
        if (Time.timeScale == 1)
            Time.timeScale = 0.5f;
        else
            Time.timeScale = 1;
    }

    public void OnSecretClick()
    {
        if (!SecterFound)
        {
            UIManager.ShowMessage("SECRET FOUND!");
            SecterFound = true;
        }

        SwitchTimeScale();
    }

    public void EnableTrail()
    {
        trailRenderer.emitting = true;
    }
    public void DisableTrail()
    {
       trailRenderer.emitting = false;
    }
}



