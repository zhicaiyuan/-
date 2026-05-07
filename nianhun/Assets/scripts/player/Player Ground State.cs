using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.jumpchance = 1;
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

        if (Input.GetKeyDown(KeyCode.O) && player.spin.CanSkill())
            statemachine.changestate(player.spinstate);

        if (Input.GetKeyDown(KeyCode.U))
            statemachine.changestate(player.counterattackstate);

        if (Input.GetKeyDown(KeyCode.J))
            statemachine.changestate(player.primaryattack);

        if (Input.GetKeyDown(KeyCode.K) && player.isgrounddetected() && !player.iswalldetected())
            statemachine.changestate(player.jumpstate);

        if (!player.isgrounddetected())
            statemachine.changestate(player.airstate);
    }


}
