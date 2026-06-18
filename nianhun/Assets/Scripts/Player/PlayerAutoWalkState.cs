using UnityEngine;

public class PlayerAutoWalkState : PlayerState
{
    private float walkDirection = 1f;
    private float speedMultiplier = 1f;
    private bool lockVerticalVelocity;

    public PlayerAutoWalkState(Player player, PlayerStateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName)
    {
    }

    public void SetWalkDirection(float direction)
    {
        walkDirection = direction == 0f ? 1f : Mathf.Sign(direction);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Max(0.05f, multiplier);
    }

    public void SetLockVerticalVelocity(bool locked)
    {
        lockVerticalVelocity = locked;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        speedMultiplier = 1f;
        lockVerticalVelocity = false;
    }

    public override void Update()
    {
        float yVelocity = lockVerticalVelocity ? 0f : rb.velocity.y;
        player.anim.SetFloat("yvelocity", yVelocity);
        player.setvelocity(walkDirection * player.movespeed * speedMultiplier, yVelocity);
    }
}
