using UnityEngine;

public class LungeState : State
{
    private DefaultRangerContext rangerContext;
    public LungeState(DefaultRangerContext context)
    {
        rangerContext = context;
    }
    public override void Enter()
    {
    }

    public override void Execute()
    {
        rangerContext.LungeTowardsPlayer();
    }

    public override void Exit()
    {
    }
}
