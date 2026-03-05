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
        // rangerContext.PickShootPoints();
        rangerContext.BeginSurvey();
    }

    public override void Execute()
    {
       // rangerContext.RequestState(rangerContext.MoveState);
       
       if (!rangerContext.SurveyFinished())
           return;
       if (rangerContext.TryPickNextShootPoint())
           rangerContext.RequestState(rangerContext.MoveState);
    }

    public override void Exit()
    {
    }
}
