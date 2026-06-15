using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttack : PlayerState
{
    private static readonly int Attack1Hash = Animator.StringToHash("player attack1");
    private static readonly int Attack2Hash = Animator.StringToHash("playerattack2");
    private static readonly int Attack3Hash = Animator.StringToHash("playerattack3");

    private int combocounter;
    private float lasttimeattack = -999f;
    private float combowindow = 2f;
    private float attackMomentumDuration = 0.18f;
    private float postAttackBusyTime = 0.06f;
    private float maxAttackStateTime = 1.5f;
    private float minHitDuration = 0.12f;
    private EntityFx fx;
    private Vector2 attackVelocity;
    private bool comboQueued;
    private float attackStateTimer;
    private float currentHitTimer;

    public PlayerPrimaryAttack(Player _player, PlayerStateMachine _statemachine, string _animboolname) : base(_player, _statemachine, _animboolname)
    {
        fx = _player.GetComponent<EntityFx>();
    }

    public override void Enter()
    {
        base.Enter();
        comboQueued = false;
        attackStateTimer = 0f;

        if (Time.time >= lasttimeattack + combowindow)
            combocounter = 0;

        StartComboHit();
    }

    public override void Exit()
    {
        base.Exit();

        if (postAttackBusyTime > 0f)
            player.StartCoroutine(player.busyfor(postAttackBusyTime));

        lasttimeattack = Time.time;
        player.ClearAttackBuffer();
    }

    public override void Update()
    {
        base.Update();
        attackStateTimer += Time.deltaTime;
        currentHitTimer += Time.deltaTime;

        if (statetimer > 0f)
        {
            float decay = statetimer / attackMomentumDuration;
            player.setvelocity(attackVelocity.x * decay, attackVelocity.y * decay);
        }
        else
        {
            player.setvelocity(0f, rb.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.J) || player.HasAttackBuffer())
            comboQueued = true;

        if (triggercalled && currentHitTimer >= minHitDuration)
            FinishAttackSwing();
        else if (attackStateTimer >= maxAttackStateTime)
            ForceExitAttack();
    }

    private void FinishAttackSwing()
    {
        if (comboQueued && combocounter < 2)
        {
            comboQueued = false;
            player.ClearAttackBuffer();
            AdvanceCombo();
            return;
        }

        comboQueued = false;
        player.ClearAttackBuffer();
        combocounter = 0;

        if (xinput != 0)
            statemachine.changestate(player.movestate);
        else
            statemachine.changestate(player.idlestate);
    }

    private void ForceExitAttack()
    {
        comboQueued = false;
        player.ClearAttackBuffer();
        combocounter = 0;

        if (xinput != 0)
            statemachine.changestate(player.movestate);
        else
            statemachine.changestate(player.idlestate);
    }

    private void AdvanceCombo()
    {
        combocounter++;
        triggercalled = false;
        attackStateTimer = 0f;
        currentHitTimer = 0f;
        StartComboHit();
    }

    private void StartComboHit()
    {
        lasttimeattack = Time.time;

        AudioManager.instance.PlaySFX(0, null);
        player.anim.SetInteger("combocounter", combocounter);
        PlayComboAnimation();
        fx.CreateAttackFx(player.transform, combocounter);

        float inputX = Input.GetAxisRaw("Horizontal");
        int attackdir = inputX != 0 ? (int)inputX : player.facedir;

        if (inputX != 0 && attackdir != player.facedir)
            player.Flip();

        Vector2 movement = player.attackmovement[combocounter];
        attackVelocity = new Vector2(movement.x * attackdir, movement.y);
        player.setvelocity(attackVelocity.x, attackVelocity.y);
        statetimer = attackMomentumDuration;
    }

    private void PlayComboAnimation()
    {
        int stateHash = combocounter switch
        {
            1 => Attack2Hash,
            2 => Attack3Hash,
            _ => Attack1Hash
        };

        player.anim.Play(stateHash, 0, 0f);
    }
}
