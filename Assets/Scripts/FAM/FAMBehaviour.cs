using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FAMBehaviour : MonoBehaviour
{
	private MonsterBehaviour monsterBehaviour;
	private MonsterState currentState;
	private Fuzzify fuzzify;
	float crispHealth;
	float crispHunger;
	float crispSleepiness;
	float crispStress;
	float crispGrudge;
	float physicalStatusValue;
	float mentalStatusValue;
	FuzzyVariable healthFuzzy;
	FuzzyVariable hungerFuzzy;
	FuzzyVariable sleepinessFuzzy;
	FuzzyVariable stressFuzzy;
	FuzzyVariable grudgeFuzzy;
	FuzzyVariable physicalstatus;
	FuzzyVariable mentalstatus;
	FuzzyRule[] physicalRules;
	FuzzyRule[] mentalRules;
	FuzzyInferenceSystem physicalFIS;
	FuzzyInferenceSystem mentalFIS;
	float lowThreshold = 0.3f;
	float highThreshold = 0.7f;
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
	}

	// Start is called before the first frame update
	void Start()
	{
		healthFuzzy = fuzzify.ToFuzzy(crispHealth);
		Debug.Log("Health fuzzy: " + healthFuzzy.MembershipValues[FuzzyClass.low] + ", " + healthFuzzy.MembershipValues[FuzzyClass.medium] + ", " + healthFuzzy.MembershipValues[FuzzyClass.high]);

		hungerFuzzy = fuzzify.ToFuzzy(crispHunger);
		Debug.Log("Hunger fuzzy: " + hungerFuzzy.MembershipValues[FuzzyClass.low] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.medium] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.high]);
		sleepinessFuzzy = fuzzify.ToFuzzy(crispSleepiness);
		Debug.Log("Sleepiness fuzzy: " + sleepinessFuzzy.MembershipValues[FuzzyClass.low] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.medium] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.high]);

		// Make empty fuzzy variable for the output
		physicalstatus = new FuzzyVariable();


		stressFuzzy = fuzzify.ToFuzzy(crispStress);
		Debug.Log("Stress fuzzy: " + stressFuzzy.MembershipValues[FuzzyClass.low] + ", " + stressFuzzy.MembershipValues[FuzzyClass.medium] + ", " + stressFuzzy.MembershipValues[FuzzyClass.high]);
		//Debug.Log("Stress fuzzy: " + stressFuzzy.Low + ", " + stressFuzzy.Medium + ", " + stressFuzzy.High);
		grudgeFuzzy = fuzzify.ToFuzzy(crispGrudge);
		Debug.Log("Grudge fuzzy: " + grudgeFuzzy.MembershipValues[FuzzyClass.low] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.medium] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.high]);
		//Debug.Log("Grudge fuzzy: " + grudgeFuzzy.Low + ", " + grudgeFuzzy.Medium + ", " + grudgeFuzzy.High);

		// Make empty fuzzy variable for the output
		mentalstatus = new FuzzyVariable();

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
		physicalFIS = new FuzzyInferenceSystem
		{
			Rules = physicalRules,
			Inputs = new FuzzyVariable[] { healthFuzzy, hungerFuzzy, sleepinessFuzzy },
			Outputs = new FuzzyVariable[] { physicalstatus }
		};

		// Create the fuzzy inference system
		mentalFIS = new FuzzyInferenceSystem
		{
			Rules = mentalRules,
			Inputs = new FuzzyVariable[] { stressFuzzy, grudgeFuzzy },
			Outputs = new FuzzyVariable[] { mentalstatus }
		};

		// Perform the fuzzy inference
		physicalFIS.Calculate();
		mentalFIS.Calculate();

		Debug.Log("Mental status: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
		Debug.Log("Physical status: " + physicalstatus.MembershipValues[FuzzyClass.low] + ", " + physicalstatus.MembershipValues[FuzzyClass.medium] + ", " + physicalstatus.MembershipValues[FuzzyClass.high]);

		// Get the output values
		//double physicalStatusValue = Math.Max(physicalstatus.MembershipValues[FuzzyClass.low], Math.Max(physicalstatus.MembershipValues[FuzzyClass.medium], physicalstatus.MembershipValues[FuzzyClass.high]));
		physicalStatusValue = fuzzify.DefuzzifyMax(physicalstatus);
		float physicalStatusValueMean = fuzzify.DefuzzifyMean(physicalstatus);
		Debug.Log("Physical status value max: " + physicalStatusValue + " Mean: " + physicalStatusValueMean);

		// Get the output values
		//double mentalStatusValue = Math.Max(mentalstatus.MembershipValues[FuzzyClass.low], Math.Max(mentalstatus.MembershipValues[FuzzyClass.medium], mentalstatus.MembershipValues[FuzzyClass.high]));
		//double mentalStatusValue = Math.Max(mentalstatus.Low, Math.Max(mentalstatus.Medium, mentalstatus.High));
		mentalStatusValue = fuzzify.DefuzzifyMax(mentalstatus);
		float mentalStatusValueMean = fuzzify.DefuzzifyMean(mentalstatus);
		Debug.Log("Mental status value max: " + mentalStatusValue + " Mean: " + mentalStatusValueMean);

		currentState = UpdateState();
		Debug.Log("Current state: " + currentState);

		if(GameManager.Instance.useFAM) {
			//StartCoroutine(UpdateCoroutine());
		}
	}

	public IEnumerator UpdateCoroutine() {
		while (true) {
			UpdateFAM();
			yield return new WaitForSeconds(reactionTime);
		}
	}

	private void GetMonsterParams() {
		reactionTime = monsterBehaviour.GetReactionTime();
		crispHealth = monsterBehaviour.GetHealth();
		crispHunger = monsterBehaviour.GetHunger();
		crispSleepiness = monsterBehaviour.GetSleepiness();
		crispStress = monsterBehaviour.GetStress();
		crispGrudge = monsterBehaviour.GetGrudge();
	}

	private bool UpdateFuzzyParam(ref FuzzyVariable fuzzyVariable, float crispValue) {
		if (!fuzzyVariable.IsEqual(fuzzify.ToFuzzy(crispValue))) {
			fuzzyVariable = fuzzify.ToFuzzy(crispValue);
			return true;
		}
		return false;
	}

	private void ParamsToFuzzy(out bool physicalParamsChanged, out bool mentalParamsChanged) {
		physicalParamsChanged = false;
		mentalParamsChanged = false;
		
		if (UpdateFuzzyParam(ref healthFuzzy, crispHealth)) {
			physicalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref hungerFuzzy, crispHunger)) {
			physicalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref sleepinessFuzzy, crispSleepiness)) {
			physicalParamsChanged = true;
		}
		
		if (UpdateFuzzyParam(ref stressFuzzy, crispStress)) {
			mentalParamsChanged = true;
		}
		if (UpdateFuzzyParam(ref grudgeFuzzy, crispGrudge)) {
			mentalParamsChanged = true;
		}
	}

	public MonsterState UpdateState() {
		// Determine the NPC state based on the output values
		if (physicalStatusValue < lowThreshold) {
			if (mentalStatusValue < lowThreshold) {
				return MonsterState.berserk;
			}
			return MonsterState.replenish;
		} else {
			switch (mentalStatusValue)
			{
				case float n when (n < lowThreshold):
					return MonsterState.angry;
				case float n when (n >= lowThreshold && n < highThreshold):
					return MonsterState.annoyed;
				default:
					return MonsterState.calm;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void UpdateFAM() {
		Debug.Log("Updating FAM");
		GetMonsterParams();
		bool physicalParamsChanged;
		bool mentalParamsChanged;

		ParamsToFuzzy(out physicalParamsChanged, out mentalParamsChanged);
		if (!physicalParamsChanged && !mentalParamsChanged) {
			Debug.Log("No fuzzy variables changed");
			return;
		}
		if (physicalParamsChanged) {
			Debug.Log("Physical fuzzy variables changed");
			physicalFIS.Calculate();
			physicalStatusValue = fuzzify.DefuzzifyMax(physicalstatus);
			Debug.Log("Physical status value max: " + physicalStatusValue);
		}
		if (mentalParamsChanged) {
			Debug.Log("Mental fuzzy variables changed");
			Debug.Log("Mental status before fis: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
			mentalFIS.Calculate();
			Debug.Log("Mental status after fis: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
			mentalStatusValue = fuzzify.DefuzzifyMax(mentalstatus);
			Debug.Log("Mental status value max: " + mentalStatusValue);
		}
		if (currentState != UpdateState()) {
			Debug.Log("State changed from " + currentState + " to " + UpdateState());
			// Exit old state
			currentState = UpdateState();
			// Enter new state
		}
		// Stay in current state
	}
}