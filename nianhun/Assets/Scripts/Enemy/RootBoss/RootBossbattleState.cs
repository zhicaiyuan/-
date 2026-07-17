using UnityEngine;

public class RootBossbattleState : EnemyState
{
    protected RootBoss enemy;
    private Transform player;
    private int movedir;

    private float flipCooldown;
    private const float flipDelay = 0.12f;
    private const float turnDeadzone = 0.15f;
    private const float closeRangeSlowMultiplier = 0.55f;

    public RootBossbattleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, RootBoss enemy)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        flipCooldown = 0f;
        enemy.SyncBattleAnimator(true);
        TryResolvePlayer();

        if (player != null)
        {
            PlayerStat playerStat = player.GetComponent<PlayerStat>();
            if (playerStat != null && playerStat.isdead)
                statemachine.changestate(enemy.movestate);
        }
    }

    public override void update()
    {
        base.update();

        if (!TryResolvePlayer())
            return;

        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null && playerStat.isdead)
        {
            enemy.zerovelocity();
            statemachine.changestate(enemy.movestate);
            return;
        }

        if (enemy.TryStartPhaseTransition())
            return;

        bool playerDetected = enemy.ispalyerdetected();
        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        if (playerDetected)
            statetimer = enemy.battletime;

        if (playerDetected || distanceToPlayer < enemy.battleDetectDistance)
        {
            // 二阶段冲撞独立冷却，不再被普攻冷却卡住
            if (enemy.dashUnlocked && enemy.CanDash() && enemy.IsDashInRange(player.position))
            {
                enemy.FacePlayer(player.position.x);
                statemachine.changestate(enemy.dashstate);
                return;
            }

            if (distanceToPlayer <= enemy.attackcheckdistance && enemy.IsAttackReady())
            {
                enemy.ConsumeAttackCooldown();
                enemy.FacePlayer(player.position.x);
                enemy.BeginAttackCombo(RootBossCombatPatterns.PickMeleeCombo());
                statemachine.changestate(enemy.attackstate);
                return;
            }
        }
        else if (statetimer < 0f || distanceToPlayer > enemy.battleLoseDistance)
        {
            statemachine.changestate(enemy.idlestate);
            return;
        }

        UpdateMovement(distanceToPlayer);
        ApplyMovement(distanceToPlayer);
        enemy.FacePlayer(player.position.x, ref flipCooldown, flipDelay);
    }

    private bool TryResolvePlayer()
    {
        if (player != null)
            return true;

        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        player = playermanger.instance.player.transform;
        return player != null;
    }

    private void UpdateMovement(float distanceToPlayer)
    {
        float dx = player.position.x - enemy.transform.position.x;
        bool inMeleeRange = distanceToPlayer <= enemy.attackcheckdistance;

        if (inMeleeRange || Mathf.Abs(dx) >= turnDeadzone)
        {
            if (dx > turnDeadzone)
                movedir = 1;
            else if (dx < -turnDeadzone)
                movedir = -1;
            else
                movedir = enemy.facedir;
        }
        else
        {
            movedir = 0;
        }

        enemy.anim.SetBool("Move", movedir != 0);
    }

    private void ApplyMovement(float distanceToPlayer)
    {
        if (Mathf.Abs(rb.velocity.y) > 0.05f)
            return;

        if (!enemy.canMoveInDirection(movedir))
        {
            movedir = 0;
            enemy.zerovelocity();
            enemy.anim.SetBool("Move", false);
            return;
        }

        float speed = enemy.movespeed;
        if (distanceToPlayer <= enemy.attackcheckdistance)
            speed *= closeRangeSlowMultiplier;

        enemy.setvelocity(movedir * speed, rb.velocity.y);
    }
}
