using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    /// <summary>
    /// The list of outgoing transitions this state can evaluate
    /// </summary>
    private List<Transition> transitions;

    public State()
    {
        transitions = new List<Transition>();
    }

    // This method represents the update cycle of the FSM
    // DO NOT override this method
    public State Update()
    {
        OnUpdate();

        // Check every transition to see if the machine has to move to another state
        // If every transition fails, return to this state
        foreach (Transition t in transitions)
        {
            if (t.evaluate != null && t.evaluate())
            {
                this.OnExit();
                t.state.OnEnter();
                return t.state;
            }
        }
        return this;
    }

    #region virtual and abstract methods of State

    /// <summary>
    /// Start logic of the state
    /// Override this method if special action is required when this state enters
    /// </summary>
    public virtual void OnEnter()
    {

    }

    /// <summary>
    /// The main lifecycle of the state
    /// Override this to ensure the controlled Enemy has a behaviour during this state
    /// </summary>
    public abstract void OnUpdate();

    /// <summary>
    /// Exit logic of the stat
    /// Override this method if special action is required when this state exits
    /// </summary>
    public virtual void OnExit()
    {

    }

    #endregion

    public void AddTransition(Transition t)
    {
        transitions.Add(t);
    }

    /// <summary>
    /// Nested class Transition 
    /// provides functionality required for the FSM to change states according to custom functions per state
    /// </summary>
    public class Transition
    {
        public Transition(Func<bool> evaluate, State state)
        {
            this.evaluate = evaluate;
            this.state = state;
        }

        public State state;
        public Func<bool> evaluate;
    }
}
