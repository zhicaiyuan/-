using UnityEngine;

public class RootBossDashState : EnemyState
{
    private RootBoss enemy;
    private Transform player;
    private int dashDir;
    private bool hasDealtDamage;

    public RootBossDashState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy) : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        AudioManager.instance.PlaySFX(32, null);
        player = playermanger.instance.player.transform;
        enemy.isattack = true;
        enemy.MarkDashUsed();

        float dx = player.position.x - enemy.transform.position.x;
        dashDir = dx >= 0 ? 1 : -1;

        if ((dashDir > 0 && !enemy.faceright) || (dashDir < 0 && enemy.faceright))
            enemy.Flip();

        statetimer = enemy.dashduration;
        hasDealtDamage = false;
        enemy.setvelocity(enemy.dashspeed * dashDir, 0f);
        AudioManager.instance.PlaySFX(29, null);
    }

    public override void exit()
    {
        base.exit();
        enemy.isattack = false;
        enemy.zerovelocity();
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        if (!hasDealtDamage && enemy.TryDealDashDamage())
            hasDealtDamage = true;

        enemy.setvelocity(enemy.dashspeed * dashDir, rb.velocity.y);

        if (statetimer < 0f || enemy.iswalldetected())
        {
            enemy.SyncBattleAnimator(true);
            statemachine.changestate(enemy.battlestate);
        }
    }
}
