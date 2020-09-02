using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get;  private set; }
    public static Level level => Level.instance;
    public static Player player => Player.instance;

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
    public Text levelNumber;

    [Tooltip("Табличка с номером следующего уровня")]
    public Text nextLevelNumber;

    [Tooltip("Табличка для вывода сообщения")]
    public MessageLabelUI messageLabel;

    void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    void Start()
    {
        level.onLevelRestart.AddListener(OnLevelRestart);
        player.onScoreChanged.AddListener(OnPlayerScoreChanged);
        player.onPlayerVictory.AddListener(OnPlayerVictory);
        player.onPlayerDefeat.AddListener(OnPlayerDefeat);
    }

    void OnDestroy()
    {
        level.onLevelRestart.RemoveListener(OnLevelRestart);
        player.onScoreChanged.RemoveListener(OnPlayerScoreChanged);
        player.onPlayerVictory.RemoveListener(OnPlayerVictory);
        player.onPlayerDefeat.RemoveListener(OnPlayerDefeat);
    }

    public void OnLevelRestart()
    {
        progressBar.maxValue = Level.floorCount - 1;
        progressBar.value = 0;
        levelNumber.text = Level.level.ToString();
        nextLevelNumber.text = Mathf.Clamp(Level.level + 1, 1, Level.levelMax).ToString();

        vicrotyUI.SetActive(false);
        defeatUI.SetActive(false);

        scoreText.text = $"SCORE: {Player.Score}";
    }

    public void OnPlayerVictory()
    {
        vicrotyUI.SetActive(true);
    }

    public void OnPlayerDefeat()
    {
        highScoreText.text = $"BEST: {Player.HighScore.ToString()}";
        defeatUI.SetActive(true);
    }

    public void OnPlayerFall()
    {
        progressBar.value += 1;
    }

    public void OnPlayerScoreChanged(int score)
    {
        scoreText.text = $"SCORE: {score}";
    }

    public static void ShowMessage(string message)
    {
        instance.messageLabel.Show(message);
    }
}
