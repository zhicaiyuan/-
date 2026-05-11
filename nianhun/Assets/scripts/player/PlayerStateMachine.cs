using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerStateMachine 
{
    public PlayerState currentstate {  get; private set; }
    

    public void initialize(PlayerState _startstate)
    {
        currentstate = _startstate;
        currentstate.Enter();
    }

    public void changestate(PlayerState _newstate)
    {
        currentstate.Exit();
        currentstate = _newstate;
        currentstate.Enter();
    }
}
