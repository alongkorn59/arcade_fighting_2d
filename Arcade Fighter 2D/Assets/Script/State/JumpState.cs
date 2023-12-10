using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : IState
{
    private PlayerController controller;
    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
    }
    public void UpdateState()
    {
    }

    public void OnExit()
    {
    }
}
