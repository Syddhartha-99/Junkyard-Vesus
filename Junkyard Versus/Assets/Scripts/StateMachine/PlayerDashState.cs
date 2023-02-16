using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    public PlayerDashState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) : base(currentContext, playerStateFactory)
    {

    }

    public override void EnterState()
    {
        Ctx.Animator.SetBool(Ctx.IsDashingHash, true);
    }

    public override void UpdateState()
    {
        if (Ctx.DashGas > 0f && Ctx.IsDashPressed)
        {
            Ctx.DashGas = Mathf.Max(0f, Ctx.DashGas - Ctx.DashConsumptionSpeed * Time.deltaTime);
            Ctx.AppliedMovementX = Ctx.CurrentMovementInput.x * Ctx.DashThrust;
            Ctx.AppliedMovementZ = Ctx.CurrentMovementInput.y * Ctx.DashThrust;
        }
        else if (Ctx.DashGas <= 0)
        {
            Ctx.AppliedMovementX = Ctx.CurrentMovementInput.x * Ctx.RunMultiplier;
            Ctx.AppliedMovementZ = Ctx.CurrentMovementInput.y * Ctx.RunMultiplier;
            Ctx.Animator.SetBool(Ctx.IsDashingHash, false);
            Ctx.Animator.SetBool(Ctx.IsRunningHash, true);
        }
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsDashingHash, false);
    }

    public override void CheckSwitchStates()
    {
        if (!Ctx.IsMovementPressed && !Ctx.IsDashPressed)
        {
            SwitchState(Factory.Idle());
        }
        else if (Ctx.IsMovementPressed && !Ctx.IsDashPressed)
        {
            SwitchState(Factory.Run());
        }
    }

    public override void InitialiseSubState()
    {

    }
}