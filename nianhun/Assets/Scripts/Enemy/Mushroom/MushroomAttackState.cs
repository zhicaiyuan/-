using UnityEngine;

public class MushroomAttackState : EnemyState
{
    private Mushroom enemy;

    public MushroomAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = mushroom;
    }

    public override void exit()
    {
        base.exit();
        enemy.lasttimeattack = Time.time;
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled)
        {
            enemy.isattack = false;
            statemachine.changestate(enemy.battlestate);
        }
    }
}
