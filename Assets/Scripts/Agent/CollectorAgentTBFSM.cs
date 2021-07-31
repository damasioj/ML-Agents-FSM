using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Represents a collector agent that uses a task-based FSM architecture.
/// No RL is used in for this agent.
/// </summary>
public class CollectorAgentTBFSM : ManualAgent, IHasGoal
{
    static readonly ProfilerMarker s_ExecutePerfMarker = new ProfilerMarker("TaskBasedFSM.ExecuteAction");
    static readonly ProfilerMarker s_SetPerfMarker = new ProfilerMarker("TaskBasedFSM.SetAction");
    static readonly ProfilerMarker s_OverallPerfMarker = new ProfilerMarker("TaskBasedFSM.Overall");

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
            [AgentStateType.Gather] = new MGatherState(this),
            [AgentStateType.Haul] = new MHaulState(this),
            [AgentStateType.Build] = new MBuildState(this)
        };

        StateDictionary[AgentStateType.Haul].FinishEvent += OnHaulComplete;
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

    protected void DoAction()
    {
        if (StateDictionary[CurrentState].IsFinished)
        {
            s_SetPerfMarker.Begin();

            if (HasResource && !IsAtGoal) // has resource, not at goal
            {
                SetHaulState();
            }
            else if (!IsAtSource && !HasResource) // not at target, doesnt have resource
            {
                // if no targets available, make idle and keep checking job status
                if (Target.ResourceCount == 0)
                {
                    CurrentState = AgentStateType.Idle;
                    ValidateJobComplete();
                }
                else // otherwise set to gather
                {
                    SetGatherState();
                }
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
    private void SetGatherState()
    {
        CurrentState = AgentStateType.Gather;
        StateDictionary[CurrentState].SetTarget(Target);
    }

    /// <summary>
    /// In a normal scenario, this logic would be delegated to a service.
    /// </summary>
    private void SetHaulState()
    {
        CurrentState = AgentStateType.Haul;
        StateDictionary[CurrentState].SetTarget(Goal);
    }

    protected void OnHaulComplete(object sender, EventArgs e)
    {
        ValidateJobComplete();
        ValidateGoalComplete();
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
            CurrentState = AgentStateType.Build;
            // other logic ..
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
    }

    public void UpdateGoal(IEnumerable<BaseStructure> baseStructures)
    {
        if (baseStructures.Count() > 0)
        {
            Goal = baseStructures.First();
        }
    }
}
