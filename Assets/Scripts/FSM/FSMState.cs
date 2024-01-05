using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defer function to perform action
public delegate void FSMAction();

// FSMState implements the IDTNode interface to be returned as a leaf of the decision tree
public class FSMState: IDTNode {

	public MonsterState stateName;

	// Arrays of actions to perform based on transitions fire (or not)
	// Getters and setters are preferable, but we want to keep the source clean
	public List<FSMAction> enterActions = new List<FSMAction> ();
	public List<FSMAction> stayActions = new List<FSMAction> ();
	public List<FSMAction> exitActions = new List<FSMAction> ();

	// A decision tree to evaluate transitions
	public DecisionTree transionTree;

	public FSMState(MonsterState name) {
		stateName = name;
	}

	// We link a decision tree to the state
	public void AddTransition(DecisionTree transition) {
		transionTree = transition;
	}
	
	// These methods will perform the actions in each list
	public void Enter() { foreach (FSMAction a in enterActions) a(); }
	public void Stay() { foreach (FSMAction a in stayActions) a(); }
	public void Exit() { foreach (FSMAction a in exitActions) a(); }

	// FSMState is a leaf of the decision tree and will always return itself
	public IDTNode Walk()
	{
		return this;
	}
}