using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.dashchance--;
        AudioManager.instance.PlaySFX(11, null);
        player.fx.CreateSmokeFx(player.transform);
        statetimer = player.dashduration;
    }

    public override void Exit()
    {
        base.Exit();
        SkillManager.instance.dash.usedskill = false;
        player.setvelocity(0, rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();
        

        player.setvelocity(player.dashspeed * player.dashdir, 0);

        if (player.iswalldetected() && !player.isgrounddetected() && SkillManager.instance.wallJump.wallJumpUnlocked)
        {
            statemachine.changestate(player.wallslide);
        }
        if (statetimer < 0 && player.isgrounddetected())
        {
            statemachine.changestate(player.idlestate);
        }
        else if (!player.isgrounddetected() && statetimer < 0)
        {
            statemachine.changestate(player.airstate);
        }
    }
}