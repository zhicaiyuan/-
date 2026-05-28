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

        if (!hasDealtDamage && IsPlayerInDashHitRange())
        {
            enemy.DealDamageToDetectedPlayers(1.35f);
            hasDealtDamage = true;
        }

        enemy.setvelocity(enemy.dashspeed * dashDir, rb.velocity.y);

        if (statetimer < 0f || enemy.iswalldetected())
            statemachine.changestate(enemy.battlestate);
    }

    private bool IsPlayerInDashHitRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemy.attackcheck.position, enemy.attackcheckradius * 1.35f);
        foreach (var col in colliders)
        {
            if (col.GetComponent<Player>() != null)
                return true;
        }
        return false;
    }
}
