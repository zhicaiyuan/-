using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
  public static SkillManager instance;

    public Dashskill dash { get; private set; }
    public CloneSkill clone {  get; private set; }
    public BlackHoleSkill blackhole { get; private set; }
    public SpinSkill spin { get; private set; }
    public StrikeSkill strike { get; private set; }
    public LaserSkill laser { get; private set; }
    public ParrySkill parry { get; private set; }
    public DoubleJumpSkill doubleJump { get; private set; }
    public WallJumpSkill wallJump { get; private set; }
    public void Awake()
    {
        if (instance != null)
            Destroy(instance.gameObject);
        else 
            instance = this;
    }

    private void Start()
    {
        dash = GetComponent<Dashskill>();
        clone = GetComponent<CloneSkill>();
        blackhole = GetComponent<BlackHoleSkill>();
        spin = GetComponent<SpinSkill>();
        strike = GetComponent<StrikeSkill>();
        laser = GetComponent<LaserSkill>();
        parry = GetComponent<ParrySkill>();
        doubleJump = GetComponent<DoubleJumpSkill>();
        wallJump = GetComponent<WallJumpSkill>();
    }
}
