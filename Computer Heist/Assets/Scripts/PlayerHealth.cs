using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public GameObject killMenu;
    public GameObject[] healthObjects;

    [Header("Debug")]
    public bool debug;

    int health;

    private void Start()
    {
        killMenu.SetActive(false);
        health = healthObjects.Length;
    }

    public void Damage()
    {
        if (debug)
            return;

        health--;

        if (health > 0)
            healthObjects[health].SetActive(false);
        else
        {
            healthObjects[health].SetActive(false);
            Kill();
        }
    }

    private void Kill()
    {
        PlayerMovement.Instance.gameObject.SetActive(false);
        killMenu.SetActive(true);
    }

}
