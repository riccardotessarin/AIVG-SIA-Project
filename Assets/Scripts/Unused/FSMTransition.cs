using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*

// Defer function to trigger activation condition
// Returns true when transition can fire
public delegate bool FSMCondition();

public class FSMTransition {

	// The method to evaluate if the transition is ready to fire
	public FSMCondition myCondition;

	// A list of actions to perform when this transition fires
	private List<FSMAction> myActions = new List<FSMAction>();

	public FSMTransition(FSMCondition condition, FSMAction[] actions = null) {
		myCondition = condition;
		if (actions != null) myActions.AddRange(actions);
	}

	// Call all  actions
	public void Fire() {
		foreach (FSMAction action in myActions) action();
	}
}

*/