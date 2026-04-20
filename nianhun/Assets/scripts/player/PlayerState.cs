using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState 
{
    protected PlayerStateMachine statemachine;
    protected Player player;

    protected Rigidbody2D rb;

    private string animboolname;
    protected float xinput;

    protected float statetimer;
    protected bool triggercalled;
    public PlayerState(Player _player, PlayerStateMachine _statemachine,string _animboolname)
    {
        this.statemachine = _statemachine;
        this.player = _player;
        this.animboolname = _animboolname;
    }
    public virtual void enter()
    {
        player.anim.SetBool(animboolname,true);
        rb =player.rb;
        triggercalled = false;
    }
    public virtual void exit()
    {
        player.anim.SetBool(animboolname, false);
    }
    public virtual void update()
    {
        xinput = Input.GetAxisRaw("Horizontal");
        player.anim.SetFloat("yvelocity",rb.velocity.y);
        statetimer -= Time.deltaTime;
    }

    public virtual void animationfinishtrigger()
    {
        triggercalled = true;
    }

    
}
