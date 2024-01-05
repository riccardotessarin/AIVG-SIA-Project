using System;
using System.Collections.Generic;

// Interface for both decisions and actions
// Any node belonging to the decision tree must be walkable
public interface IDTNode {
	//DTAction Walk();
	FSMState RecursiveWalk();
	IDTNode Walk();
}

// This delegate will defer functions to both
// making decisions and performing actions
public delegate object DTCall(object bundle);

// Decision node
public class DTDecision : IDTNode {

	// The method to call to take the decision
	private DTCall Selector;

	// The return value of the decision is checked against
	// a dictionary and the corresponding link is followed
	private Dictionary<object, IDTNode> links;

	public DTDecision(DTCall selector) {
		Selector = selector;
		links = new Dictionary<object, IDTNode>();
	}

	// Add an entry in the dictionary linking a possible output
	// of Selector to a node
	public void AddLink(object value, IDTNode next) {
		links.Add(value, next);
	}

	// Recursive version of Walk
	// We call the selector and check if there is a matching link
	// for the return value. In such case, we walk on the link
	// No link means no state and null is returned
	public FSMState RecursiveWalk() {
		object o = Selector(null);
		return links.ContainsKey(o) ? links[o].RecursiveWalk() : null;
	}

	// Non-recursive version of the tree walk
	// We walk the tree while we find a decision node
	// We return a leaf node or null. If we return a leaf node, it means we found a state
	// If the decision doesn't have a link for the value returned by the selector, we return null
	// The FSM Update will just label it as "stay in the current state" since the return value is not a state
	public IDTNode Walk() {
		IDTNode current = this;
		while (current is DTDecision decision) {
			DTDecision d = decision;
			object o = d.Selector(null);
			if (!d.links.ContainsKey(o)) return null;
			current = d.links[o];
		}
		return current;
	}

}

// This class is holding our decision structure
public class DecisionTree {

	private IDTNode root;

	// Create a decision tree with starting from a root node
	public DecisionTree(IDTNode start) {
		root = start;
	}

	// Walk the structure and return the leaf state if we find one
	// If we don't find a leaf, we return null
	public FSMState walk() {
		if (root.Walk() is FSMState result) {
			return result;
		}
		return null;
	}
}