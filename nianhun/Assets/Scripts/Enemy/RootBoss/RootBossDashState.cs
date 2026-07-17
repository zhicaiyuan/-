using UnityEngine;

public class RootBossDashState : EnemyState
{
    private RootBoss enemy;
    private Transform player;
    private int dashDir;
    private bool hasDealtDamage;

    public RootBossDashState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        // 不用 base.enter 的纯 bool 切换：Change/Walk 后 AnyState 可能接不住，强制播冲撞动画
        triggercalled = false;
        rb = enemybase.rb;

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(32, null);

        if (playermanger.instance != null && playermanger.instance.player != null)
            player = playermanger.instance.player.transform;

        enemy.isattack = true;
        enemy.MarkDashUsed();

        if (player != null)
        {
            float dx = player.position.x - enemy.transform.position.x;
            dashDir = dx >= 0f ? 1 : -1;
            enemy.FacePlayer(player.position.x);
        }
        else
        {
            dashDir = enemy.facedir;
        }

        enemy.anim.SetBool("Move", false);
        enemy.anim.SetBool("Attack", false);
        enemy.anim.SetBool("Change", false);
        enemy.anim.SetBool("Stun", false);
        enemy.anim.SetBool("Dash", true);
        enemy.anim.Play("RootAttack3", 0, 0f);

        statetimer = enemy.dashduration;
        hasDealtDamage = false;
        enemy.setvelocity(enemy.dashspeed * dashDir, 0f);

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(29, null);
    }

    public override void exit()
    {
        enemybase.anim.SetBool("Dash", false);
        enemybase.AssignlastAnimName(animboolname);
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

        // 撞墙时按冲刺方向检测，避免朝向标志不同步导致提前结束
        bool hitWall = enemy.iswalldetected(dashDir);
        enemy.setvelocity(enemy.dashspeed * dashDir, rb.velocity.y);

        if (statetimer < 0f || hitWall)
        {
            enemy.SyncBattleAnimator(true);
            statemachine.changestate(enemy.battlestate);
        }
    }
}
