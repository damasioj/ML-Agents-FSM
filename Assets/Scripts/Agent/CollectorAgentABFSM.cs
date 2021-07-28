using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Represents a collector agent that an action-based FSM architecture.
/// No RL is used for this agent.
/// </summary>
public class CollectorAgentABFSM : ManualAgent, IHasGoal
{
    static readonly ProfilerMarker s_ExecutePerfMarker = new ProfilerMarker("ActionBasedFSM.ExecuteAction");
    static readonly ProfilerMarker s_SetPerfMarker = new ProfilerMarker("ActionBasedFSM.SetAction");
    static readonly ProfilerMarker s_OverallPerfMarker = new ProfilerMarker("ActionBasedFSM.Overall");

    private BaseResource resource;
    private bool HasResource => resource is object;
    private bool IsAtSource { get; set; }
    private bool IsAtGoal { get; set; }
    new public BaseSource Target { get; set; }
    public BaseStructure Goal { get; private set; }

    protected override void OnStart()
    {
        AssignStateDictionary();
    }

    protected override void OnUpdate()
    {
        // ..
    }

    protected override void OnFixedUpdate()
    {
        s_OverallPerfMarker.Begin();
        DoAction();
        s_OverallPerfMarker.End();
    }

    protected override void AssignStateDictionary()
    {
        StateDictionary = new Dictionary<AgentStateType, ManualState>()
        {
            [AgentStateType.Idle] = new MIdleState(this),
            [AgentStateType.Move] = new MMoveState(this),
            [AgentStateType.Interact] = new MInteractState(this)
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "goal":
                IsAtGoal = true;
                break;
            case "target":
                if (other.GetComponent<BaseSource>().Equals(Target))
                {
                    IsAtSource = true;
                }
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("target"))
        {
            IsAtSource = false;
        }
        else if (other.CompareTag("goal"))
        {
            IsAtGoal = false;
        }
    }

    //public override void CollectObservations(VectorSensor sensor)
    //{
    //    if (Target is object)
    //    {
    //        // target location
    //        sensor.AddObservation(Target.Location.x); //1
    //        sensor.AddObservation(Target.Location.z); //1

    //        // goal info
    //        sensor.AddObservation(Goal.Location.x); //1
    //        sensor.AddObservation(Goal.Location.z); //1

    //        // Agent data
    //        sensor.AddObservation(HasResource); //1
    //        sensor.AddObservation(transform.localPosition.x); //1
    //        sensor.AddObservation(transform.localPosition.z); //1
    //        sensor.AddObservation(Body.velocity.x); //1
    //        sensor.AddObservation(Body.velocity.z); //1
    //        sensor.AddObservation((int)CurrentState); // 1
    //        sensor.AddObservation(IsAtResource); // 1
    //    }
    //}

    //public override void OnActionReceived(float[] vectorAction)
    //{
    //    //TODO : refactor ... maybe use events / delegates?
    //    if (StateDictionary[CurrentState].IsFinished)
    //    {
    //        CollectResource();
    //    }

    //    if (StateDictionary[CurrentState].IsFinished)
    //    {
    //        Move(vectorAction);
    //    }
    //}

    protected void DoAction()
    {
        if (StateDictionary[CurrentState].IsFinished)
        {
            s_SetPerfMarker.Begin();

            if (HasResource && !IsAtGoal)
            {
                CurrentState = AgentStateType.Move;
            }
            else if (!IsAtSource && !HasResource) // not at target, doesnt have resource
            {
                // if no targets available, make idle and keep checking job status
                if (Target.ResourceCount == 0)
                {
                    CurrentState = AgentStateType.Idle;
                    ValidateJobComplete();
                }
                else // otherwise set move to target
                {
                    CurrentState = AgentStateType.Move;
                }
            }
            else if (IsAtSource && !HasResource) // at target, collect resource
            {
                CollectResource();
            }
            else if (HasResource && IsAtGoal) // has resource, at goal
            {
                // drop resources
                Goal.AddResource(ref resource);
                ValidateJobComplete();
                ValidateGoalComplete();
                InternalStepCount = 0;
            }

            s_SetPerfMarker.End();
        }
        else // continue current action
        {
            s_ExecutePerfMarker.Begin();
            StateDictionary[CurrentState].OnFixedUpdate();
            s_ExecutePerfMarker.End();
        }
    }

    /// <summary>
    /// In a normal scenario, this logic would be delegated to a service.
    /// </summary>
    private void CollectResource()
    {
        InternalStepCount = 0;
        CurrentState = AgentStateType.Interact;
        StateDictionary[CurrentState].SetAction(TakeResource, 50f);
    }


    private void TakeResource()
    {
        if (!HasResource)
        {
            resource = Target.TakeResource();

            if (resource is object)
            {
                InternalStepCount = 0;
            }
        }
    }

    /// <summary>
    /// Checks if the goal has the required number of resources from current target.
    /// </summary>
    protected void ValidateJobComplete()
    {
        var isResourceRequired = Goal.GetResourcesRequired().Any(g => g.Key == Target.GetResourceType() && g.Value > 0);

        if (!isResourceRequired)
        {
            OnTaskDone();
        }
    }

    protected void ValidateGoalComplete()
    {
        if (Goal.IsComplete)
        {
            Debug.Log("COLLECTOR :: Job complete.");
        }
    }

    public override bool IsAtDestination()
    {
        if ((!HasResource && !IsAtSource)
            || (HasResource && !IsAtGoal))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override Vector3 GetDestination()
    {
        if (HasResource)
        {
            return Goal.Location;
        }
        else
        {
            return Target.Location;
        }
    }

    public override void UpdateTarget(IEnumerable<BaseTarget> baseTargets)
    {
        // updates with a valid target that contains a resource required by the goal
        var resourceTypes = Goal.GetResourcesRequired().Where(g => g.Value > 0).Select(g => g.Key);
        Target = baseTargets.FirstOrDefault(t => t.IsValid
                                            && t is BaseSource source
                                            && resourceTypes.Contains(source.GetResourceType())) as BaseSource;

        if (Target == null)
        {
            Debug.Log("COLLECTOR :: No targets.");
        }
    }

    public void UpdateGoal(IEnumerable<BaseStructure> baseStructures)
    {
        if (baseStructures.Count() > 0)
        {
            Goal = baseStructures.First();
        }
        else
        {
            Debug.Log("COLLECTOR :: No new goals.");
        }
    }
}
