using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermanger : MonoBehaviour
{
    public static playermanger instance;

    public Player player;

    private void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        else
            instance = this;
    }
}
