using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState, IRootState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        InitialiseSubState();
    }

    public override void EnterState()
    {
        HandleJump();
    }

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
        if(Ctx.IsJumpPressed)
        {
            Ctx.RequireNewJumpPress = true;
        }
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
        if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed)
        {
            SetSubState(Factory.Idle());
        }
        else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed)
        {
            SetSubState(Factory.Walk());
        }
        else
        {
            SetSubState(Factory.Run());
        }
    }

    private void HandleJump()
    {
        if (Ctx.IsJumpPressed)
        {
            Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
            Ctx.IsJumping = true;
            Ctx.CurrentMovementY = Ctx.InitialJumpVelocity;
            Ctx.AppliedMovementY = Ctx.InitialJumpVelocity;
        }
        else if (!Ctx.IsJumpPressed)
        {
            Ctx.IsJumping = false;
        }
    }

    public void HandleGravity()
    {
        float previousYVelocity = Ctx.CurrentMovementY;
        Ctx.CurrentMovementY = Ctx.CurrentMovementY + Ctx.Gravity * Time.deltaTime;
        Ctx.AppliedMovementY = (previousYVelocity + Ctx.CurrentMovementY) * 0.5f;
    }
}
