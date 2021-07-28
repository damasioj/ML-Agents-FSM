using System;
using UnityEngine;

/// <summary>
/// This is used by the FSM architecture 1, that was made to be as similar as possible to the RL architecture.
/// A typical FSM architecture may not have a Move state, and instead have states the represent jobs or tasks
/// rather than actions.
/// </summary>
public class MMoveState : ManualState
{
    public override bool IsFinished { get; protected set; }

    public MMoveState(ManualAgent owner)
        : base(owner) { }

    public override void DoAction()
    {
        return;
    }

    public override void DoAction(float[] input)
    {
        var rBody = Owner.GetComponent<Rigidbody>();
        var scale = Owner.gameObject.transform.localScale.x;

        if (rBody is object)
        {
            Vector3 direction = Vector3.zero;
            direction.x = input[0];
            direction.z = input[1];

            rBody.AddForce(new Vector3(direction.x * Owner.acceleration * scale, 0, direction.z * Owner.acceleration * scale));
        }

        SetDirection();
        lastPosition = Owner.transform.position;
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
            IsFinished = true;
        }
        else
        {
            float[] movement = BasicPathfinder.GetDirection(Owner.Body.transform.localPosition, Owner.GetDestination());
            DoAction(movement);
        }
    }

    public override void OnUpdate()
    {
        return;
    }

    public override void SetAction(Action action, float duration = 0f)
    {
        return;
    }
}
