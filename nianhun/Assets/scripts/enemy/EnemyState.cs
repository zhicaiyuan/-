using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState 
{
    protected EnemyStateMachine statemachine;
    protected Enemy enemybase;
    protected Rigidbody2D rb;
    

    protected bool triggercalled;
    private string animboolname;
   protected float statetimer;

    public EnemyState(Enemy _enemybase, EnemyStateMachine _statemachine, string _animboolname)
    {
        this.enemybase = _enemybase;
        this.statemachine = _statemachine;
        this.animboolname = _animboolname;

    }

    public virtual void enter()
    {
        triggercalled = false;
        enemybase.anim.SetBool(animboolname, true);
        rb =  enemybase.rb;
    }

    public virtual void update()
    {
        statetimer -= Time.deltaTime;
    }

    public virtual void exit()
    {
        enemybase.anim.SetBool(animboolname, false);
        enemybase.AssignlastAnimName(animboolname);

    }
    public virtual void aniamtionfinishtrigger()
    {
        triggercalled = true;
    }
}
