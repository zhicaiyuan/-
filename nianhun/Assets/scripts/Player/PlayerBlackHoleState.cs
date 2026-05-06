using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlackHoleState : PlayerState
{
    private float flyTime = .4f;
    private bool skillUsed;
    private float defaultGravity;
    public PlayerBlackHoleState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void animationfinishtrigger()
    {
        base.animationfinishtrigger();
    }

    public override void Enter()
    {
        base.Enter();
        defaultGravity = player.rb.gravityScale;
        skillUsed = false;
        statetimer = flyTime;
        rb.gravityScale = 0;
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = defaultGravity;
        player.MakeTransprent(false);
    }

    public override void Update()
    {
        base.Update();
        if (statetimer > 0)
            rb.velocity = new Vector2(0, 10);

        if(statetimer  < 0)
        {
            rb.velocity =new Vector2(0, -.1f);
            if (!skillUsed)
            {
                if(player.skill.blackhole.Canuseskill());
                    skillUsed = true;
            }
        }

        if (player.skill.blackhole.BlackholeFinish())
            statemachine.changestate(player.airstate);
    }
}
