using UnityEngine;

public class MushroomGroundState : EnemyState
{
    protected Mushroom enemy;
    protected Transform player;

    public MushroomGroundState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = mushroom;
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

        if (enemy.ispalyerdetected() || Vector2.Distance(enemy.transform.position, player.position) < Mushroom.CombatDetectDistance)
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
