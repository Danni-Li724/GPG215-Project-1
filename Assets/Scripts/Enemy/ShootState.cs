using UnityEngine;

public class ShootState : State
{
    private DefaultRangerContext rangerContext;
    
    public ShootState(DefaultRangerContext context)
    {
        rangerContext = context;
    }
    public override void Enter()
    {
        rangerContext.ResetBurst();
    }

    public override void Execute()
    {
        rangerContext.FireContinuously();

        if (rangerContext.ShootingCompleted())
            rangerContext.FinishShootWaveAndRequestNext();
    }

    public override void Exit()
    {
    }
}
