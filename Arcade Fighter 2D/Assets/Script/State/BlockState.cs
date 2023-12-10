using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockState : IState
{
    private PlayerController controller;
    public void OnEnter(PlayerController controller)
    {
        this.controller = controller;
        
        controller.Block();
    }
    public void UpdateState()
    {
    }

    public void OnExit()
    {
    }
}
