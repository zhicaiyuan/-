using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LostCurrency : MonoBehaviour
{
    public int currency;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            playermanger.instance.currency += currency;
            Destroy(this.gameObject);
        }
    }
}
