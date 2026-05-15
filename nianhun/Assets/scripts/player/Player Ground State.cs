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
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.dashchance = 1;
        if (Input.GetKeyDown(KeyCode.P) && player.blackHole.CanSkill() && player.skill.blackhole.blackHoleUnlocked)
            statemachine.changestate(player.blackholestate);

        if (Input.GetKeyDown(KeyCode.O) && player.spin.CanSkill() && player.skill.spin.spinUnlocked)
            statemachine.changestate(player.spinstate);

        if(Input.GetKeyDown(KeyCode.I) && player.strike.CanSkill() && player.skill.strike.strikeUnlocked)
            statemachine.changestate(player.strikeSkillState);

        if (Input.GetKeyDown(KeyCode.U) && player.skill.parry.parryUnlocked)
            statemachine.changestate(player.counterattackstate);

        if (Input.GetKeyDown(KeyCode.J))
            statemachine.changestate(player.primaryattack);

        if (Input.GetKeyDown(KeyCode.K) && player.isgrounddetected() && !player.iswalldetected())
            statemachine.changestate(player.jumpstate);

        if(Input.GetKeyDown(KeyCode.L) && player.laser.CanSkill() && player.skill.laser.laserUnlocked)
            statemachine.changestate(player.laserState);

        if (!player.isgrounddetected())
            statemachine.changestate(player.airstate);
    }


}
