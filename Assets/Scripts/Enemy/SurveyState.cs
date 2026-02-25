using UnityEngine;

public class SurveyState : State
{
    private DefaultRangerContext rangerContext;
    
    public SurveyState(DefaultRangerContext context)
    {
        rangerContext = context;
    }
    public override void Enter()
    {
        rangerContext.PickShootPoints();
    }

    public override void Execute()
    {
        rangerContext.RequestState(rangerContext.MoveState);
    }

    public override void Exit()
    {
    }
}
