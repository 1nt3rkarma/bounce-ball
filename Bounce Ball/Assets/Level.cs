using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Level : MonoBehaviour
{
    #region Свойства

    #region Константы

    public const int segTotal = 12;
    public const int segAngle = 360 / segTotal;

    public const int segMax = segTotal - 1;
    public const int segMin = 4;

    public const int floorMax = 100;
    public const int levelMax = 1000;

    public const int floorCountDefault = 6;
    #endregion

    #region Статические

    public static Level instance { get; private set; }
    public static Player player => Player.instance;

    public static int level
    {
        get => PlayerPrefs.GetInt("Level", 1);

        set => PlayerPrefs.SetInt("Level", Mathf.Clamp(value, 1, levelMax));
    }
    public static int floorCount;

    #endregion

    [Tooltip("Префаб сегмента")]
    public Segment segmentPrefab;

    [Tooltip("Объект-ось уровня")]
    public GameObject axis;

    #endregion

    #region События

    public UnityEvent onLevelRestart;

    #endregion

    void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        Restart();
    }

    void OnDestroy()
    {
        
    }

    public static GameObject CreateEmptyFloor(Vector3 atPosition)
    {
        GameObject floor = new GameObject();
        floor.transform.position = atPosition;
        floor.transform.SetParent(instance.transform);

        for (int j = 0; j < segTotal; j++)
        {
            var segment = Instantiate(instance.segmentPrefab, floor.transform);
            segment.name = "Сегмент " + j;
            segment.transform.eulerAngles = new Vector3(0, segAngle * j, 0);

            segment.GetComponent<MeshRenderer>().enabled = false;
            segment.type = SegmentTypes.empty;
        }

        return floor;
    }

    public void Generate (int level)
    {
        // Очистка уровня
        for (int i = 2; i < transform.childCount; i++)
            Destroy(transform.GetChild(i).gameObject);

        floorCount = floorCountDefault + Mathf.Clamp(level, 1, floorMax);

        axis.transform.localScale = new Vector3(1, floorCount * 2 + 2, 1);

        for (int i = 1; i < floorCount; i++)
        {
            #region Создание этажа

            GameObject floor = new GameObject();
            floor.transform.position = new Vector3(0, i * 2, 0);
            floor.transform.SetParent(transform);
            floor.name = "Этаж " + i;

            #endregion

            #region Заполнение этажа сегментами

            // Случайно генерируем количество активных сегментов
            int count = Random.Range(segMin, segMax);

            // Случайно генерируем смещение сегментов
            // на этаже (по/против часовой стрелке)
            int shift = 0;
            if (i < floorCount - 1)
                shift = Random.Range(-segTotal / 2, segTotal / 2);

            // Список активных сегментов на этаже
            var activeSegments = new List<Segment>();
            for (int j = 0; j < segTotal; j++)
            {
                var segment = Instantiate(segmentPrefab, floor.transform);
                segment.name = "Сегмент " + j;
                segment.transform.eulerAngles = new Vector3(0, segAngle * (j + shift), 0);

                if (j > count)
                {
                    segment.GetComponent<MeshRenderer>().enabled = false;
                    segment.type = SegmentTypes.empty;
                }
                else
                    activeSegments.Add(segment);
            }

            #endregion

            #region Создание ловушек

            // Случайно генерируем число ловушек на этаже (от 1 до половины сегментов)
            // но на верхнем этаже ловушек быть не должно!
            int trapCount = 0;
            if (i < floorCount - 1)
                trapCount = Random.Range(1, count / 2);

            // Перебираем список активных сегментов
            // при каждом проходе списка:
            // - случайно генерируем номер сегмента-ловушки в этом списке;
            // - задаем параметры сегмента-ловушки;
            // - исключаем этот сегмент из списка;
            for (int k = 0; k < trapCount; k++)
            {
                Segment trap = null;
                int trapId = Random.Range(0, activeSegments.Count);
                for (int j = 0; j < activeSegments.Count; j++)
                {
                    if (j == trapId)
                    {
                        trap = activeSegments[j];
                        activeSegments[j].type = SegmentTypes.trap;
                        activeSegments[j].GetComponent<MeshRenderer>().material.color = Color.red;
                    }
                }
                activeSegments.Remove(trap);
            }

            #endregion
        }
    }

    public void Restart()
    {
        Generate(level);
        player.Init();
        VisualManager.ClearConfetti();

        onLevelRestart.Invoke();
    }

    public void NextLevel()
    {
        level++;
        Restart();
    }

    public void ClearSave()
    {
        Player.HighScore = 0;
        Level.level = 1;
    }
}
