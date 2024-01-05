using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FSM {

	// Current state
	private FSMState current;
	public Dictionary<MonsterState, FSMState> states;

	public FSM(FSMState state) {
		states = new Dictionary<MonsterState, FSMState>();
		current = state;
	}

	public void EnterFirstState() {
		current.Enter();
	}

	// Explicitly set the current state
	// Only used when getting back to the FSM from the FAM
	public void SetCurrentState(MonsterState state) {
		current = states[state];
	}

	// Updates the state of the FSM using the decision tree of the current state
	// It walks the tree until it finds a state
	// If it doesn't find a leaf of type FSMState or the FSMState is the current state, it does nothing
	// If it finds a new state, it exits the current state, sets the new state as the current one and enters it
	public void Update() {
		if (current.transionTree.walk() is FSMState transition && transition != current) {
			current.Exit();
			GameManager.Instance.SetGameMessage("State changed from " + current.stateName + " to " + transition.stateName);
			current = transition;
			current.Enter();
		}
		current.Stay();
	}
}