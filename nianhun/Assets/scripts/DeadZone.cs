using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<CharaterStat>() != null)
            collision.GetComponent<CharaterStat>().KillEntity();
        else
            Destroy(collision.gameObject);
    }
}
