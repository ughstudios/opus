using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MobController
{

    protected override void FixedUpdate()
    {
        moveInput.Set(Input.GetAxisRaw("Horizontal"),
                Input.GetButton("Jump") ? 1f : 0f,
                Input.GetAxisRaw("Vertical"));

        base.FixedUpdate();
    }
}
