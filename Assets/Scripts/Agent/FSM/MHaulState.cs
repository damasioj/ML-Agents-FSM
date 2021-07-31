using System;

/// <summary>
/// State represents a high-level task with possibly multiple low-level actions.
/// </summary>
public class MHaulState : ManualState
{
    new private BaseStructure Target { get; set; }

    public override bool IsFinished { get; protected set; }

    public MHaulState(ManualAgent owner)
        : base(owner) { }

    // not used in this state type
    public override void SetAction(Action action, float duration = 0f)
    {
        return;
    }

    public override void SetTarget(BaseTarget target)
    {
        Target = target as BaseStructure;
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
            if (!IsFinished)
            {
                IsFinished = true;
                Target.AddResource(ref Owner.resource);
                Owner.CurrentState = AgentStateType.Idle;
                Finished();
                return;
            }
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
