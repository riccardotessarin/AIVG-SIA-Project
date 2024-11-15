using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FAMBehaviour : MonoBehaviour
{
	private MonsterBehaviour monsterBehaviour;
	private Coroutine coroutineFAM;
	private FAMState currentState;
	private Dictionary<MonsterState, FAMState> states;
	private Fuzzify fuzzify;
	private float crispHealth;
	private float crispHunger;
	private float crispSleepiness;
	private float crispStress;
	private float crispGrudge;
	private float physicalStatusValue;
	private float mentalStatusValue;
	private FuzzyVariable healthFuzzy;
	private FuzzyVariable hungerFuzzy;
	private FuzzyVariable sleepinessFuzzy;
	private FuzzyVariable stressFuzzy;
	private FuzzyVariable grudgeFuzzy;
	private FuzzyVariable physicalstatus;
	private FuzzyVariable mentalstatus;
	private FuzzyRule[] physicalRules;
	private FuzzyRule[] mentalRules;
	private FAM physicalFAM;
	private FAM mentalFAM;
	private bool useDefuzzyMean = true;
	private float lowThreshold = 0.3f;
	private float highThreshold = 0.7f;
	private float reactionTime;

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		fuzzify = new Fuzzify();
		fuzzify.AddMembershipFunction(FuzzyClass.low, new float[] { 0f, 0f, 20f, 40f });
		fuzzify.AddMembershipFunction(FuzzyClass.medium, new float[] { 20f, 40f, 60f, 80f });
		fuzzify.AddMembershipFunction(FuzzyClass.high, new float[] { 60f, 80f, 100f, 100f });
		monsterBehaviour = GetComponent<MonsterBehaviour>();
		GetMonsterParams();
		states = new Dictionary<MonsterState, FAMState>();
		FAMState calm = new FAMState(MonsterState.calm);
		calm.enterActions.Add(monsterBehaviour.StartRoaming);
		calm.exitActions.Add(monsterBehaviour.StopRoaming);
		states.Add(MonsterState.calm, calm);
		FAMState annoyed = new FAMState(MonsterState.annoyed);
		annoyed.enterActions.Add(monsterBehaviour.StartFleeing);
		annoyed.stayActions.Add(monsterBehaviour.IncreaseGrudge);
		annoyed.stayActions.Add(monsterBehaviour.Flee);
		annoyed.exitActions.Add(monsterBehaviour.StopFleeing);
		states.Add(MonsterState.annoyed, annoyed);
		FAMState angry = new FAMState(MonsterState.angry);
		angry.enterActions.Add(monsterBehaviour.StartAngry);
		angry.stayActions.Add(monsterBehaviour.IncreaseGrudge);
		angry.stayActions.Add(monsterBehaviour.AngryAttack);
		angry.exitActions.Add(monsterBehaviour.StopAngry);
		states.Add(MonsterState.angry, angry);
		FAMState berserk = new FAMState(MonsterState.berserk);
		berserk.enterActions.Add(monsterBehaviour.StartBerserk);
		berserk.stayActions.Add(monsterBehaviour.IncreaseGrudge);
		berserk.stayActions.Add(monsterBehaviour.BerserkAttack);
		berserk.exitActions.Add(monsterBehaviour.StopBerserk);
		states.Add(MonsterState.berserk, berserk);
		FAMState replenish = new FAMState(MonsterState.replenish);
		replenish.enterActions.Add(monsterBehaviour.StartReplenishing);
		replenish.exitActions.Add(monsterBehaviour.StopReplenishing);
		states.Add(MonsterState.replenish, replenish);
		GameManager.GameStructureChanged += GameManagerOnGameStructureChanged;
	}

	private void OnDestroy() {
		GameManager.GameStructureChanged -= GameManagerOnGameStructureChanged;
	}

	private void GameManagerOnGameStructureChanged(GameStructure structure) {
		switch ( structure ) {
			case GameStructure.FSM:
				Debug.Log("Switching to FSM");
				StopFAM();
				monsterBehaviour.StartFSM();
				break;
			case GameStructure.FAM:
				Debug.Log("Switching to FAM");
				monsterBehaviour.StopFSM();
				StartFAM();
				break;
			default:
				break;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		healthFuzzy = fuzzify.ToFuzzy("health", crispHealth);
		//Debug.Log("Health fuzzy: " + healthFuzzy.MembershipValues[FuzzyClass.low] + ", " + healthFuzzy.MembershipValues[FuzzyClass.medium] + ", " + healthFuzzy.MembershipValues[FuzzyClass.high]);
		hungerFuzzy = fuzzify.ToFuzzy("hunger", crispHunger);
		//Debug.Log("Hunger fuzzy: " + hungerFuzzy.MembershipValues[FuzzyClass.low] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.medium] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.high]);
		sleepinessFuzzy = fuzzify.ToFuzzy("sleepiness", crispSleepiness);
		//Debug.Log("Sleepiness fuzzy: " + sleepinessFuzzy.MembershipValues[FuzzyClass.low] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.medium] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.high]);

		// Make empty fuzzy variable for the output
		physicalstatus = new FuzzyVariable() { Name = "physicalstatus" };

		stressFuzzy = fuzzify.ToFuzzy("stress", crispStress);
		//Debug.Log("Stress fuzzy: " + stressFuzzy.MembershipValues[FuzzyClass.low] + ", " + stressFuzzy.MembershipValues[FuzzyClass.medium] + ", " + stressFuzzy.MembershipValues[FuzzyClass.high]);
		grudgeFuzzy = fuzzify.ToFuzzy("grudge", crispGrudge);
		//Debug.Log("Grudge fuzzy: " + grudgeFuzzy.MembershipValues[FuzzyClass.low] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.medium] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.high]);

		// Make empty fuzzy variable for the output
		mentalstatus = new FuzzyVariable() { Name = "mentalstatus" };

		physicalRules = new FuzzyRule[]
		{
			#region Physical status rules
			// General rule: if health is low, physical status is low. NPC's priority is to think about itself rather than the player
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = hungerFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium, FuzzyClass.high } },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium, FuzzyClass.high } }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.low }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.medium },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium, FuzzyClass.high } }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.low }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.high }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.low }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.medium },
					new FuzzyCondition { Variable = hungerFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium } },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.high }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.medium }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.medium },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.high }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.medium }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.medium }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.medium }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.high }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.high }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.low }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.high }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.medium, FuzzyClass.high } },
					new FuzzyCondition { Variable = hungerFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium } },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium } }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.high }
			}
			#endregion
		};

		mentalRules = new FuzzyRule[] {
			#region Mental status rules
				new FuzzyRule {
					Conditions = new FuzzyCondition[]
					{
						new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.low },
						new FuzzyCondition { Variable = grudgeFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium } },
					},
					Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.high }
				},
				new FuzzyRule
				{
					Conditions = new FuzzyCondition[]
					{
						new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.medium },
						new FuzzyCondition { Variable = grudgeFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium } },
					},
					Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.medium }
				},
				new FuzzyRule
				{
					Conditions = new FuzzyCondition[]
					{
						new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.low },
						new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.high },
					},
					Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.medium }
				},
				new FuzzyRule
				{
					Conditions = new FuzzyCondition[]
					{
						new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.high },
						new FuzzyCondition { Variable = grudgeFuzzy, SetClasses = new FuzzyClass[] { FuzzyClass.low, FuzzyClass.medium, FuzzyClass.high } },
					},
					Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.low }
				},
				new FuzzyRule
				{
					Conditions = new FuzzyCondition[]
					{
						new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.medium },
						new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.high },
					},
					Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.low }
				},
			#endregion
		};

		// Create the fuzzy inference system
		physicalFAM = new FAM
		{
			Rules = physicalRules,
			Inputs = new List<FuzzyVariable> { healthFuzzy, hungerFuzzy, sleepinessFuzzy },
			Outputs = new FuzzyVariable[] { physicalstatus }
		};

		// Create the fuzzy inference system
		mentalFAM = new FAM
		{
			Rules = mentalRules,
			Inputs = new List<FuzzyVariable> { stressFuzzy, grudgeFuzzy },
			Outputs = new FuzzyVariable[] { mentalstatus }
		};

		// Perform the fuzzy inference
		physicalFAM.Calculate();
		mentalFAM.Calculate();

		//Debug.Log("Mental status: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
		//Debug.Log("Physical status: " + physicalstatus.MembershipValues[FuzzyClass.low] + ", " + physicalstatus.MembershipValues[FuzzyClass.medium] + ", " + physicalstatus.MembershipValues[FuzzyClass.high]);

		// Get the output values
		physicalStatusValue = fuzzify.DefuzzifyMax(physicalstatus);
		//Debug.Log("Physical status value max: " + physicalStatusValue);

		// Get the output values
		mentalStatusValue = fuzzify.DefuzzifyMax(mentalstatus);
		//Debug.Log("Mental status value max: " + mentalStatusValue);

		currentState = UpdateState();
		//Debug.Log("Current state: " + currentState.stateName);

		if(GameManager.Instance.useFAM) {
			currentState.Enter();
			coroutineFAM = StartCoroutine(UpdateCoroutine());
		}
	}

	public IEnumerator UpdateCoroutine() {
		while (true) {
			UpdateFAM();
			yield return new WaitForSeconds(reactionTime);
		}
	}

	public void StopFAM() {
		if (coroutineFAM != null) {
			StopCoroutine(coroutineFAM);
		}
	}

	public void StartFAM() {
		currentState = states[monsterBehaviour.GetCurrentState()];
		coroutineFAM = StartCoroutine(UpdateCoroutine());
	}

	private void GetMonsterParams() {
		reactionTime = monsterBehaviour.GetReactionTime();
		crispHealth = monsterBehaviour.GetHealth();
		crispHunger = monsterBehaviour.GetHunger();
		crispSleepiness = monsterBehaviour.GetSleepiness();
		crispStress = monsterBehaviour.GetStress();
		crispGrudge = monsterBehaviour.GetGrudge();
	}

	private bool UpdateFuzzyParam(ref FuzzyVariable fuzzyVariable, string name, float crispValue) {
		FuzzyVariable otherFuzzyVariable = fuzzify.ToFuzzy(name, crispValue);
		if (!fuzzyVariable.IsEqual(otherFuzzyVariable)) {
			fuzzyVariable.UpdateMembershipValues(otherFuzzyVariable);
			return true;
		}
		return false;
	}

	private void ParamsToFuzzy(out bool physicalParamsChanged, out bool mentalParamsChanged) {
		physicalParamsChanged = false;
		mentalParamsChanged = false;
		
		if (UpdateFuzzyParam(ref healthFuzzy, "health", crispHealth)) {
			physicalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref hungerFuzzy, "hunger", crispHunger)) {
			physicalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref sleepinessFuzzy, "sleepiness", crispSleepiness)) {
			physicalParamsChanged = true;
		}
		
		if (UpdateFuzzyParam(ref stressFuzzy, "stress", crispStress)) {
			mentalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref grudgeFuzzy, "grudge", crispGrudge)) {
			mentalParamsChanged = true;
		}
	}

	public FAMState UpdateState() {
		// Determine the NPC state based on the output values
		if (physicalStatusValue < lowThreshold) {
			if (mentalStatusValue < lowThreshold) {
				return states[MonsterState.berserk];
			}
			return states[MonsterState.replenish];
		} else {
			switch (mentalStatusValue)
			{
				case float n when (n < lowThreshold):
					return states[MonsterState.angry];
				case float n when (n >= lowThreshold && n < highThreshold):
					return states[MonsterState.annoyed];
				default:
					return states[MonsterState.calm];
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void UpdateFAM() {
		//Debug.Log("Updating FAM. Current state: " + currentState.stateName);
		GetMonsterParams();
		bool physicalParamsChanged;
		bool mentalParamsChanged;

		ParamsToFuzzy(out physicalParamsChanged, out mentalParamsChanged);
		if (!physicalParamsChanged && !mentalParamsChanged) {
			//Debug.Log("No fuzzy variables changed");
			return;
		}
		if (physicalParamsChanged) {
			//Debug.Log("Physical fuzzy variables changed");
			physicalFAM.Calculate();
			physicalstatus = physicalFAM.GetSingleOutput();
			physicalStatusValue = useDefuzzyMean ? fuzzify.DefuzzifyMean(physicalstatus) : fuzzify.DefuzzifyMax(physicalstatus);
			Debug.Log("Defuzzy physical status value: " + physicalStatusValue);
		}
		if (mentalParamsChanged) {
			//Debug.Log("Mental fuzzy variables changed");
			mentalFAM.Calculate();
			mentalstatus = mentalFAM.GetSingleOutput();
			mentalStatusValue = useDefuzzyMean ? fuzzify.DefuzzifyMean(mentalstatus) : fuzzify.DefuzzifyMax(mentalstatus);
			Debug.Log("Defuzzy mental status value: " + mentalStatusValue);
		}
		if (UpdateState() is FAMState transition && currentState != transition) {
			Debug.Log("State changed from " + currentState.stateName + " to " + transition.stateName);
			GameManager.Instance.SetGameMessage("State changed from " + currentState.stateName + " to " + transition.stateName);
			currentState.Exit(); // Exit old state
			currentState = transition;
			currentState.Enter(); // Enter new state
		}
		currentState.Stay(); // Stay in current state
	}
}