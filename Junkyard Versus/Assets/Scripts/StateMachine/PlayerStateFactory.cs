public class PlayerStateFactory
{
    PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    public PlayerBaseState Idle() 
    {
        return new PlayerIdleState(_context, this);
    }
    public PlayerBaseState Run() 
    {
        return new PlayerRunState(_context, this);
    }
    public PlayerBaseState Jump() 
    {
        return new PlayerJumpState(_context, this);
    }
    public PlayerBaseState Grounded() 
    {
        return new PlayerGroundedState(_context, this);
    }
    public PlayerBaseState Fall()
    {
        return new PlayerFallState(_context, this);
    }

    public PlayerBaseState Jet()
    {
        return new PlayerJetState(_context, this);
    }

    public PlayerBaseState Dash()
    {
        return new PlayerDashState(_context, this);
    }
}
