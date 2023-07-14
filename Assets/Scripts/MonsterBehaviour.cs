using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{

	private float health = 100f;
	private float hunger = 0f;
	private float sleepiness = 0f;
	private float stress = 0f;
	private float grudge = 0f;

	private FSM monsterFSM;

	private DecisionTree calmDT;
	private DecisionTree annoyedDT;
	private DecisionTree replenishDT;
	private DecisionTree angryDT;
	private DecisionTree berserkDT;

	// Start is called before the first frame update
	void Start()
	{
		FSMState calmState = new FSMState();
		FSMState annoyedState = new FSMState();
		FSMState replenishState = new FSMState();
		FSMState angryState = new FSMState();
		FSMState berserkState = new FSMState();

		// Define DT transitions for each state
		// From calm state
		DTDecision calmd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision calmd2 = new DTDecision(GoodMentalStatus);
		DTDecision calmd3 = new DTDecision(IsAngry);

		// Define links between decisions
		calmd1.AddLink(true, calmd2);
		calmd1.AddLink(false, replenishState);
		calmd2.AddLink(true, calmState);
		calmd2.AddLink(false, calmd3);
		calmd3.AddLink(true, angryState);
		calmd3.AddLink(false, annoyedState);

		// From annoyed state
		DTDecision annoyedd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision annoyedd2 = new DTDecision(GoodMentalStatus);
		DTDecision annoyedd3 = new DTDecision(IsAngry);

		// Define links between decisions
		annoyedd1.AddLink(true, annoyedd2);
		annoyedd1.AddLink(false, replenishState);
		annoyedd2.AddLink(true, calmState);
		annoyedd2.AddLink(false, annoyedd3);
		annoyedd3.AddLink(true, angryState);
		annoyedd3.AddLink(false, annoyedState);

		// From replenish state
		DTDecision replenishd1 = new DTDecision(GoodMentalStatus);
		DTDecision replenishd2 = new DTDecision(GoodPhysicalStatus);
		DTDecision replenishd2bis = new DTDecision(GoodPhysicalStatus);
		DTDecision replenishd3 = new DTDecision(IsAngry);
		DTDecision replenishd4 = new DTDecision(PlayerInRange);

		// Define links between decisions
		replenishd1.AddLink(true, replenishd2);
		replenishd1.AddLink(false, replenishd2bis);
		replenishd2.AddLink(true, calmState);
		replenishd2.AddLink(false, replenishState);
		replenishd2bis.AddLink(true, replenishd3);
		replenishd2bis.AddLink(false, replenishd4);
		replenishd3.AddLink(true, angryState);
		replenishd3.AddLink(false, annoyedState);
		replenishd4.AddLink(true, berserkState);
		replenishd4.AddLink(false, replenishState);


		// From angry state
		DTDecision angryd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision angryd2 = new DTDecision(GoodMentalStatus);
		DTDecision angryd3 = new DTDecision(PlayerInRange);

		// Define links between decisions
		angryd1.AddLink(true, angryd2);
		angryd1.AddLink(false, angryd3);
		angryd2.AddLink(true, calmState);
		angryd2.AddLink(false, angryState);
		angryd3.AddLink(true, berserkState);
		angryd3.AddLink(false, replenishState);

		// From berserk state
		//DTDecision berserkd2 = new DTDecision(GoodPhysicalStatus);
		DTDecision berserkd1 = new DTDecision(GoodMentalStatus);
		//DTDecision berserkd3 = new DTDecision(PlayerInRange);

		// Define links between decisions
		berserkd1.AddLink(true, replenishState);
		berserkd1.AddLink(false, berserkState);

		// Setup my DecisionTree at the root node
		calmDT = new DecisionTree(calmd1);
		annoyedDT = new DecisionTree(annoyedd1);
		replenishDT = new DecisionTree(replenishd1);
		angryDT = new DecisionTree(angryd1);
		berserkDT = new DecisionTree(berserkd1);

		// Define transitions by linking a decision tree to each state
		calmState.AddTransition(calmDT);
		annoyedState.AddTransition(annoyedDT);
		replenishState.AddTransition(replenishDT);
		angryState.AddTransition(angryDT);
		berserkState.AddTransition(berserkDT);

	}


	// Update is called once per frame
	void Update()
	{
		
	}
	
	
	// Decisions

	private object GoodPhysicalStatus(object o) {
		throw new NotImplementedException();
	}

	private object GoodMentalStatus(object o) {
		return false;
	}

	private object PlayerInRange(object o) {
		throw new NotImplementedException();
	}

	private object IsAngry(object o) {
		throw new NotImplementedException();
	}
	
}
