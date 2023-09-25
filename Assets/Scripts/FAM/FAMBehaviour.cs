using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FAMBehaviour : MonoBehaviour
{
	private Fuzzify fuzzify;
	double crispHealth = 25;
	double crispHunger = 25;
	double crispSleepiness = 25;
	double crispStress = 75;
	double crispGrudge = 15;
	FuzzyVariable healthFuzzy;
	FuzzyVariable hungerFuzzy;
	FuzzyVariable sleepinessFuzzy;
	FuzzyVariable stressFuzzy;
	FuzzyVariable grudgeFuzzy;
	FuzzyVariable physicalstatus;
	FuzzyVariable mentalstatus;
	FuzzyRule[] rules;
	FuzzyInferenceSystem FIS;
	float lowThreshold = 0.3f;
	float highThreshold = 0.7f;

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		fuzzify = new Fuzzify();
		fuzzify.AddMembershipFunction("Low", new double[] { 0, 0, 20, 40 });
		fuzzify.AddMembershipFunction("Medium", new double[] { 20, 40, 60, 80 });
		fuzzify.AddMembershipFunction("High", new double[] { 60, 80, 100, 100 });
	}

	// Start is called before the first frame update
	void Start()
	{
		
		//health.Fuzzify(crispHealth);
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

		rules = new FuzzyRule[]
		{
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

		// Create the fuzzy inference system
		FIS = new FuzzyInferenceSystem
		{
			Rules = rules,
			Inputs = new FuzzyVariable[] { healthFuzzy, hungerFuzzy, sleepinessFuzzy, stressFuzzy, grudgeFuzzy },
			Outputs = new FuzzyVariable[] { mentalstatus, physicalstatus }
		};

		// Perform the fuzzy inference
		FIS.Calculate();

		Debug.Log("Mental status: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
		Debug.Log("Physical status: " + physicalstatus.MembershipValues[FuzzyClass.low] + ", " + physicalstatus.MembershipValues[FuzzyClass.medium] + ", " + physicalstatus.MembershipValues[FuzzyClass.high]);

		// Get the output values
		//double physicalStatusValue = Math.Max(physicalstatus.MembershipValues[FuzzyClass.low], Math.Max(physicalstatus.MembershipValues[FuzzyClass.medium], physicalstatus.MembershipValues[FuzzyClass.high]));
		float physicalStatusValue = fuzzify.DefuzzifyMax(physicalstatus);
		float physicalStatusValueMean = fuzzify.DefuzzifyMean(physicalstatus);
		Debug.Log("Physical status value max: " + physicalStatusValue + " Mean: " + physicalStatusValueMean);

		// Get the output values
		//double mentalStatusValue = Math.Max(mentalstatus.MembershipValues[FuzzyClass.low], Math.Max(mentalstatus.MembershipValues[FuzzyClass.medium], mentalstatus.MembershipValues[FuzzyClass.high]));
		//double mentalStatusValue = Math.Max(mentalstatus.Low, Math.Max(mentalstatus.Medium, mentalstatus.High));
		float mentalStatusValue = fuzzify.DefuzzifyMax(mentalstatus);
		float mentalStatusValueMean = fuzzify.DefuzzifyMean(mentalstatus);
		Debug.Log("Mental status value max: " + mentalStatusValue + " Mean: " + mentalStatusValueMean);

		// Determine the NPC state based on the output values
		if (physicalStatusValue < lowThreshold) {
			Debug.Log("NPC state is replenish");
			if (mentalStatusValue < lowThreshold) {
				Debug.Log("NPC state is berserk");
			}
		} else {
			switch (mentalStatusValue)
			{
				case float n when (n < lowThreshold):
					Debug.Log("NPC state is angry");
					break;
				case float n when (n >= lowThreshold && n < highThreshold):
					Debug.Log("NPC state is annoyed");
					break;
				default:
					Debug.Log("NPC state is calm");
					break;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}