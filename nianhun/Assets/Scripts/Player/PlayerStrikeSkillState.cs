using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStrikeSkillState : PlayerState
{
    private float strikeStateTimer;
    public PlayerStrikeSkillState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void animationfinishtrigger()
    {
        base.animationfinishtrigger();
    }

    public override void Enter()
    {
        strikeStateTimer = 3.1f;
        player.zerovelocity();
        player.isUnstoppable = true;
        player.isbusy = true;
        SkillManager.instance.strike.Canuseskill();
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
        strikeStateTimer -= Time.deltaTime;
        if (strikeStateTimer <= 0)
        {
            statemachine.changestate(player.idlestate);
        }
        base.Update();
    }
}
