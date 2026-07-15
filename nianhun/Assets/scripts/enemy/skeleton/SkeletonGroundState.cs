using UnityEngine;

public class SkeleonGroundState : EnemyState
{
    protected Skeleton enemy;
    protected Transform player;

    public SkeleonGroundState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        TryResolvePlayer();
    }

    public override void update()
    {
        base.update();

        if (!TryResolvePlayer())
            return;

        if (enemy.ispalyerdetected() || Vector2.Distance(enemy.transform.position, player.position) < 2)
            statemachine.changestate(enemy.battlestate);
    }

    protected bool TryResolvePlayer()
    {
        if (player != null)
            return true;

        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        player = playermanger.instance.player.transform;
        return player != null;
    }
}
