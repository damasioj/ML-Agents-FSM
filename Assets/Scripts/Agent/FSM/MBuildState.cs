using System;

/// <summary>
/// This is not used as our environment doesn't simulate the building action.
/// </summary>
public class MBuildState : ManualState
{
    private Action actionToExecute;
    private float counter = 0f;
    private float actionDuration = 0f;

    public override bool IsFinished { get; protected set; }

    public MBuildState(ManualAgent owner)
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
        IsFinished = true;
    }

    public override void OnExit()
    {
        return;
    }

    public override void OnFixedUpdate()
    {
        return;
    }

    public override void OnUpdate()
    {
        return;
    }
}
