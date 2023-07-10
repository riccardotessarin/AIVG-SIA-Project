using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMState {

	// Arrays of actions to perform based on transitions fire (or not)
	// Getters and setters are preferable, but we want to keep the source clean
	public List<FSMAction> enterActions = new List<FSMAction> ();
	public List<FSMAction> stayActions = new List<FSMAction> ();
	public List<FSMAction> exitActions = new List<FSMAction> ();

	// A dictionary of transitions and the states they are leading to
	private Dictionary<FSMTransition, FSMState> links;

	public FSMState() {
		links = new Dictionary<FSMTransition, FSMState>();
	}

	// We link a decision tree to a transition
	public void AddTransition(FSMTransition transition, FSMState target) {
		links [transition] = target;
	}

	public FSMTransition VerifyTransitions() {
		foreach (FSMTransition t in links.Keys) {
			if (t.myCondition()) return t;
		}
		return null;
	}

	public FSMState NextState(FSMTransition t) {
		return links [t];
	}
	
	// These methods will perform the actions in each list
	public void Enter() { foreach (FSMAction a in enterActions) a(); }
	public void Stay() { foreach (FSMAction a in stayActions) a(); }
	public void Exit() { foreach (FSMAction a in exitActions) a(); }

}