using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Player player { get => Player.instance; }

    [Tooltip("Заготовка кляксы")]
    public GameObject spotPrefab;

    [Tooltip("Система частиц")]
    public ParticleSystem particleSystem;

    // Таймер задержки между касаниями
    float jumpTimer = 0;
    // Время задержки между касаниями
    float jumpDelay;

    void Start()
    {
        jumpDelay = player.animator.GetCurrentAnimatorStateInfo(0).length;
    }

    void FixedUpdate()
    {
        jumpTimer += Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        var segment = other.GetComponent<Segment>();
        if (segment)
            if (player.isActive && (jumpTimer > jumpDelay) && segment.isActive)
            {
                player.Hit(segment);

                if (segment.type != SegmentTypes.empty)
                {
                    jumpTimer = 0;
                    CreateSpot(segment.transform);
                }
                else
                {
                    jumpTimer = jumpDelay * 0.9f;
                }
            }
    }

    void CreateSpot(Transform onSegment)
    {
        particleSystem.Play();

        Vector3 position = new Vector3(transform.position.x,
                                        onSegment.position.y + 0.2f,
                                        transform.position.z);

        GameObject spot = Instantiate(spotPrefab, position, transform.rotation);

        float randomRotation = Random.Range(0,360f);
        spot.transform.eulerAngles = new Vector3(90, randomRotation, 0);

        spot.GetComponent<SpriteRenderer>().color = VisualManager.ballColor;
        spot.transform.SetParent(onSegment);
    }
}
