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

    public override void Enter()
    {
        base.Enter();
        AudioManager.instance.PlaySFX(9, null);
        AudioManager.instance.playBgm = false;

        player.StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        yield return player.StartCoroutine(player.WaitForDeathAnimation());

        UI ui = Object.FindObjectOfType<UI>();
        if (ui != null)
            ui.SwitchOnEndScreen();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
        player.zerovelocity();
    }
}
