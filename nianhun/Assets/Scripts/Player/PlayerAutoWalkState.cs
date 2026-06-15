using UnityEngine;

public class PlayerAutoWalkState : PlayerState
{
    private float walkDirection = 1f;

    public PlayerAutoWalkState(Player player, PlayerStateMachine stateMachine, string animBoolName)
        : base(player, stateMachine, animBoolName)
    {
    }

    public void SetWalkDirection(float direction)
    {
        walkDirection = direction == 0f ? 1f : Mathf.Sign(direction);
    }

    public override void Update()
    {
        player.anim.SetFloat("yvelocity", rb.velocity.y);
        player.setvelocity(walkDirection * player.movespeed, rb.velocity.y);
    }
}
