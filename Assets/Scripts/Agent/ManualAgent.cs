using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ManualAgent : MonoBehaviour
{
    public event EventHandler TaskDone;

    public int maxInternalSteps;
    public float acceleration;
    public BaseResource resource;

    #region Properties
    public virtual BaseTarget Target { get; set; }
    public Rigidbody Body { get; protected set; }
    protected bool IsDoneCalled { get; set; }
    protected Dictionary<AgentStateType, ManualState> StateDictionary { get; set; }
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
            StateDictionary[currentState].OnExit();
            currentState = value;
            StateDictionary[currentState].OnEnter();
        }
    }
    #endregion

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        StartPosition = transform.position;
        PreviousPosition = StartPosition;
        AssignStateDictionary();
        IsDoneCalled = false;
        OnTaskDone(); // force update of target and goal
        transform.position = StartPosition;
        PreviousPosition = StartPosition;
        CurrentState = AgentStateType.Idle;
    }

    void Start()
    {
        OnStart();
    }

    void Update()
    {
        OnUpdate();
    }

    void FixedUpdate()
    {
        OnFixedUpdate();
    }

    // this is used to replace constructor overloading
    protected abstract void OnStart();
    protected abstract void OnUpdate();
    protected abstract void OnFixedUpdate();

    /// <summary>
    /// The states that the agent has.
    /// </summary>
    protected virtual void AssignStateDictionary()
    {
        StateDictionary = new Dictionary<AgentStateType, ManualState>()
        {
            [AgentStateType.Idle] = new MIdleState(this),
            [AgentStateType.Move] = new MMoveState(this)
        };
    }

    /// <summary>
    /// Signals to the environment that the agent finished its current task.
    /// </summary>
    protected virtual void OnTaskDone()
    {
        TaskDone?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Checks if the agent has reached the target.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsAtDestination();

    /// <summary>
    /// Gets the current destination of the agent.
    /// </summary>
    public virtual Vector3 GetDestination()
    {
        return Target.Location;
    }

    /// <summary>
    /// Makes the agent update their target based on the provided targets.
    /// </summary>
    /// <param name="baseTargets">Targets</param>
    public abstract void UpdateTarget(IEnumerable<BaseTarget> baseTargets);
    
}
