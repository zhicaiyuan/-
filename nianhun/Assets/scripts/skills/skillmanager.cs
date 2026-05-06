using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skillmanager : MonoBehaviour
{
  public static skillmanager instance;

    public dashskill Dash { get; private set; }
    public CloneSkill clone {  get; private set; }
    public BlackHoleSkill blackhole { get; private set; }

    public void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else 
            instance = this;
    }

    private void Start()
    {
        Dash = GetComponent<dashskill>();
        clone = GetComponent<CloneSkill>();
        blackhole = GetComponent<BlackHoleSkill>();
    }
}
