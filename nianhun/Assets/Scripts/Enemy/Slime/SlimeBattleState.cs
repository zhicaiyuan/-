using UnityEngine;

public class SlimeBattleState : EnemyState
{
    protected Slime enemy;
    private Transform player;
    private int movedir;

    private float flipCooldown;
    private const float flipDelay = 0.1f;
    private const float turnDeadzone = 0.05f;
    private const float closeRangeSlowMultiplier = 0.55f;
    private const float groundedVelocityThreshold = 0.01f;

    public SlimeBattleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Slime slime)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = slime;
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

        if (enemy.ispalyerdetected())
        {
            statetimer = enemy.battletime;
            if (enemy.ispalyerdetected().distance < enemy.attackcheckdistance && canattack())
            {
                enemy.isattack = true;
                statemachine.changestate(enemy.attackstate);
                AudioManager.instance.PlaySFX(29, null);
                return;
            }
        }
        else
        {
            if (statetimer < 0 || Vector2.Distance(player.position, enemy.transform.position) > 20)
                statemachine.changestate(enemy.idlestate);
        }

        if (flipCooldown > 0f)
            flipCooldown -= Time.deltaTime;

        UpdateMoveDirection();
        UpdateFacing();
        ApplyMovement();
    }

    private void UpdateMoveDirection()
    {
        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);
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
    }

    private void UpdateFacing()
    {
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

    private void ApplyMovement()
    {
        if (Mathf.Abs(rb.velocity.y) > groundedVelocityThreshold)
            return;

        if (!enemy.canMoveInDirection(movedir))
        {
            movedir = 0;
            enemy.zerovelocity();
            return;
        }

        float speed = enemy.movespeed;
        if (Vector2.Distance(player.position, enemy.transform.position) <= enemy.attackcheckdistance)
            speed *= closeRangeSlowMultiplier;

        enemy.setvelocity(movedir * speed, rb.velocity.y);
    }

    private bool canattack()
    {
        if (!enemy.CanAttack())
            return false;

        if (Time.time >= enemy.lasttimeattack + enemy.attackcooldown)
        {
            enemy.attackcooldown = Random.Range(enemy.minattackcooldown, enemy.maxattackcooldown);
            enemy.lasttimeattack = Time.time;
            return true;
        }
        return false;
    }
}
