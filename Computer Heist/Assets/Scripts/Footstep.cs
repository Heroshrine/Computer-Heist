using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Footstep : MonoBehaviour
{
    public float maxSize = 4;
    private void Update()
    {
        transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime * maxSize, transform.localScale.y + Time.deltaTime * maxSize, transform.localScale.z + Time.deltaTime * maxSize);
    }

    public void Destroy() => Destroy(gameObject);
}
