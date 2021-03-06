using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An extended abstract class based on the Unity.MLAgents.Agent class that provides basic core functions.
/// Parent class for all agents within this project.
/// </summary>
public abstract class BasicAgent : Agent
{
    public event EventHandler TaskDone;

    public int maxInternalSteps;
    public float acceleration;

    #region Properties
    public virtual BaseTarget Target { get; set; }
    public Rigidbody Body { get; protected set; }
    public int InternalStepCount { get; protected set; }
    protected bool IsDoneCalled { get; set; }
    protected Dictionary<AgentStateType, AgentState> StateDictionary { get; set; }
    private Vector3 PreviousPosition { get; set; }
    private Vector3 StartPosition { get; set; }

    private AgentStateType currentState;
    public AgentStateType CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            if (value != currentState)
            {
                StateDictionary[currentState].OnExit();
                currentState = value;
                StateDictionary[currentState].OnEnter();
            }
        }
    }
    #endregion

    public override void Initialize()
    {
        Body = GetComponent<Rigidbody>();
        CurrentState = AgentStateType.Idle;
        StartPosition = transform.position;
        PreviousPosition = StartPosition;
        AssignStateDictionary();
        InternalStepCount = 0;
        IsDoneCalled = false;
        OnTaskDone(); // force update of target and goal
    }

    void Update()
    {
        StateDictionary[CurrentState].OnUpdate();

        if (maxInternalSteps > 0 && StepCount - InternalStepCount > maxInternalSteps && !IsDoneCalled)
        {
            IsDoneCalled = true;
            SubtractReward(0.1f);
            Debug.Log($"Reward: {GetCumulativeReward()}");
            Debug.Log($"No point earned in last {maxInternalSteps} steps. Restarting ...");
            EndEpisode();
        }
    }

    void FixedUpdate()
    {
        StateDictionary[CurrentState].OnFixedUpdate();
    }

    /// <summary>
    /// The states that the agent has.
    /// </summary>
    protected virtual void AssignStateDictionary()
    {
        StateDictionary = new Dictionary<AgentStateType, AgentState>()
        {
            [AgentStateType.Idle] = new IdleState(this),
            [AgentStateType.Move] = new MoveState(this)
        };
    }

    /// <summary>
    /// Signals to the environment that the agent finished its current task.
    /// </summary>
    protected virtual void OnTaskDone()
    {
        TaskDone?.Invoke(this, EventArgs.Empty);
    }

    public override void OnEpisodeBegin()
    {
        SetReward(0f);
        IsDoneCalled = false;
        InternalStepCount = 0;
        Body.angularVelocity = Vector3.zero;
        Body.velocity = Vector3.zero;
        transform.position = StartPosition;
        PreviousPosition = StartPosition;
    }

    protected virtual void Move(float[] vectorAction)
    {
        // agent is idle
        if (vectorAction[0] == 0 && vectorAction[1] == 0)
        {
            CurrentState = AgentStateType.Idle;
            return;
        }

        // agent is moving
        CurrentState = AgentStateType.Move;
        StateDictionary[CurrentState].DoAction(vectorAction);
    }

    protected virtual void SetDirection()
    {
        return;
    }

    public override void Heuristic(float[] actions)
    {
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    protected void SubtractReward(float value)
    {
        AddReward(value * -1);
    }

    /// <summary>
    /// Makes the agent update their target based on the provided targets.
    /// </summary>
    /// <param name="baseTargets">Targets</param>
    public abstract void UpdateTarget(IEnumerable<BaseTarget> baseTargets);
}
