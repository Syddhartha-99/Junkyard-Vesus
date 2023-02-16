using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitialiseSubState();
    }

    public override void EnterState()
    { 
        Ctx.Animator.SetBool(Ctx.IsFallingHash, true);
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsFallingHash, false);
    }

    public override void CheckSwitchStates()
    {
        if (!Ctx.CharacterController.isGrounded && Ctx.IsFlyPressed)
        {
            SwitchState(Factory.Jet());
        }
        else if (Ctx.CharacterController.isGrounded)
        {
            SwitchState(Factory.Grounded());
        }

    }

    public override void InitialiseSubState()
    {
        if (!Ctx.IsMovementPressed && !Ctx.IsDashPressed)
        {
            SetSubState(Factory.Idle());
        }
        else if (Ctx.IsMovementPressed && Ctx.IsDashPressed)
        {
            SetSubState(Factory.Dash());
        }
        else
        {
            SetSubState(Factory.Run());
        }
    }

    public void HandleGravity()
    {
        float previousYVelocity = Ctx.CurrentMovementY;
        Ctx.CurrentMovementY = Ctx.CurrentMovementY + Ctx.Gravity * Time.deltaTime;
        Ctx.AppliedMovementY = MathF.Max((previousYVelocity + Ctx.CurrentMovementY) * 0.5f, -8f);
    }
}
