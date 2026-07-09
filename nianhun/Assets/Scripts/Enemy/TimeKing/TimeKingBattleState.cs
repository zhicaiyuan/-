using UnityEngine;

public class TimeKingBattleState : EnemyState
{
    private TimeKing enemy;
    private Transform player;
    private int movedir;

    private const float turnDeadzone = 0.05f;
    private const float closeRangeSlowMultiplier = 0.55f;

    public TimeKingBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.SyncGroundAnimator(true);

        if (playermanger.instance != null && playermanger.instance.player != null)
            player = playermanger.instance.player.transform;
    }

    public override void update()
    {
        base.update();

        if (enemy.TryStartPhaseTransition())
            return;

        if (player == null)
        {
            if (playermanger.instance == null || playermanger.instance.player == null)
                return;

            player = playermanger.instance.player.transform;
        }

        if (player.GetComponent<PlayerStat>().isdead)
        {
            enemy.zerovelocity();
            return;
        }

        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);
        bool playerDetected = enemy.ispalyerdetected().collider != null;

        if (playerDetected)
            statetimer = enemy.battletime;

        if (playerDetected || distanceToPlayer < enemy.battleDetectDistance)
        {
            if (distanceToPlayer > enemy.jumpAttackDistance && enemy.IsSkillReady(TimeKingAttackType.JumpAttack))
            {
                enemy.CurrentAttack = TimeKingAttackType.JumpAttack;
                enemy.ConsumeSkillCooldown(TimeKingAttackType.JumpAttack);
                statemachine.changestate(enemy.jumpattackstate);
                return;
            }

            if (distanceToPlayer <= enemy.attackcheckdistance &&
                TimeKingCombatPatterns.TryPickCombo(enemy, out TimeKingAttackType[] combo))
            {
                enemy.BeginAttackCombo(combo);
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
        enemy.FacePlayer();
        ApplyMovement(distanceToPlayer);
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
                movedir = 0;
        }
        else
        {
            movedir = 0;
        }

        enemy.SyncGroundAnimator(movedir != 0);
    }

    private void ApplyMovement(float distanceToPlayer)
    {
        if (Mathf.Abs(rb.velocity.y) > 0.01f)
            return;

        if (!enemy.canMoveInDirection(movedir))
        {
            movedir = 0;
            enemy.zerovelocity();
            enemy.SyncGroundAnimator(false);
            return;
        }

        float speed = enemy.movespeed;
        if (distanceToPlayer <= enemy.attackcheckdistance)
            speed *= closeRangeSlowMultiplier;

        enemy.setvelocity(movedir * speed, rb.velocity.y);
    }
}
