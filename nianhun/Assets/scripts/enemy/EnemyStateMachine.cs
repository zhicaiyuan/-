using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine 
{
    public EnemyState currentstate {  get; private set; }

    public void Initialize(EnemyState _startstate)
    {
        currentstate = _startstate;
        currentstate.enter();
    }

    public void changestate(EnemyState _newstate)
    {
        bool keepAnim = currentstate.animboolname == _newstate.animboolname;

        if (keepAnim)
            currentstate.exitWithoutAnim();
        else
            currentstate.exit();

        currentstate = _newstate;

        if (keepAnim)
            currentstate.enterWithoutAnim();
        else
            currentstate.enter();
    }
}
