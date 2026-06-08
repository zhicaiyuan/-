using UnityEngine;

public class PlayerPrayState : PlayerState
{
    private Checkpoint checkpoint;

    public PlayerPrayState(Player _player, PlayerStateMachine _statemachine, string _animboolname)
        : base(_player, _statemachine, _animboolname)
    {
    }

    public override void Enter()
    {
        base.Enter();

        checkpoint = player.NearbyCheckpoint;
        player.zerovelocity();
        player.isbusy = true;
        player.Stat.MakeInvincible(true);
        statetimer = player.prayDuration;
    }

    public override void Exit()
    {
        base.Exit();
        player.isbusy = false;
        player.Stat.MakeInvincible(false);
    }

    public override void Update()
    {
        statetimer -= Time.deltaTime;
        player.anim.SetFloat("yvelocity", rb.velocity.y);
        player.zerovelocity();

        if (triggercalled || statetimer < 0f)
        {
            checkpoint?.ApplyPrayReward();
            statemachine.changestate(player.idlestate);
        }
    }
}
