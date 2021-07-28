using System;

public class MGatherState : ManualState
{
    private Action actionToExecute;
    private float counter = 0f;
    private float actionDuration = 0f;

    public override bool IsFinished { get; protected set; }

    public MGatherState(ManualAgent owner)
        : base(owner) { }

    public override void SetAction(Action action, float duration = 0f)
    {
        actionDuration = duration;
        counter = 0f;
        actionToExecute = action;
    }

    public override void DoAction()
    {
        return;
    }

    public override void DoAction(float[] vectorActions)
    {
        return;
    }

    public override void OnEnter()
    {
        IsFinished = false;
    }

    public override void OnExit()
    {
        return;
    }

    public override void OnFixedUpdate()
    {
        if (Owner.IsAtDestination())
        {
            if (!IsFinished && counter >= actionDuration)
            {
                actionToExecute();
                IsFinished = true;
                Owner.CurrentState = AgentStateType.Idle;
                return;
            }

            counter++;
        }
        else
        {
            float[] movement = BasicPathfinder.GetDirection(Owner.Body.transform.localPosition, Owner.GetDestination());
            Move(movement);
        }
    }

    public override void OnUpdate()
    {
        return;
    }
}
