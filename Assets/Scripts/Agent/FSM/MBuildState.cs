using System;

/// <summary>
/// Only for demonstration purposes.
/// This is not used as our environment doesn't simulate the building action.
/// </summary>
public class MBuildState : ManualState
{
    new private BaseStructure Target { get; set; }
    private float actionDuration = 100f; // this info would be taken from the target

    public override bool IsFinished { get; protected set; }

    public MBuildState(ManualAgent owner)
        : base(owner) { }

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
