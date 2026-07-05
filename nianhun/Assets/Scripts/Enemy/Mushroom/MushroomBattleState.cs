using UnityEngine;

public class MushroomBattleState : EnemyState
{
    private Transform player;
    private Mushroom enemy;
    private int movedir;

    private const float turnDeadzone = 0.05f;
    private const float closeRangeSlowMultiplier = 0.55f;
    private const float groundedVelocityThreshold = 0.01f;

    public MushroomBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Mushroom mushroom)
        : base(enemyBase, stateMachine, animBoolName)
    {
        enemy = mushroom;
    }

    public override void enter()
    {
        base.enter();
        statetimer = enemy.battletime;

        player = playermanger.instance.player.transform;
        if (player.GetComponent<PlayerStat>().isdead)
            statemachine.changestate(enemy.movestate);
    }

    public override void update()
    {
        base.update();

        float distanceToPlayer = Vector2.Distance(player.position, enemy.transform.position);

        if (IsPlayerInCombatRange(distanceToPlayer))
        {
            statetimer = enemy.battletime;

            RaycastHit2D playerHit = enemy.ispalyerdetected();
            if (playerHit && playerHit.distance < enemy.attackcheckdistance && canattack())
            {
                enemy.isattack = true;
                statemachine.changestate(enemy.attackstate);
                AudioManager.instance.PlaySFX(5, null);
                return;
            }
        }
        else if (statetimer < 0 || distanceToPlayer > 20)
        {
            statemachine.changestate(enemy.idlestate);
            return;
        }

        UpdateMoveDirection(distanceToPlayer);
        UpdateFacing();
        ApplyMovement(distanceToPlayer);
    }

    private void UpdateMoveDirection(float distanceToPlayer)
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

    private void ApplyMovement(float distanceToPlayer)
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

        float speed = enemy.movespeed;
        if (distanceToPlayer <= enemy.attackcheckdistance)
            speed *= closeRangeSlowMultiplier;

        enemy.setvelocity(movedir * speed, rb.velocity.y);
    }

    private bool IsPlayerInCombatRange(float distanceToPlayer)
    {
        return enemy.ispalyerdetected() || distanceToPlayer < Mushroom.CombatDetectDistance;
    }

    private bool canattack()
    {
        if (Time.time >= enemy.lasttimeattack + enemy.attackcooldown)
        {
            enemy.attackcooldown = Random.Range(enemy.minattackcooldown, enemy.maxattackcooldown);
            enemy.lasttimeattack = Time.time;
            return true;
        }

        return false;
    }
}
