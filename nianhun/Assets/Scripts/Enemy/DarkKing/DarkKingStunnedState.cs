using UnityEngine;

public class DarkKingStunnedState : EnemyState
{
    private DarkKing enemy;

    public DarkKingStunnedState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.zerovelocity();
        statetimer = enemy.stuntime > 0f ? enemy.stuntime : 1.5f;
        enemy.fx.InvokeRepeating("redcolourblink", 0, .1f);
    }

    public override void exit()
    {
        enemy.fx.Invoke("cancelcolorchange", 0);
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (statetimer < 0f)
            statemachine.changestate(enemy.battlestate);
    }
}
