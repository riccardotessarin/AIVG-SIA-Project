using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{

	public GameObject player;

	private float reactionTime = 1.5f;

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
		FSMState calmState = new FSMState("calm");
		FSMState annoyedState = new FSMState("annoyed");
		FSMState replenishState = new FSMState("replenish");
		FSMState angryState = new FSMState("angry");
		FSMState berserkState = new FSMState("berserk");

		// Define DT transitions for each state
		// From calm state
		DTDecision calmd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision calmd2 = new DTDecision(GoodMentalStatus);
		DTDecision calmd3 = new DTDecision(IsAngry);

		// Define links between decisions
		calmd1.AddLink(true, calmd2);
		calmd1.AddLink(false, replenishState);
		//calmd2.AddLink(true, calmState); // NOTE: if we want to stay in the same state, we don't need to add a link
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
		//annoyedd3.AddLink(false, annoyedState); // NOTE: if we want to stay in the same state, we don't need to add a link

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
		//replenishd2.AddLink(false, replenishState); // NOTE: if we want to stay in the same state, we don't need to add a link
		replenishd2bis.AddLink(true, replenishd3);
		replenishd2bis.AddLink(false, replenishd4);
		replenishd3.AddLink(true, angryState);
		replenishd3.AddLink(false, annoyedState);
		replenishd4.AddLink(true, berserkState);
		//replenishd4.AddLink(false, replenishState); // NOTE: if we want to stay in the same state, we don't need to add a link


		// From angry state
		DTDecision angryd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision angryd2 = new DTDecision(GoodMentalStatus);
		DTDecision angryd3 = new DTDecision(PlayerInRange);

		// Define links between decisions
		angryd1.AddLink(true, angryd2);
		angryd1.AddLink(false, angryd3);
		angryd2.AddLink(true, calmState);
		//angryd2.AddLink(false, angryState); // NOTE: if we want to stay in the same state, we don't need to add a link
		angryd3.AddLink(true, berserkState);
		angryd3.AddLink(false, replenishState);

		// From berserk state
		//DTDecision berserkd2 = new DTDecision(GoodPhysicalStatus);
		DTDecision berserkd1 = new DTDecision(GoodMentalStatus);
		//DTDecision berserkd3 = new DTDecision(PlayerInRange);

		// Define links between decisions
		berserkd1.AddLink(true, replenishState);
		//berserkd1.AddLink(false, berserkState); // NOTE: if we want to stay in the same state, we don't need to add a link

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

		monsterFSM = new FSM(calmState);

		StartCoroutine(UpdateFSM());
	}

	public IEnumerator UpdateFSM() {
		while (true) {
			monsterFSM.Update();
			yield return new WaitForSeconds(reactionTime);
		}
	}


	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return)) {
			Debug.Log("Current stats: " + health + " " + hunger + " " + sleepiness + " " + stress + " " + grudge);
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			health = 100f;
			hunger = 0f;
			sleepiness = 0f;
			stress = 0f;
			grudge = 0f;
		}
		if (Input.GetKeyDown(KeyCode.H)) {
			health -= 20f;
			Debug.Log("Health: " + health);
		}
		if (Input.GetKeyDown(KeyCode.S)) {
			stress += 20f;
			Debug.Log("Stress: " + stress);
		}
		if (Input.GetKeyDown(KeyCode.G)) {
			grudge += 20f;
			Debug.Log("Grudge: " + grudge);
		}
	}
	
	
	// Decisions

	private object GoodPhysicalStatus(object o) {
		if (health > 20f && hunger < 80f && sleepiness < 80f) {
			return true;
		}
		return false;
	}

	private object GoodMentalStatus(object o) {
		if (stress + grudge < 50f) {
			return true;
		}
		return false;
	}

	private object PlayerInRange(object o) {
		if (Vector3.Distance(transform.position, player.transform.position) < 10f) {
			return true;
		}
		return false;
	}

	private object IsAngry(object o) {
		if (stress + grudge > 100f) {
			return true;
		}
		return false;
	}
	
}
