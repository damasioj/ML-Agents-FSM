using System;

public class InteractState : AgentState
{
    private Action actionToExecute;
    private float counter = 0f;

    public override bool IsFinished { get; protected set; }

    public InteractState(BasicAgent owner)
        : base(owner) { }

    public override void SetAction(Action action)
    {
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
        counter = Owner.StepCount;
        // todo : start animation
    }

    public override void OnExit()
    {
        return;
    }

    public override void OnFixedUpdate()
    {
        if (Owner.StepCount - counter >= 50)
        {
            actionToExecute();
            IsFinished = true;
            Owner.CurrentState = AgentStateType.Idle;
        };
    }

    public override void OnUpdate()
    {
        return;
    }
}
