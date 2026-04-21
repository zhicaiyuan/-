using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class playermanger : MonoBehaviour
{
    public static playermanger instance;

    public Player player;

    public int currency;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }

    public bool HaveEnoughMoney(int price)
    {
        if(price > currency)
        {
            Debug.Log("没有足够的钱");
            return false;
        }

        currency -= price;
        return true;
    }

    public int CurrentCurrencyAmount()
    {
        return currency;
    }
}
