using System;

/// <summary>
/// State represents a high-level task with possibly multiple low-level actions.
/// </summary>
public class MGatherState : ManualState
{
    new private BaseSource Target { get; set; }
    public override bool IsFinished { get; protected set; }
    private float counter = 0;
    private float actionDuration = 50; // this value is normally taken from the target

    public MGatherState(ManualAgent owner)
        : base(owner) { }

    public override void SetAction(Action action, float duration = 0f)
    {
        return;
    }

    public override void SetTarget(BaseTarget target)
    {
        Target = target as BaseSource;
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
        counter = 0;
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
                Owner.resource = Target.TakeResource();
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
