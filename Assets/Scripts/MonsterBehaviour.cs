using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MonsterBehaviour : MonoBehaviour
{

	public GameObject player;

	private Rigidbody rb;

	private Animator animator;

	
	DragBehaviour dragBehaviour;
	AvoidBehaviourVolume avoidBehaviourVolume;
	SeekBehaviour seekBehaviour;
	//FleeBehaviour fleeBehaviour;
	FreeFleeBehaviour freeFleeBehaviour;
	//FreeRoamingBehaviour freeRoamingBehaviour;
	SeekRestoreBehaviour seekRestoreBehaviour;

	private float reactionTime = 1.5f;
	
	private Dictionary<string, float> monsterSpeed = new Dictionary<string, float> {
		{ "roam", 1f },
		{ "flee", 2.5f },
		{ "chase", 5f },
		{ "berserk", 8f }
	};

	public float minLinearSpeed = 0.5f;
	public float currentSpeed;
	//public float maxLinearSpeed = 5f;
	//public float maxAngularSpeed = 5f;
	private MovementStatus status;

	private float health = 100f;
	private float hunger = 0f;
	private float sleepiness = 0f;
	private float stress = 0f;
	private float grudge = 0f;

	private Dictionary<MonsterState, float> rageMultiplier = new Dictionary<MonsterState, float> {
		{ MonsterState.calm, 0.5f },
		{ MonsterState.annoyed, 1f },
		{ MonsterState.angry, 1.5f },
		{ MonsterState.replenish, 2f },
		{ MonsterState.berserk, 0.8f }
	};

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
	private float maxPlayerDistance = 10f;
	private float maxattackRange = 2f;

	
	private MonsterState currentState;
	private FAMBehaviour monsterFAM;
	
    private void Awake() {
		currentState = MonsterState.calm;
		currentSpeed = monsterSpeed["roam"];
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		monsterFAM = GetComponent<FAMBehaviour>();
	}

	// Start is called before the first frame update
	void Start()
	{
		status = new MovementStatus ();
		dragBehaviour = GetComponent<DragBehaviour>();
		avoidBehaviourVolume = GetComponent<AvoidBehaviourVolume>();
		seekBehaviour = GetComponent<SeekBehaviour>();
		//fleeBehaviour = GetComponent<FleeBehaviour>();
		freeFleeBehaviour = GetComponent<FreeFleeBehaviour>();
		//freeRoamingBehaviour = GetComponent<FreeRoamingBehaviour>();
		seekRestoreBehaviour = GetComponent<SeekRestoreBehaviour>();

		#region FSM setup

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
		DTDecision angryd4 = new DTDecision(IsAngry);

		// Define links between decisions
		angryd1.AddLink(true, angryd2);
		angryd1.AddLink(false, angryd3);
		angryd2.AddLink(true, calmState);
		angryd2.AddLink(false, angryd4);
		angryd3.AddLink(true, berserkState);
		angryd3.AddLink(false, replenishState);
		angryd4.AddLink(false, annoyedState);

		// From berserk state
		//DTDecision berserkd2 = new DTDecision(GoodPhysicalStatus);
		DTDecision berserkd1 = new DTDecision(GoodMentalStatus);
		DTDecision berserkd2 = new DTDecision(PlayerInRange);

		// Define links between decisions
		berserkd1.AddLink(true, replenishState);
		berserkd1.AddLink(false, berserkd2);
		berserkd2.AddLink(false, replenishState);
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
		calmState.exitActions.Add(StopRoaming);
		annoyedState.enterActions.Add(StartFleeing);
		annoyedState.stayActions.Add(IncreaseGrudge);
		annoyedState.stayActions.Add(Flee);
		annoyedState.exitActions.Add(StopFleeing);
		replenishState.enterActions.Add(StartReplenishing);
		replenishState.exitActions.Add(StopReplenishing);
		angryState.enterActions.Add(StartAngry);
		angryState.stayActions.Add(IncreaseGrudge);
		angryState.stayActions.Add(AngryAttack);
		angryState.exitActions.Add(StopAngry);
		berserkState.enterActions.Add(StartBerserk);
		berserkState.stayActions.Add(IncreaseGrudge);
		berserkState.stayActions.Add(BerserkAttack);
		berserkState.exitActions.Add(StopBerserk);

		#endregion

		if (!GameManager.Instance.useFAM) {
			monsterFSM = new FSM(calmState);
			StartCoroutine(UpdateFSM());
		}
		StartCoroutine(LivePhysicalStatus());
		StartCoroutine(LiveMentalStatus());
	}

	public IEnumerator UpdateFSM() {
		while (true) {
			monsterFSM.Update();
			yield return new WaitForSeconds(reactionTime);
		}
	}

	public IEnumerator LivePhysicalStatus() {
		while (true) {
			if (hunger < 100f) {
				hunger += 0.5f;
			}
			if (sleepiness < 100f) {
				sleepiness += 0.5f;
			}
			yield return new WaitForSeconds(1f);
		}
	}

	public IEnumerator LiveMentalStatus() {
		while (true) {
			stress = stress - 1.0f > 0f ? stress - 1.0f : 0f;
			grudge = grudge - 0.1f > 0f ? grudge - 0.1f : 0f;
			yield return new WaitForSeconds(1f);
		}
	}


	// Update is called once per frame
	void Update()
	{
		if (status.linearSpeed > 3f) {
			animator.SetBool("isRunning", true);
		} else {
			animator.SetBool("isRunning", false);
		}


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
		status.movementDirection = transform.forward;

		// Contact al behaviours and build a list of directions
		List<Vector3> components = new List<Vector3>();
		
		components.Add(dragBehaviour.GetAcceleration(status));
		components.Add(avoidBehaviourVolume.GetAcceleration(status));
		
		
		switch (currentState)
		{
			case MonsterState.calm:
				if (isRoaming) {
					components.Add(freeFleeBehaviour.GetAcceleration(status));
				}
				break;
			case MonsterState.annoyed:
				if (isFleeing) {
					// Continues free roaming while also avoiding the player
					components.Add(freeFleeBehaviour.GetAcceleration(status));
				}
				break;
			case MonsterState.angry:
				if (isChasing && CanSeePlayer()) {
					components.Add(seekBehaviour.GetAcceleration(status));
				} else {
					// Continues free roaming
					components.Add(freeFleeBehaviour.GetAcceleration(status));
				}
				break;
			case MonsterState.berserk:
				if (isChasingBerserk && CanSeePlayer()) {
					components.Add(seekBehaviour.GetAcceleration(status));
				} else {
					// Continues free roaming
					components.Add(freeFleeBehaviour.GetAcceleration(status));
				}
				break;
			case MonsterState.replenish:
				if (isReplenishing) {
					// Tries to reach the restoration point while also avoiding the player
					components.Add(seekRestoreBehaviour.GetAcceleration(status));
				}
				break;
			default:
				break;
		}

		// Blend the list to obtain a single acceleration to apply
		Vector3 blendedAcceleration = Blender.Blend(components);

		// if we have an acceleration, apply it
		if (blendedAcceleration.magnitude != 0f) {
			Driver.Steer(rb, status, blendedAcceleration, minLinearSpeed, currentSpeed, currentSpeed);
			//Driver.Steer(rb, status, blendedAcceleration, minLinearSpeed, maxLinearSpeed, maxAngularSpeed);
		}
	}


	
	#region Decisions

	private object GoodPhysicalStatus(object o) {
		if (health > 20f && hunger < 80f && sleepiness < 80f) {
			return true;
		}
		return false;
	}

	private object GoodMentalStatus(object o) {
		if (stress < 10) {
			return true;
		}
		return false;
	}

	private object PlayerInRange(object o) {
		if (Vector3.Distance(transform.position, player.transform.position) < maxPlayerDistance) {
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

	private bool PlayerInAttackRange() {
		if (Vector3.Distance(transform.position, player.transform.position) < maxattackRange) {
			return true;
		}
		return false;
	}
	
	private bool sensedPlayer = false;
	private float maxSeekTime = 15f;
	private Coroutine seekCoroutine = null;

	private bool CanSeePlayer(){
		RaycastHit hit;
		Vector3 headPosition = transform.GetChild(2).position;
		Vector3 playerHeadPosition = player.transform.GetChild(2).position;
		Vector3 rayDirection = playerHeadPosition - headPosition;
		if (Physics.Raycast(headPosition, rayDirection, out hit, maxPlayerDistance)
				&& Vector3.Angle(rayDirection, transform.forward) < 90f) {
			if (hit.collider.gameObject == player) {
				Debug.DrawRay(headPosition, rayDirection, Color.green);
				sensedPlayer = true;
				return true;
			}
		}
		Debug.DrawRay(headPosition, rayDirection, Color.red);

		// If we saw the player at least once, we start by going to the last known position
		if (sensedPlayer) {
			if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
			sensedPlayer = false;
			freeFleeBehaviour.ResetTargetRandomPosition(player.transform, true);
			seekCoroutine = StartCoroutine(TargetedSeeking(maxSeekTime));
		}
		return false;
	}

	// This function destroys the device after a certain time if it doesn't hit anything
	private IEnumerator TargetedSeeking(float giveUpTime) {
		Debug.Log("Start seeking targeted...");
		yield return new WaitForSecondsRealtime(giveUpTime);
		freeFleeBehaviour.SetIsSeeking(false);
	}

	#endregion
	
	#region Actions

	public void StartRoaming() {
		isRoaming = true;
		currentSpeed = monsterSpeed["roam"];
		currentState = MonsterState.calm;
	}

	/*

	// Function for free map roaming

	private float stopAt = 0.1f;
	private Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	private float time = 0f;
	private float timeToTarget = 5f;

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

	*/

	public void StopRoaming() {
		isRoaming = false;
	}

	public void StartFleeing() {
		isFleeing = true;
		freeFleeBehaviour.SetIsFleeing(true);
		currentSpeed = monsterSpeed["flee"];
		currentState = MonsterState.annoyed;
	}

	public void Flee() {
		//Debug.Log("Chasing");
		if (freeFleeBehaviour.PlayerInRange()) {
			//Debug.Log("Player in range");
			currentSpeed = Mathf.Lerp(monsterSpeed["flee"], monsterSpeed["flee"]*2, freeFleeBehaviour.GetPercentage());
		} else {
			//Debug.Log("Player not in range");
			currentSpeed = monsterSpeed["flee"];
		}
	}

	public void IncreaseGrudge() {
		grudge = grudge + reactionTime > 100f ? 100f : grudge + reactionTime;
	}

	public void StopFleeing() {
		isFleeing = false;
		freeFleeBehaviour.SetIsFleeing(false);
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartAngry() {
		isChasing = true;
		currentSpeed = monsterSpeed["chase"];
		currentState = MonsterState.angry;
	}

	public void AngryAttack() {
		if(PlayerInAttackRange()) {
			Attack();
			// Decrease stress by 20% and grudge by 10%
			// FSM will take care of the transition to the calm state
			stress *= 0.8f;
			grudge *= 0.9f;
		}
	}

	public void StopAngry() {
		isChasing = false;
		if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
		freeFleeBehaviour.SetIsSeeking(false);
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartBerserk() {
		isChasingBerserk = true;
		currentSpeed = monsterSpeed["berserk"];
		currentState = MonsterState.berserk;
	}

	public void BerserkAttack() {
		if(PlayerInAttackRange()) {
			Attack();
			// Decrease stress by 10% and grudge by 5%
			// FSM will take care to transition back to the replenish state
			stress *= 0.9f;
			grudge *= 0.95f;
		}
	}

	public void StopBerserk() {
		isChasingBerserk = false;
		if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
		freeFleeBehaviour.SetIsSeeking(false);
		if (isAttacking) {
			isAttacking = false;
		}
	}

	public void StartReplenishing() {
		isReplenishing = true;
		currentSpeed = monsterSpeed["flee"];
		currentState = MonsterState.replenish;
	}

	public void Replenish() {
		Debug.Log("Replenishing");
		health = 100f;
		hunger = 0f;
		sleepiness = 0f;
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

	public void Attack() {
		Debug.Log("Attacking");
		isAttacking = true;
		player.GetComponent<PlayerBehaviour>().TakeDamage(10f);
		animator.SetTrigger("isAttacking");
		isAttacking = false;
	}

	public void TakeDamage(float damage, float stressDamage, float grudgeDamage) {
		// Only for demo purposes, the monster can't die
		health = health - damage < 0f ? 0f : health - damage;
		stressDamage *= rageMultiplier[currentState];
		grudgeDamage *= rageMultiplier[currentState];
		stress = stress + stressDamage > 100f ? 100f : stress + stressDamage;
		grudge = grudge + grudgeDamage > 100f ? 100f : grudge + grudgeDamage;
		Debug.Log("Easy Mode: " + GameManager.Instance.easyMode);
		if (health == 0f && !GameManager.Instance.easyMode) {
			GameManager.Instance.GameOver();
		}
	}

	#endregion

	#region Getters
		public float GetHealth() {
			return health;
		}
	
		public float GetHunger() {
			return hunger;
		}
	
		public float GetSleepiness() {
			return sleepiness;
		}
	
		public float GetStress() {
			return stress;
		}
	
		public float GetGrudge() {
			return grudge;
		}

		public float GetReactionTime()
		{
			return reactionTime;
		}

    #endregion

}
