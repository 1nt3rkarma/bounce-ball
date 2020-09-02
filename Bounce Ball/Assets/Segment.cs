using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour
{
    public SegmentTypes type = SegmentTypes.common;

    public float shrinkDuration = 1;
    public float shrinkFactor = 0.1f;

    public Collider collider;
    public Rigidbody body;
    public MeshRenderer renderer { get => GetComponent<MeshRenderer>(); }
    public Transform floor { get => transform.parent; }

    public bool isActive = true;

    public void Destroy()
    {
        transform.SetParent(null);
        collider.isTrigger = false;

        if (type != SegmentTypes.empty)
        {
            isActive = false;

            Vector3 randomTorque = new Vector3(Random.Range(-1f, 1f),
                                   Random.Range(-1f, 1f),
                                   Random.Range(-1f, 1f));

            body.isKinematic = false;
            body.AddTorque(randomTorque, ForceMode.Impulse);
            body.AddExplosionForce(200, transform.position, 1);

            StartCoroutine(ShrinkRoutine());
        }
        else
            Destroy(gameObject);
    }

    IEnumerator ShrinkRoutine()
    {
        float timer = 0;

        float scale = transform.localScale.x;
        float scaleFinal = scale * shrinkFactor;
        float speed = (scale - scaleFinal) / shrinkDuration;

        while (timer < shrinkDuration)
        {
            timer += Time.deltaTime;
            scale -= Time.deltaTime * speed;
            var newScale = new Vector3(scale, scale, scale);
            transform.localScale = newScale;
            yield return null;
        }
        Destroy(gameObject);
    }
}

public enum SegmentTypes { common, empty, trap, finish }