using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FSM {

	// Current state
	public FSMState current;

	public FSM(FSMState state) {
		current = state;
		current.Enter();
	}

	// Examine transitions leading out from the current state
	// If a condition is activated, then:
	// (1) Execute actions associated to exit from the current state
	// (2) Execute actions associated to the firing transition
	// (3) Retrieve the new state and set is as the current one
	// (4) Execute actions associated to entering the new current state
	// Otherwise, if no condition is activated,
	// (5) Execute actions associated to staying into the current state

	/*

	public void Update() { // NOTE: this is NOT a MonoBehaviour
		FSMTransition transition = current.VerifyTransitions ();
		if (transition != null) {
			current.Exit();		// 1
			transition.Fire();	// 2
			current = current.NextState(transition);	// 3
			current.Enter();	// 4
		} else {
			current.Stay();		// 5
		}
	}

	*/

	// This is the new Update method, using the decision tree
	public void Update() {
		//Debug.Log("Current state: " + current.stateName + "");
		//DecisionTree transition = current.VerifyTransitions ();
		FSMState transition = current.transionTree.walk();
		if (transition != null) {
			current.Exit();		// 1
			//transition.Fire();	// 2
			//current = current.NextState(transition);	// 3
			current = transition;
			current.Enter();	// 4
			Debug.Log("New state: " + current.stateName + "");
		} else {
			current.Stay();		// 5
		}
	}
}