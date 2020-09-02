using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player instance;

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
    public static int score = 0;
    public static int highScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);

        set => PlayerPrefs.SetInt("HighScore", value);
    }

    static bool secterFound
    {
        get => PlayerPrefs.GetInt("secterFound", 0) == 1 ? true : false;
         
        set => PlayerPrefs.SetInt("secterFound", value ? 1 : 0);
    }

    [Tooltip("Элементы интерфейса игрока")]
    public PlayerUI playerUI;

    void Awake()
    {
        instance = this;
        bonus = bonusMin;
        fallSpeed = fallSpeedMin;
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

        #region Обновление интерфейса

        playerUI.progressBar.maxValue = Level.floorCount - 1;
        playerUI.progressBar.value = 0;
        playerUI.level.text = Level.level.ToString();
        playerUI.nextLevel.text = Mathf.Clamp(Level.level + 1,1,Level.levelMax).ToString();

        playerUI.vicrotyUI.SetActive(false);
        playerUI.defeatUI.SetActive(false);

        playerUI.scoreText.text = "" + score;

        #endregion

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

        playerUI.highScoreText.text = highScore.ToString();

        AudioManager.PlayDefeatJingle();

        playerUI.defeatUI.SetActive(true);
        Stop();
    }

    public void Victory()
    {
        VisualManager.PlayConfetti();

        AudioManager.PlayVictoryJingle();

        playerUI.vicrotyUI.SetActive(true);
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
        score += bonus;

        playerUI.scoreText.text = score.ToString();
        playerUI.progressBar.value += 1;

        if (score > highScore)
            highScore = score;

        fallTargetHeight = fromFloor.position.y - 1;
        isFalling = true;
        DestroyFloor(fromFloor);
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
        if (!secterFound)
        {
            playerUI.messageLabel.Show("SECRET FOUND!");
            secterFound = true;
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

[System.Serializable]
public class PlayerUI : object
{
    [Header("Элементы инерфейса")]

    [Tooltip("Блок интерфейса победного экрана")]
    public GameObject vicrotyUI;

    [Tooltip("Блок интерфейса экрана поражения")]
    public GameObject defeatUI;

    [Tooltip("Счет игрока")]
    public Text scoreText;

    [Tooltip("Лучший счет")]
    public Text highScoreText;

    [Tooltip("Шкала прогресса")]
    public Slider progressBar;

    [Tooltip("Табличка с номером текущего уровня")]
    public Text level;

    [Tooltip("Табличка с номером следующего уровня")]
    public Text nextLevel;

    [Tooltip("Табличка для вывода сообщения")]
    public MessageLabelUI messageLabel;
}



