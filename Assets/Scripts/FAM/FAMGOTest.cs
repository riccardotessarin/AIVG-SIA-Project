using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class FAMGOTest : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{
		FuzzifyMain();
		//Main();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public static void FuzzifyMain()
	{
		/*
		Fuzzify temperature = new Fuzzify();
		temperature.AddMembershipFunction("Cold", new double[] { 0, 10, 15, 20 });
		temperature.AddMembershipFunction("Medium", new double[] { 15, 20, 25, 30 });
		temperature.AddMembershipFunction("Hot", new double[] { 25, 30, 35, 40 });

		double crispTemperature = 27.5;
		temperature.ShowFuzzify(crispTemperature);
		*/

		Fuzzify fuzzify = new Fuzzify();
		fuzzify.AddMembershipFunction("Low", new double[] { 0, 0, 20, 40 });
		fuzzify.AddMembershipFunction("Medium", new double[] { 20, 40, 60, 80 });
		fuzzify.AddMembershipFunction("High", new double[] { 60, 80, 100, 100 });

		double crispHealth = 25;
		double crispHunger = 25;
		double crispSleepiness = 25;
		//health.Fuzzify(crispHealth);
		FuzzyVariable healthFuzzy = fuzzify.ToFuzzy(crispHealth);
		Debug.Log("Health fuzzy: " + healthFuzzy.MembershipValues[FuzzyClass.low] + ", " + healthFuzzy.MembershipValues[FuzzyClass.medium] + ", " + healthFuzzy.MembershipValues[FuzzyClass.high]);

		FuzzyVariable hungerFuzzy = fuzzify.ToFuzzy(crispHunger);
		Debug.Log("Hunger fuzzy: " + hungerFuzzy.MembershipValues[FuzzyClass.low] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.medium] + ", " + hungerFuzzy.MembershipValues[FuzzyClass.high]);
		FuzzyVariable sleepinessFuzzy = fuzzify.ToFuzzy(crispSleepiness);
		Debug.Log("Sleepiness fuzzy: " + sleepinessFuzzy.MembershipValues[FuzzyClass.low] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.medium] + ", " + sleepinessFuzzy.MembershipValues[FuzzyClass.high]);

		// Make empty fuzzy variable for the output
		FuzzyVariable physicalstatus = new FuzzyVariable();

		double crispStress = 35;
		double crispGrudge = 10;
		FuzzyVariable stressFuzzy = fuzzify.ToFuzzy(crispStress);
		Debug.Log("Stress fuzzy: " + stressFuzzy.MembershipValues[FuzzyClass.low] + ", " + stressFuzzy.MembershipValues[FuzzyClass.medium] + ", " + stressFuzzy.MembershipValues[FuzzyClass.high]);
		//Debug.Log("Stress fuzzy: " + stressFuzzy.Low + ", " + stressFuzzy.Medium + ", " + stressFuzzy.High);
		FuzzyVariable grudgeFuzzy = fuzzify.ToFuzzy(crispGrudge);
		Debug.Log("Grudge fuzzy: " + grudgeFuzzy.MembershipValues[FuzzyClass.low] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.medium] + ", " + grudgeFuzzy.MembershipValues[FuzzyClass.high]);
		//Debug.Log("Grudge fuzzy: " + grudgeFuzzy.Low + ", " + grudgeFuzzy.Medium + ", " + grudgeFuzzy.High);

		// Make empty fuzzy variable for the output
		FuzzyVariable mentalstatus = new FuzzyVariable();

		FuzzyRule[] rules = new FuzzyRule[]
		{
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.low },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.high }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.medium },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.high }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.medium },
					new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.low },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.medium }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetClass = FuzzyClass.medium },
					new FuzzyCondition { Variable = grudgeFuzzy, SetClass = FuzzyClass.medium },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetClass = FuzzyClass.medium }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.high },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.high }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.medium }
				//Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "replenish" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = hungerFuzzy, SetClass = FuzzyClass.low },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetClass = FuzzyClass.low }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetClass = FuzzyClass.low }
			}
		};

		// Create the fuzzy inference system
		FuzzyInferenceSystem mentalFIS = new FuzzyInferenceSystem
		{
			Rules = rules,
			Inputs = new FuzzyVariable[] { healthFuzzy, hungerFuzzy, sleepinessFuzzy, stressFuzzy, grudgeFuzzy },
			Outputs = new FuzzyVariable[] { mentalstatus, physicalstatus }
		};

		// Perform the fuzzy inference
		mentalFIS.Calculate();

		Debug.Log("Mental status: " + mentalstatus.MembershipValues[FuzzyClass.low] + ", " + mentalstatus.MembershipValues[FuzzyClass.medium] + ", " + mentalstatus.MembershipValues[FuzzyClass.high]);
		Debug.Log("Physical status: " + physicalstatus.MembershipValues[FuzzyClass.low] + ", " + physicalstatus.MembershipValues[FuzzyClass.medium] + ", " + physicalstatus.MembershipValues[FuzzyClass.high]);

		// Get the output values
		double physicalStatusValue = Math.Max(physicalstatus.MembershipValues[FuzzyClass.low], Math.Max(physicalstatus.MembershipValues[FuzzyClass.medium], physicalstatus.MembershipValues[FuzzyClass.high]));
		Debug.Log("Physical status value: " + physicalStatusValue);

		// Get the output values
		double mentalStatusValue = Math.Max(mentalstatus.MembershipValues[FuzzyClass.low], Math.Max(mentalstatus.MembershipValues[FuzzyClass.medium], mentalstatus.MembershipValues[FuzzyClass.high]));
		//double mentalStatusValue = Math.Max(mentalstatus.Low, Math.Max(mentalstatus.Medium, mentalstatus.High));
		Debug.Log("Mental status value: " + mentalStatusValue);
	}

	/*
	public static void Main()
	{
		// Define the fuzzy variables
		FuzzyVariable health = new FuzzyVariable();
		FuzzyVariable hunger = new FuzzyVariable();
		FuzzyVariable sleepiness = new FuzzyVariable();
		FuzzyVariable stress = new FuzzyVariable();
		FuzzyVariable grudge = new FuzzyVariable();

		FuzzyVariable physicalstatus = new FuzzyVariable();
		FuzzyVariable mentalstatus = new FuzzyVariable();

		// Set the input values
		health.Low = 0;
		health.Medium = 0.5;
		health.High = 0;

		hunger.Low = 0;
		hunger.Medium = 0.5;
		hunger.High = 0;

		sleepiness.Low = 0;
		sleepiness.Medium = 0.5;
		sleepiness.High = 0;

		stress.Low = 0.5;
		stress.Medium = 0;
		stress.High = 0;

		grudge.Low = 0.5;
		grudge.Medium = 0;
		grudge.High = 0;

		// Define the fuzzy rules
		FuzzyRule[] rules = new FuzzyRule[]
		{
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = health, SetName = "low" },
					new FuzzyCondition { Variable = hunger, SetName = "high" },
					new FuzzyCondition { Variable = sleepiness, SetName = "high" }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "medium" }
				//Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "replenish" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = health, SetName = "low" },
					new FuzzyCondition { Variable = hunger, SetName = "low" },
					new FuzzyCondition { Variable = sleepiness, SetName = "low" }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "bad" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stress, SetName = "high" },
					new FuzzyCondition { Variable = grudge, SetName = "high" }
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "bad" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stress, SetName = "low" },
					new FuzzyCondition { Variable = grudge, SetName = "low" }
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "good" }
			}
		};

		// Create the fuzzy inference system
		FuzzyInferenceSystem fis = new FuzzyInferenceSystem
		{
			Rules = rules,
			Inputs = new FuzzyVariable[] { health, hunger, sleepiness, stress, grudge },
			Outputs = new FuzzyVariable[] { physicalstatus, mentalstatus }
		};

		// Perform the fuzzy inference
		fis.Calculate();

		// Get the output values
		double physicalStatusValue = Math.Max(physicalstatus.Low, Math.Max(physicalstatus.Medium, physicalstatus.High));
		double mentalStatusValue = Math.Max(mentalstatus.Low, Math.Max(mentalstatus.Medium, mentalstatus.High));

		// Determine the NPC state based on the output values
		if (physicalStatusValue > 0.5 && mentalStatusValue > 0.5)
		{
			Debug.Log("NPC state is berserk");
		}
		else if (physicalStatusValue > 0.5)
		{
			Debug.Log("NPC state is replenish");
		}
		else if (mentalStatusValue > 0.5)
		{
			Debug.Log("NPC state is annoyed");
		}
		else
		{
			Debug.Log("NPC state is calm");
		}
	}
	*/
}