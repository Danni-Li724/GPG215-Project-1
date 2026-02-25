using UnityEngine;

public class MoveToPointState : State
{
    private DefaultRangerContext rangerContext;
    
    public MoveToPointState(DefaultRangerContext context)
    {
        rangerContext = context;
    }
    public override void Enter()
    {
    }

    public override void Execute()
    {
        rangerContext.MoveTowardsTarget(rangerContext.MoveSpeed);

        if (rangerContext.HasArrived())
            rangerContext.RequestState(rangerContext.ShootState);
    }

    public override void Exit()
    {
    }
}
