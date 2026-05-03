using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerState
{
    private PlayerDeadState deadstate;
    private string v;

    public PlayerDeadState(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
    }

   

    public override void animationfinishtrigger()
    {
        base.animationfinishtrigger();
    }

    public override void enter()
    {
        base.enter();
        AudioManager.instance.PlaySFX(9, null);
        AudioManager.instance.playBgm = false;

        GameObject.Find("画布").GetComponent<UI>().SwitchOnEndScreen();
    }

    public override void exit()
    {
        base.exit();
    }

    public override void update()
    {
        base.update();
        player.zerovelocity();
    }
}
