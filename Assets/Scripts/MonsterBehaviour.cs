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

	private Coroutine coroutineFSM;	// Handle for the UpdateFSM coroutine, used when switching between FSM and FAM
	private float reactionTime = 1.5f;
	private float hungerStep = 0.5f;	// Hunger increase per second
	private float sleepinessStep = 0.5f;	// Sleepiness increase per second
	private float stressStep = 1f;	// Stress decrease per second
	private float grudgeStep = 0.1f;	// Grudge decrease per second
	private float liveUpdateStep = 1f;	// Time interval for updating the NPC's MS and PS

	DragBehaviour dragBehaviour;
	AvoidBehaviourVolume avoidBehaviourVolume;
	SeekBehaviour seekBehaviour;
	FreeFleeBehaviour freeFleeBehaviour;
	SeekRestoreBehaviour seekRestoreBehaviour;
	private bool sensedPlayer = false;
	private float maxSeekTime = 15f;
	private Coroutine seekCoroutine = null;

	// Max speed values for NPC movement in different states
	private Dictionary<string, float> monsterSpeed = new Dictionary<string, float> {
		{ "roam", 1f },
		{ "flee", 2.5f },
		{ "chase", 5f },
		{ "berserk", 8f }
	};
	public float minLinearSpeed = 0.5f;
	public float currentMaxSpeed;	// Used for both linear and angular speed
	private MovementStatus status;

	private float health = 100f;
	private float hunger = 0f;
	private float sleepiness = 0f;
	private float stress = 0f;
	private float grudge = 0f;

	private bool isHealing = false;
	private bool isSleeping = false;
	private bool isEating = false;
	private float maxPlayerDistance = 10f;
	private float maxattackRange = 2f;
	private float attackPower = 10f;

	// Rage multiplier to amplify or dampen stress and grudge from attacks depending on the current state
	private Dictionary<MonsterState, float> rageMultiplier = new Dictionary<MonsterState, float> {
		{ MonsterState.calm, 0.5f },
		{ MonsterState.annoyed, 1f },
		{ MonsterState.angry, 1.5f },
		{ MonsterState.replenish, 2f },
		{ MonsterState.berserk, 0.8f }
	};

	private FSM monsterFSM;	
	private MonsterState currentState;
	
    private void Awake() {
		currentState = MonsterState.calm;
		currentMaxSpeed = monsterSpeed["roam"];
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
	}

	// Start is called before the first frame update
	void Start()
	{
		status = new MovementStatus ();
		dragBehaviour = GetComponent<DragBehaviour>();
		avoidBehaviourVolume = GetComponent<AvoidBehaviourVolume>();
		seekBehaviour = GetComponent<SeekBehaviour>();
		freeFleeBehaviour = GetComponent<FreeFleeBehaviour>();
		seekRestoreBehaviour = GetComponent<SeekRestoreBehaviour>();

		#region FSM setup

		FSMState calmState = new FSMState(MonsterState.calm);
		FSMState annoyedState = new FSMState(MonsterState.annoyed);
		FSMState replenishState = new FSMState(MonsterState.replenish);
		FSMState angryState = new FSMState(MonsterState.angry);
		FSMState berserkState = new FSMState(MonsterState.berserk);

		// Define DT transitions for each state.
		/* NOTE: if we want to stay in the same state, we don't need to add a link:
		 * We can either explicitly add the link or not, the FSM Update won't trigger transitions
		 * if the current state is the same as the next or if the walk does not return an FSMState.
		 * We leave the link to the same state commented for convenience.
		*/

		// From calm state...
		DTDecision calmd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision calmd2 = new DTDecision(GoodMentalStatus);
		DTDecision calmd3 = new DTDecision(IsAngry);

		// Define links between decisions
		calmd1.AddLink(true, calmd2);
		calmd1.AddLink(false, replenishState);
		//calmd2.AddLink(true, calmState); 
		calmd2.AddLink(false, calmd3);
		calmd3.AddLink(true, angryState);
		calmd3.AddLink(false, annoyedState);

		// From annoyed state...
		DTDecision annoyedd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision annoyedd2 = new DTDecision(GoodMentalStatus);
		DTDecision annoyedd3 = new DTDecision(IsAngry);

		// Define links between decisions
		annoyedd1.AddLink(true, annoyedd2);
		annoyedd1.AddLink(false, replenishState);
		annoyedd2.AddLink(true, calmState);
		annoyedd2.AddLink(false, annoyedd3);
		annoyedd3.AddLink(true, angryState);
		//annoyedd3.AddLink(false, annoyedState);

		// From replenish state...
		DTDecision replenishd1 = new DTDecision(GoodPhysicalStatus);
		DTDecision replenishd2 = new DTDecision(GoodMentalStatus);
		DTDecision replenishd2bis = new DTDecision(GoodMentalStatus);
		DTDecision replenishd3 = new DTDecision(IsAngry);
		DTDecision replenishd4 = new DTDecision(PlayerInRange);

		// Define links between decisions
		replenishd1.AddLink(true, replenishd2);
		replenishd1.AddLink(false, replenishd2bis);
		replenishd2.AddLink(true, calmState);
		replenishd2.AddLink(false, replenishd3);
		replenishd3.AddLink(true, angryState);
		replenishd3.AddLink(false, annoyedState);
		//replenishd2bis.AddLink(true, replenishState);
		replenishd2bis.AddLink(false, replenishd4);
		replenishd4.AddLink(true, berserkState);
		//replenishd4.AddLink(false, replenishState);

		// From angry state...
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

		// From berserk state...
		DTDecision berserkd1 = new DTDecision(GoodMentalStatus);
		DTDecision berserkd2 = new DTDecision(PlayerInRange);

		// Define links between decisions
		berserkd1.AddLink(true, replenishState);
		berserkd1.AddLink(false, berserkd2);
		berserkd2.AddLink(false, replenishState);
		//berserkd1.AddLink(false, berserkState);

		// Setup my DecisionTree at the root node
		DecisionTree calmDT = new DecisionTree(calmd1);
		DecisionTree annoyedDT = new DecisionTree(annoyedd1);
		DecisionTree replenishDT = new DecisionTree(replenishd1);
		DecisionTree angryDT = new DecisionTree(angryd1);
		DecisionTree berserkDT = new DecisionTree(berserkd1);

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
		
		monsterFSM = new FSM(calmState);
		// This dictionary is only useful when switching between FSM and FAM...
		monsterFSM.states.Add(MonsterState.calm, calmState);
		monsterFSM.states.Add(MonsterState.annoyed, annoyedState);
		monsterFSM.states.Add(MonsterState.angry, angryState);
		monsterFSM.states.Add(MonsterState.replenish, replenishState);
		monsterFSM.states.Add(MonsterState.berserk, berserkState);
		// ...so we can use it to explicitly set the current state when switching back to FSM

		if (!GameManager.Instance.useFAM) {
			monsterFSM.EnterFirstState();
			coroutineFSM = StartCoroutine(UpdateFSM());
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

	// Stops the FSM coroutine to switch to FAM
	public void StopFSM() {
		if (coroutineFSM != null) {
			StopCoroutine(coroutineFSM);
		}
	}

	// Called to start the FSM update when switching from FAM to FSM
	public void StartFSM() {
		monsterFSM.SetCurrentState(currentState);
		coroutineFSM = StartCoroutine(UpdateFSM());
	}

	// Increases NPC's hunger and sleepiness over time
	public IEnumerator LivePhysicalStatus() {
		while (true) {
			if (hunger < 100f) {
				hunger += hungerStep;
			}
			if (sleepiness < 100f) {
				sleepiness += sleepinessStep;
			}
			yield return new WaitForSeconds(liveUpdateStep);
		}
	}

	// Decreases NPC's stress and grudge over time
	public IEnumerator LiveMentalStatus() {
		while (true) {
			stress = stress - stressStep > 0f ? stress - stressStep : 0f;
			grudge = grudge - grudgeStep > 0f ? grudge - grudgeStep : 0f;
			yield return new WaitForSeconds(liveUpdateStep);
		}
	}


	void Update()
	{
		// Set running/walking animation based on current speed
		if (status.linearSpeed > 3f) {
			animator.SetBool("isRunning", true);
		} else {
			animator.SetBool("isRunning", false);
		}

		// Used for logging to in-game UI
		string logText = currentState + "\n" + health + "\n" + hunger + "\n" + sleepiness + "\n" + stress + "\n" + grudge;
		GameManager.Instance.SetNPCLoggingText(logText);

		// Only used for debugging to manually increase/decrease status values
		bool debugging = false;
		if (debugging) {
			if (Input.GetKeyDown(KeyCode.Return)) {
				health = 100f;
				hunger = 0f;
				sleepiness = 0f;
				stress = 0f;
				grudge = 0f;
			}
			if (Input.GetKeyDown(KeyCode.Z)) {
				health -= 20f;
				Debug.Log("Health: " + health);
			}
			if (Input.GetKeyDown(KeyCode.X)) {
				stress += 20f;
				Debug.Log("Stress: " + stress);
			}
			if (Input.GetKeyDown(KeyCode.C)) {
				grudge += 20f;
				Debug.Log("Grudge: " + grudge);
			}
		}
	}
	
	
	private void FixedUpdate() {
		status.movementDirection = transform.forward;

		// List of directions to blend obtained from each behaviour
		List<Vector3> components = new List<Vector3>();
		
		components.Add(dragBehaviour.GetAcceleration(status));
		components.Add(avoidBehaviourVolume.GetAcceleration(status));
		
		if (currentState == MonsterState.calm || currentState == MonsterState.annoyed) {
			// If annoyed, continues free roaming while also avoiding the player
			components.Add(freeFleeBehaviour.GetAcceleration(status));
		} else if (currentState == MonsterState.angry || currentState == MonsterState.berserk) {
				if (CanSeePlayer()) {
					components.Add(seekBehaviour.GetAcceleration(status));
				} else {
					// Continues free roaming
					components.Add(freeFleeBehaviour.GetAcceleration(status));
				}
		} else if (currentState == MonsterState.replenish) {
			components.Add(seekRestoreBehaviour.GetAcceleration(status));
		}

		// Blend the list to obtain a single acceleration to apply
		Vector3 blendedAcceleration = Blender.Blend(components);

		// if we have an acceleration, apply it
		if (blendedAcceleration.magnitude != 0f) {
			Driver.Steer(rb, status, blendedAcceleration, minLinearSpeed, currentMaxSpeed, currentMaxSpeed);
		}
	}


	// Region for both decision tree's and general decisions
	#region Decisions

	private object GoodPhysicalStatus(object o) {
		if (health > 20f && hunger < 80f && sleepiness < 80f) {
			return true;
		}
		return false;
	}

	// This only tests stress to check if it has some sort of mental status discomfort...
	private object GoodMentalStatus(object o) {
		if (stress < 10) {
			return true;
		}
		return false;
	}

	// ...while this tests both stress and grudge to check if the NPC is angry. These two work together
	// to determine the emotional state of the NPC
	private object IsAngry(object o) {
		if (stress + grudge > 100f) {
			return true;
		}
		return false;
	}

	// This checks if the player is inside a certain range, the NPC can "sense" the player presence
	private object PlayerInRange(object o) {
		if (Vector3.Distance(transform.position, player.transform.position) < maxPlayerDistance) {
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
	
	// This checks if the NPC can see the player (returning true will start the seek behaviour)
	private bool CanSeePlayer(){
		RaycastHit hit;
		Vector3 headPosition = transform.GetChild(2).position;
		Vector3 playerHeadPosition = player.transform.GetChild(2).position;
		Vector3 rayDirection = playerHeadPosition - headPosition;	// Placed empty game objects at the heads positions for convenience
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

	// This function sets a timer to stop seeking the player after a certain amount of time and resume normal free roaming
	private IEnumerator TargetedSeeking(float giveUpTime) {
		Debug.Log("Start seeking targeted...");
		yield return new WaitForSecondsRealtime(giveUpTime);
		freeFleeBehaviour.SetIsSeeking(false);
	}

	#endregion
	
	#region Actions

	public void StartRoaming() {
		currentMaxSpeed = monsterSpeed["roam"];
		currentState = MonsterState.calm;
	}

	public void StopRoaming() {
	}

	public void StartFleeing() {
		freeFleeBehaviour.SetIsFleeing(true);
		currentMaxSpeed = monsterSpeed["flee"];
		currentState = MonsterState.annoyed;
	}

	// Annoyed stay action to increase max flee speed when the player is in range
	public void Flee() {
		//Debug.Log("Started fleeing");
		if (freeFleeBehaviour.PlayerInRange()) {
			//Debug.Log("Player in range");
			currentMaxSpeed = Mathf.Lerp(monsterSpeed["flee"], monsterSpeed["flee"]*2, freeFleeBehaviour.GetPercentage());
		} else {
			//Debug.Log("Player not in range");
			currentMaxSpeed = monsterSpeed["flee"];
		}
	}

	// Stay action for various states to increase grudge over time (when it is not calm, it builds up grudge)
	public void IncreaseGrudge() {
		grudge = grudge + reactionTime > 100f ? 100f : grudge + reactionTime;
	}

	public void StopFleeing() {
		freeFleeBehaviour.SetIsFleeing(false);
	}

	public void StartAngry() {
		currentMaxSpeed = monsterSpeed["chase"];
		currentState = MonsterState.angry;
	}

	// Decrease stress by 20% and grudge by 10%
	// FSM will take care of the transition to less-angry states
	public void AngryAttack() {
		if(PlayerInAttackRange()) {
			StartCoroutine(Attack());
			stress *= 0.8f;
			grudge *= 0.9f;
		}
	}

	public void StopAngry() {
		if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
		freeFleeBehaviour.SetIsSeeking(false);
	}

	public void StartBerserk() {
		currentMaxSpeed = monsterSpeed["berserk"];
		currentState = MonsterState.berserk;
	}

	// Decrease stress by 10% and grudge by 5%
	// FSM will take care to transition back to the replenish state
	public void BerserkAttack() {
		if(PlayerInAttackRange()) {
			StartCoroutine(Attack());
			stress *= 0.9f;
			grudge *= 0.95f;
		}
	}

	public void StopBerserk() {
		if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
		freeFleeBehaviour.SetIsSeeking(false);
	}

	public void StartReplenishing() {
		currentMaxSpeed = monsterSpeed["flee"];
		currentState = MonsterState.replenish;
	}

	public void Replenish() {
		health = 100f;
		hunger = 0f;
		sleepiness = 0f;
	}

	public void StopReplenishing() {
		if (isHealing || isEating || isSleeping) {
			isHealing = false;
			isEating = false;
			isSleeping = false;
		}
	}

	private IEnumerator Attack() {
		Debug.Log("Attacking");
		transform.LookAt(player.transform);
		player.GetComponent<PlayerBehaviour>().TakeDamage(attackPower);
		animator.SetTrigger("isAttacking");
		yield return null;
	}

	// Only for demo purposes, the NPC can't die in easy mode
	public void TakeDamage(float damage, float stressDamage, float grudgeDamage) {
		health = health - damage < 0f ? 0f : health - damage;
		stressDamage *= rageMultiplier[currentState];
		grudgeDamage *= rageMultiplier[currentState];
		stress = stress + stressDamage > 100f ? 100f : stress + stressDamage;
		grudge = grudge + grudgeDamage > 100f ? 100f : grudge + grudgeDamage;
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

		public MonsterState GetCurrentState() {
			return currentState;
		}

    #endregion

}
