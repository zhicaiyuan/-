using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.P) && player.blackHole.CanSkill())
            statemachine.changestate(player.blackholestate);
        if (player.iswalldetected() )
        {
            statemachine.changestate(player.wallslide);
        }
       
        if (player.isgrounddetected())
        {
           
            statemachine.changestate(player.idlestate);
        }


        if(xinput != 0 )
        {
            player.setvelocity(player.movespeed *  xinput, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.K) && player.jumpchance > 0)
        {
            Debug.Log(player.jumpchance);
            rb.velocity = new Vector2(rb.velocity.x, player.jumpforce);
            player.jumpchance--;
        }
    }
}
