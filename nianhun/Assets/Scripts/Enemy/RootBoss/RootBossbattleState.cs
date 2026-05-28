using UnityEngine;

public class RootBossbattleState : EnemyState
{
    protected RootBoss enemy;
    private Transform player;
    private int movedir;

    private float flipCooldown;
    private const float flipDelay = 0.1f;
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
            if (enemy.dashUnlocked && enemy.CanDash() && enemy.IsDashInRange(player.position) && TryConsumeAttackCooldown())
            {
                statemachine.changestate(enemy.dashstate);
                return;
            }

            if (distanceToPlayer <= enemy.attackcheckdistance && TryConsumeAttackCooldown())
            {
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

        if (Mathf.Abs(dx) < turnDeadzone)
            movedir = 0;
        else if (dx > 0)
            movedir = 1;
        else
            movedir = -1;

        bool isWalking = movedir != 0;
        enemy.anim.SetBool("Move", isWalking);
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

    private bool TryConsumeAttackCooldown()
    {
        if (Time.time < enemy.lasttimeattack + enemy.attackcooldown)
            return false;

        enemy.attackcooldown = Random.Range(enemy.minattackcooldown, enemy.maxattackcooldown);
        enemy.lasttimeattack = Time.time;
        return true;
    }
}
