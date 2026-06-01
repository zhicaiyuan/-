using UnityEngine;

public class RootBossbattleState : EnemyState
{
    protected RootBoss enemy;
    private Transform player;
    private int movedir;

    private float flipCooldown;
    private const float flipDelay = 0.1f;
    private const float turnDeadzone = 0.05f;
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

        player = playermanger.instance.player.transform;
        if (player.GetComponent<PlayerStat>().isdead)
            statemachine.changestate(enemy.movestate);
    }

    public override void update()
    {
        base.update();

        if (enemy.ShouldTransform())
        {
            enemy.TryStartPhaseTransition();
            return;
        }

        bool playerDetected = enemy.ispalyerdetected();
        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        if (playerDetected)
            statetimer = enemy.battletime;

        if (playerDetected || distanceToPlayer < enemy.battleDetectDistance)
        {
            if (enemy.dashUnlocked && enemy.CanDash() && enemy.IsDashInRange(player.position) && enemy.IsAttackReady())
            {
                enemy.ConsumeAttackCooldown();
                statemachine.changestate(enemy.dashstate);
                return;
            }

            if (distanceToPlayer <= enemy.attackcheckdistance && enemy.IsAttackReady())
            {
                enemy.ConsumeAttackCooldown();
                enemy.BeginAttackCombo(RootBossCombatPatterns.PickMeleeCombo());
                statemachine.changestate(enemy.attackstate);
                AudioManager.instance.PlaySFX(29, null);
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
        UpdateFacing();
    }

    private void UpdateMovement(float distanceToPlayer)
    {
        float dx = player.position.x - enemy.transform.position.x;
        bool inMeleeRange = distanceToPlayer <= enemy.attackcheckdistance;

        // 贴身或冷却中仍要朝玩家移动，避免 movedir=0 站在原地发呆
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
        if (rb.velocity.y != 0)
            return;

        float speed = enemy.movespeed;
        if (distanceToPlayer <= enemy.attackcheckdistance)
            speed *= closeRangeSlowMultiplier;

        enemy.setvelocity(movedir * speed, rb.velocity.y);
    }

    private void UpdateFacing()
    {
        if (flipCooldown > 0f)
            flipCooldown -= Time.deltaTime;

        if (flipCooldown > 0f || movedir == 0)
            return;

        if (movedir > 0 && !enemy.faceright)
        {
            enemy.Flip();
            flipCooldown = flipDelay;
        }
        else if (movedir < 0 && enemy.faceright)
        {
            enemy.Flip();
            flipCooldown = flipDelay;
        }
    }

}
