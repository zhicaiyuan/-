using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaserState : PlayerState
{
    private float laserStateTimer = 2f;
    public PlayerLaserState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void animationfinishtrigger()
    {
        base.animationfinishtrigger();
    }

    public override void Enter()
    {
        laserStateTimer = 2f;
        player.zerovelocity();
        player.isUnstoppable = true;
        player.isbusy = true;
        SkillManager.instance.laser.Canuseskill();
        base.Enter();
    }

    public override void Exit()
    {
        player.isUnstoppable = false;
        player.isbusy = false;
        
        base.Exit();
    }

    public override void Update()
    {
        laserStateTimer -= Time.deltaTime;
        if (laserStateTimer <= 0)
        {
            statemachine.changestate(player.idlestate);
        }
        base.Update();
    }
}
