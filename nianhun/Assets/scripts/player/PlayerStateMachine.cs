using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine 
{
    public PlayerState currentstate {  get; private set; }
    
    public void initialize(PlayerState _startstate)
    {
        currentstate = _startstate;
        currentstate.enter();
    }

    public void changestate(PlayerState _newstate)
    {
        currentstate.exit();
        currentstate = _newstate;
        currentstate.enter();
    }
}
