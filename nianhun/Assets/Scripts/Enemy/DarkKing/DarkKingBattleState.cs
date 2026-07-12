using UnityEngine;

public class DarkKingBattleState : EnemyState
{
    private DarkKing enemy;
    private Transform player;
    private int movedir;
    private const float turnDeadzone = 0.05f;

    public DarkKingBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, DarkKing darkKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = darkKing;
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
            if (DarkKingCombatPatterns.TryPickSkill(enemy, distanceToPlayer, out DarkKingAttackType skill))
            {
                enemy.EnterSkill(skill);
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
        ApplyMovement();
    }

    private void UpdateMovement(float distanceToPlayer)
    {
        float dx = player.position.x - enemy.transform.position.x;
        bool inMelee = distanceToPlayer <= enemy.attackcheckdistance;

        if (inMelee || Mathf.Abs(dx) >= turnDeadzone)
        {
            if (dx > turnDeadzone)
                movedir = 1;
            else if (dx < -turnDeadzone)
                movedir = -1;
            else
                movedir = 0;
        }
        else
            movedir = 0;

        enemy.SyncGroundAnimator(movedir != 0);
    }

    private void ApplyMovement()
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

        enemy.setvelocity(movedir * enemy.movespeed, rb.velocity.y);
    }
}
