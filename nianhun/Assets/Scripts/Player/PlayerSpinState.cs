using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpinState : PlayerState
{
    private float spinStatetimer;
    private float defaultMoveSpeed;
    public PlayerSpinState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }






    public override void Exit()
    {
        player.movespeed = defaultMoveSpeed;
        base.Exit();
    }

    public override void Enter()
    {
        spinStatetimer = player.spin.spinDuration;
        defaultMoveSpeed = player.movespeed;
        player.movespeed = player.movespeed * .4f;
        skillmanager.instance.spin.Canuseskill();
        base.Enter();
    }

    public override void animationfinishtrigger()
    {
        base.animationfinishtrigger();
    }

    public override void Update()
    {
        spinStatetimer -= Time.deltaTime;

        if (spinStatetimer < 0)
            player.statemachine.changestate(player.idlestate);
        player.setvelocity(xinput * player.movespeed, rb.velocity.y);
        
        base.Update();
    }
}
