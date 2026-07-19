using UnityEngine;

public class PlayerAirState : PlayerState
{
    private bool transitionControlled;
    private float transitionVerticalDirection = 1f;
    private float transitionSpeedMultiplier = 1f;

    public PlayerAirState(Player _player, PlayerStateMachine _statemachine, string _animboolname)
        : base(_player, _statemachine, _animboolname)
    {
    }

    public void BeginTransitionMove(float verticalDirection, float speedMultiplier)
    {
        transitionControlled = true;
        transitionVerticalDirection = verticalDirection == 0f ? 1f : Mathf.Sign(verticalDirection);
        transitionSpeedMultiplier = Mathf.Max(0.05f, speedMultiplier);
    }

    public void SetTransitionSpeedMultiplier(float speedMultiplier)
    {
        transitionSpeedMultiplier = Mathf.Max(0.05f, speedMultiplier);
    }

    public void EndTransitionMove()
    {
        transitionControlled = false;
        transitionSpeedMultiplier = 1f;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        EndTransitionMove();
    }

    public override void Update()
    {
        if (transitionControlled)
        {
            UpdateTransitionMove();
            return;
        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.P) && player.blackHole.CanSkill())
            statemachine.changestate(player.blackholestate);

        if (player.iswalldetected() && SkillManager.instance.wallJump.wallJumpUnlocked)
            statemachine.changestate(player.wallslide);

        if (player.isgrounddetected())
            statemachine.changestate(player.idlestate);

        if (xinput != 0)
            player.setvelocity(player.movespeed * xinput, rb.velocity.y);

        if (player.jumpKeyDown)
        {
            if (player.CanCoyoteJump())
                player.ExecuteGroundJump();
            else
                player.TryJump();
        }
    }

    private void UpdateTransitionMove()
    {
        float speed = player.movespeed * transitionSpeedMultiplier;
        float yVelocity = transitionVerticalDirection * speed;
        player.setvelocity(0f, yVelocity);
        player.anim.SetFloat("yvelocity", yVelocity);
    }
}
