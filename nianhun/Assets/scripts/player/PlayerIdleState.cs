using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.zerovelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // base.Update 里可能已经切到跳跃等状态，不能再清速度，否则会吞掉起跳
        if (statemachine.currentstate != this)
            return;

        if (xinput != 0 && !player.isbusy)
        {
            statemachine.changestate(player.movestate);
            return;
        }

        // 站在移动平台上时持续同步平台速度，否则 Idle 进入时清零后就跟不住
        player.zerovelocity();
    }
}
