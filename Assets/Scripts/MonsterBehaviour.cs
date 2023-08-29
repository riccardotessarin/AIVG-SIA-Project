using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MonsterBehaviour : MonoBehaviour
{

	public GameObject player;

	Rigidbody rb;

	private float reactionTime = 1.5f;
	
	private float roamSpeed = 1f;
	private float fleeSpeed = 3f;
	private float chaseSpeed = 3f;
	private float berserkSpeed = 5f;
	private float currentSpeed;
	private float stopAt;
	private Vector3 targetRandomPosition;
	private float time = 0f;
	private float timeToTarget = 5f;

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

	private bool isRoaming = false;
	private bool isReplenishing = false;
	private bool isHealing = false;
	private bool isSleeping = false;
	private bool isEating = false;
	private bool isFleeing = false;
	private bool isChasing = false;
	private bool isChasingBerserk = false;
	private bool isAttacking = false;

	private enum MonsterState { calm, annoyed, replenish, angry, berserk };
	private MonsterState currentState;
	
	private void Awake() {
		currentState = MonsterState.calm;
		currentSpeed = roamSpeed;
		rb = GetComponent<Rigidbody>();
		stopAt = 0.1f;
		targetRandomPosition = new Vector3(20, 1, 20);
	}

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

		// Define actions for each state
		calmState.enterActions.Add(StartRoaming);
		//calmState.stayActions.Add(Roam);
		calmState.exitActions.Add(StopRoaming);
		annoyedState.enterActions.Add(StartFleeing);
		annoyedState.stayActions.Add(Flee);
		annoyedState.exitActions.Add(StopFleeing);
		replenishState.enterActions.Add(StartReplenishing);
		replenishState.stayActions.Add(Replenish);
		replenishState.exitActions.Add(StopReplenishing);
		angryState.enterActions.Add(StartChaseToHit);
		angryState.stayActions.Add(ChaseToHit);
		angryState.exitActions.Add(StopChaseToHit);
		berserkState.enterActions.Add(StartChaseToKill);
		berserkState.stayActions.Add(ChaseToKill);
		berserkState.exitActions.Add(StopChaseToKill);


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
	
	
	private void FixedUpdate() {
		Roam();
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
	
	// Actions

	public void StartRoaming() {
		isRoaming = true;
		currentSpeed = roamSpeed;
		currentState = MonsterState.calm;
	}

	// Function for free map roaming
	public void Roam() {
		Debug.Log("Roaming to destination...");

		Vector3 verticalAdj = new Vector3(targetRandomPosition.x, transform.position.y, targetRandomPosition.z);
		Vector3 toDestination = verticalAdj - transform.position;

		if (stopAt > toDestination.magnitude || time > timeToTarget) {
			targetRandomPosition = UnityEngine.Random.onUnitSphere*UnityEngine.Random.Range(10f, 20f);
			time = 0f;
		} else {
			// we keep only option a
			transform.LookAt(verticalAdj);
			rb.MovePosition(rb.position + transform.forward * currentSpeed * Time.deltaTime);
			time += Time.deltaTime;
		}
	}

	public void StopRoaming() {
		isRoaming = false;
	}

	public void StartFleeing() {
		isFleeing = true;
		currentSpeed = fleeSpeed;
		currentState = MonsterState.annoyed;
	}

	public void Flee() {
		//Debug.Log("Chasing");
	}

	public void StopFleeing() {
		isFleeing = false;
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartChaseToHit() {
		isChasing = true;
		currentSpeed = chaseSpeed;
		currentState = MonsterState.angry;
	}

	public void ChaseToHit() {
		//Debug.Log("Chasing");
	}

	public void StopChaseToHit() {
		isChasing = false;
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartChaseToKill() {
		isChasingBerserk = true;
		currentSpeed = berserkSpeed;
		currentState = MonsterState.berserk;
	}

	public void ChaseToKill() {
		//Debug.Log("Chasing");
	}

	public void StopChaseToKill() {
		isChasingBerserk = false;
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartReplenishing() {
		isReplenishing = true;
		currentSpeed = roamSpeed;
		currentState = MonsterState.replenish;
	}

	public void Replenish() {
		//Debug.Log("Replenishing");
	}

	public void StopReplenishing() {
		if (isReplenishing) {
			isReplenishing = false;
		}
		if (isHealing || isEating || isSleeping) {
			isHealing = false;
			isEating = false;
			isSleeping = false;
		}
	}
}
