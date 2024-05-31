using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : State
{
    Enemy controller;
    public Die(Enemy controller)
    {
        this.controller = controller;
    }

    public override void OnEnter()
    {
        controller.GetComponent<Animator>().SetBool("Dead", true);
        controller.Move(Vector3.zero);
    }

    public override void OnUpdate()
    {
        controller.Move(Vector3.zero);
    }
}
