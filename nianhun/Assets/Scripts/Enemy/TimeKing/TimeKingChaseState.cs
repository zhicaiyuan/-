using UnityEngine;

public class TimeKingChaseState : EnemyState
{
    private TimeKing enemy;
    private Transform player;
    private int movedir;
    private float currentChaseSpeed;

    private const float turnDeadzone = 0.05f;

    public TimeKingChaseState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, TimeKing timeKing)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = timeKing;
    }

    public override void enter()
    {
        triggercalled = false;
        rb = enemybase.rb;
        enemy.isattack = false;
        enemy.ClearAttackCombo();
        enemy.SyncGroundAnimator(true);
        currentChaseSpeed = enemy.chaseSpeedNear;

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

        if (distanceToPlayer <= enemy.attackcheckdistance)
        {
            enemy.zerovelocity();
            statemachine.changestate(enemy.battlestate);
            return;
        }

        UpdateFacingAndDirection();
        ApplyChaseMovement(distanceToPlayer);
    }

    private void UpdateFacingAndDirection()
    {
        float dx = player.position.x - enemy.transform.position.x;

        if (Mathf.Abs(dx) >= turnDeadzone)
            movedir = dx > 0f ? 1 : -1;
        else
            movedir = 0;

        enemy.FacePlayer();
        enemy.SyncGroundAnimator(movedir != 0);
    }

    private void ApplyChaseMovement(float distanceToPlayer)
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

        float targetSpeed = enemy.GetChaseSpeedForDistance(distanceToPlayer);
        float smoothRate = Mathf.Max(enemy.chaseSpeedSmoothRate, 0.01f);
        currentChaseSpeed = Mathf.Lerp(currentChaseSpeed, targetSpeed, Time.deltaTime * smoothRate);
        enemy.setvelocity(movedir * currentChaseSpeed, rb.velocity.y);
    }
}
