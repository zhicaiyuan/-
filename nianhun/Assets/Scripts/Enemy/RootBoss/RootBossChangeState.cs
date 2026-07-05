using UnityEngine;

public class RootBossChangeState : EnemyState
{
    private RootBoss enemy;

    public RootBossChangeState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        enemy.SyncBattleAnimator(false);
        base.enter();
        enemy.zerovelocity();
        enemy.isattack = false;
        enemy.Stat.MakeInvincible(true);
        statetimer = enemy.changeDuration;
    }

    public override void exit()
    {
        base.exit();
        enemy.Stat.MakeInvincible(false);
        enemy.OnTransformComplete();
    }

    public override void update()
    {
        base.update();
        enemy.zerovelocity();

        if (triggercalled || statetimer < 0f)
        {
            enemy.SyncBattleAnimator(true);
            statemachine.changestate(enemy.battlestate);
        }
    }
}
