using System;
using UnityEngine;

public class MoveState : AgentState
{
    private Vector3 _lastPosition = Vector3.zero;
    public override bool IsFinished { get; protected set; }

    public MoveState(BasicAgent owner)
        : base(owner) { }

    public override void DoAction()
    {
        return;
    }

    public override void DoAction(float[] vectorAction)
    {
        var rBody = Owner.GetComponent<Rigidbody>();
        var scale = Owner.gameObject.transform.localScale.x;

        if (rBody is object)
        {
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = vectorAction[0];
            controlSignal.z = vectorAction[1];

            rBody.AddForce(new Vector3(controlSignal.x * Owner.acceleration * scale, 0, controlSignal.z * Owner.acceleration * scale));
        }

        SetDirection();
        _lastPosition = Owner.transform.position;

        IsFinished = true;
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
        return;
    }

    public override void OnUpdate()
    {
        return;
    }

    public override void SetAction(Action action)
    {
        return;
    }

    private void SetDirection()
    {
        var direction = (Owner.transform.position - _lastPosition).normalized;

        if (Owner.transform.rotation != Quaternion.LookRotation(direction))
        {
            Owner.transform.rotation = Quaternion.Slerp(Owner.transform.rotation, Quaternion.LookRotation(direction), 0.08F);
        }
    }
}
