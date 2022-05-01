using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Bullet : MonoBehaviour
{
    public float bulletSpeed;
    readonly float bulletLife = 30f;
    float life;

    Bounds colliderBounds;

    private void Start()
    {
        colliderBounds = GetComponent<BoxCollider2D>().bounds;
    }

    private void Update()
    {
        life += Time.deltaTime;
        if (life > bulletLife)
            Destroy(gameObject);

        transform.position += bulletSpeed * Time.deltaTime * transform.right;
    }

    private void FixedUpdate()
    {
        Collider2D[] cols = Physics2D.OverlapAreaAll(new Vector2(colliderBounds.extents.x + transform.position.x, colliderBounds.extents.y + transform.position.y), new Vector2(transform.position.x - colliderBounds.extents.x, transform.position.y - colliderBounds.extents.y));
        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i].isTrigger)
                continue;

            if (cols[i].gameObject.layer == 6)
            {
                cols[i].GetComponent<SneakyPlayerMovement>().Damage();
                Destroy(gameObject);
            }

            if (cols[i].gameObject.layer == 3)
                Destroy(gameObject);

        }


    }
}
