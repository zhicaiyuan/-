using UnityEngine;

public class SkeletonBattleState : EnemyState
{
    private Transform player;
    private Skeleton enemy;
    private int movedir;

    private float flipCooldown;
    private const float flipDelay = 0.2f;
    private const float stopTurnDistance = 0.8f;
    private const float turnDeadzone = 0.15f;

    public SkeletonBattleState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname, Skeleton enemy)
        : base(_enemybase, _statemachine, _animboolname)
    {
        this.enemy = enemy;
    }

    public override void enter()
    {
        base.enter();
        flipCooldown = 0f;

        if (!TryResolvePlayer())
            return;

        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null && playerStat.isdead)
            statemachine.changestate(enemy.movestate);
    }

    public override void update()
    {
        base.update();

        if (!TryResolvePlayer())
            return;

        if (enemy.ispalyerdetected())
        {
            statetimer = enemy.battletime;
            if (enemy.ispalyerdetected().distance < enemy.attackcheckdistance && canattack())
            {
                enemy.isattack = true;
                statemachine.changestate(enemy.attackstate);
                if (AudioManager.instance != null)
                    AudioManager.instance.PlaySFX(5, null);
            }
        }
        else
        {
            if (statetimer < 0 || Vector2.Distance(player.position, enemy.transform.position) > 20)
                statemachine.changestate(enemy.idlestate);
        }

        if (flipCooldown > 0f)
            flipCooldown -= Time.deltaTime;

        if (Vector2.Distance(player.position, enemy.transform.position) > stopTurnDistance)
        {
            float dx = player.position.x - enemy.transform.position.x;
            if (Mathf.Abs(dx) < turnDeadzone)
                movedir = 0;
            else if (dx > 0)
                movedir = 1;
            else
                movedir = -1;
        }
        else
        {
            movedir = 0;
        }

        if (!enemy.canMoveInDirection(movedir))
        {
            movedir = 0;
            enemy.zerovelocity();
        }
        else if (rb.velocity.y == 0)
        {
            enemy.setvelocity(movedir * enemy.movespeed, rb.velocity.y);
        }

        if (flipCooldown <= 0f && movedir != 0)
        {
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

    private bool TryResolvePlayer()
    {
        if (player != null)
            return true;

        if (playermanger.instance == null || playermanger.instance.player == null)
            return false;

        player = playermanger.instance.player.transform;
        return player != null;
    }
}
