using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defer function to perform action
public delegate void FAMAction();

public class FAMState {
	public MonsterState stateName;

	// Arrays of actions to perform based on transitions fire (or not)
	public List<FAMAction> enterActions = new List<FAMAction> ();
	public List<FAMAction> stayActions = new List<FAMAction> ();
	public List<FAMAction> exitActions = new List<FAMAction> ();

	public FAMState(MonsterState name) {
		stateName = name;
	}

	// These methods will perform the actions in each list
	public void Enter() { foreach (FAMAction a in enterActions) a(); }
	public void Stay() { foreach (FAMAction a in stayActions) a(); }
	public void Exit() { foreach (FAMAction a in exitActions) a(); }
}
