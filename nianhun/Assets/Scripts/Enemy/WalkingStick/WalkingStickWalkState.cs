using UnityEngine;

public class WalkingStickWalkState : EnemyState
{
    private Transform player;
    private WalkingStick enemy;
    private int movedir;

    private const float turnDeadzone = 0.05f;
    private const float groundedVelocityThreshold = 0.01f;

    public WalkingStickWalkState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, WalkingStick walkingStick)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = walkingStick;
    }

    public override void enter()
    {
        base.enter();
        enemy.SetSuperArmor(false);
        enemy.SyncGroundAnimator(true);

        if (playermanger.instance == null || playermanger.instance.player == null)
            return;

        player = playermanger.instance.player.transform;
    }

    public override void update()
    {
        base.update();

        if (player == null)
        {
            if (playermanger.instance != null && playermanger.instance.player != null)
                player = playermanger.instance.player.transform;
            else
                return;
        }

        if (player.GetComponent<PlayerStat>().isdead)
        {
            enemy.zerovelocity();
            return;
        }

        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        if (enemy.IsPlayerInAttack3Range() && enemy.IsAttackReady())
        {
            enemy.FacePlayer();
            enemy.CurrentAttack = WalkingStickAttackType.Attack3;
            enemy.ConsumeAttackCooldown();
            enemy.isattack = true;
            statemachine.changestate(enemy.attackstate);
            AudioManager.instance.PlaySFX(5, null);
            return;
        }

        UpdateMoveDirection();
        UpdateFacing();
        ApplyMovement();
    }

    private void UpdateMoveDirection()
    {
        float dx = player.position.x - enemy.transform.position.x;

        if (Mathf.Abs(dx) >= turnDeadzone)
            movedir = dx > turnDeadzone ? 1 : -1;
        else
            movedir = 0;
    }

    private void UpdateFacing()
    {
        if (movedir == 0)
            return;

        if (movedir > 0 && !enemy.faceright)
            enemy.Flip();
        else if (movedir < 0 && enemy.faceright)
            enemy.Flip();
    }

    private void ApplyMovement()
    {
        if (Mathf.Abs(rb.velocity.y) > groundedVelocityThreshold)
            return;

        if (!enemy.canMoveInDirection(movedir))
        {
            enemy.zerovelocity();
            return;
        }

        if (movedir == 0)
        {
            enemy.zerovelocity();
            return;
        }

        enemy.setvelocity(movedir * enemy.movespeed, rb.velocity.y);
    }
}
