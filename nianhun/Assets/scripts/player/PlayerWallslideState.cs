using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallslideState : PlayerState
{
    public PlayerWallslideState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("1");
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.K))
        {
            statemachine.changestate(player.playerwalljump);
            return;
        }

        if(xinput !=0)
        {
            if(player.facedir != xinput)
            {
                statemachine.changestate(player.idlestate);
            }
        }

        rb.velocity = new Vector2(0, rb.velocity.y );

       

        if (player.isgrounddetected())
        {
            statemachine.changestate(player.idlestate);
        }
    }
}
