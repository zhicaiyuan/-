using UnityEngine;

public class SlimeGroundState : EnemyState
{
    protected Slime enemy;
    protected Transform player;

    public SlimeGroundState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
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
