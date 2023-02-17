using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitialiseSubState();
    }

    public override void EnterState()
    {
        HandleGravity();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        if (Ctx.IsJumpPressed && !Ctx.RequireNewJumpPress)
        {
            SwitchState(Factory.Jump());
        }

        else if (!Ctx.CharacterController.isGrounded && Ctx.IsFlyPressed)
        {
            SwitchState(Factory.Jet());
        }
        else if (!Ctx.CharacterController.isGrounded)
        {
            SwitchState(Factory.Fall());
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
        Ctx.CurrentMovementY = Ctx.Gravity;
        Ctx.AppliedMovementY = Ctx.Gravity;
    }
}
