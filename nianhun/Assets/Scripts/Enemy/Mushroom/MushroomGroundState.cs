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
        player = playermanger.instance.player.transform;
    }

    public override void update()
    {
        base.update();

        if (enemy.ispalyerdetected() || Vector2.Distance(enemy.transform.position, player.position) < Mushroom.CombatDetectDistance)
            statemachine.changestate(enemy.battlestate);
    }
}
