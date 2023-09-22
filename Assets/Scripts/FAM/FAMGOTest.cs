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

		/*
		double crispHealth = 75;
		double crispHunger = 25;
		double crispSleepiness = 25;
		//health.Fuzzify(crispHealth);
		FuzzyVariable healthFuzzy = fuzzify.ToFuzzy(crispHealth);
		Debug.Log("Health fuzzy: " + healthFuzzy.Low + ", " + healthFuzzy.Medium + ", " + healthFuzzy.High);

		FuzzyVariable hungerFuzzy = fuzzify.ToFuzzy(crispHunger);
		Debug.Log("Hunger fuzzy: " + hungerFuzzy.Low + ", " + hungerFuzzy.Medium + ", " + hungerFuzzy.High);
		FuzzyVariable sleepinessFuzzy = fuzzify.ToFuzzy(crispSleepiness);
		Debug.Log("Sleepiness fuzzy: " + sleepinessFuzzy.Low + ", " + sleepinessFuzzy.Medium + ", " + sleepinessFuzzy.High);

		// Make empty fuzzy variable for the output
		FuzzyVariable physicalstatus = new FuzzyVariable();

		FuzzyRule[] rules = new FuzzyRule[]
		{
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetName = "low" },
					new FuzzyCondition { Variable = hungerFuzzy, SetName = "high" },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetName = "high" }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "medium" }
				//Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "replenish" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = healthFuzzy, SetName = "low" },
					new FuzzyCondition { Variable = hungerFuzzy, SetName = "low" },
					new FuzzyCondition { Variable = sleepinessFuzzy, SetName = "low" }
				},
				Conclusion = new FuzzyConclusion { Variable = physicalstatus, SetName = "bad" }
			}
		};

		// Create the fuzzy inference system
		FuzzyInferenceSystem fis = new FuzzyInferenceSystem
		{
			Rules = rules,
			Inputs = new FuzzyVariable[] { healthFuzzy, hungerFuzzy, sleepinessFuzzy },
			Outputs = new FuzzyVariable[] { physicalstatus }
		};

		// Perform the fuzzy inference
		fis.Calculate();

		// Get the output values
		double physicalStatusValue = Math.Max(physicalstatus.Low, Math.Max(physicalstatus.Medium, physicalstatus.High));
		Debug.Log("Physical status value: " + physicalStatusValue);
		*/

		double crispStress = 35;
		double crispGrudge = 10;
		FuzzyVariable stressFuzzy = fuzzify.ToFuzzy(crispStress);
		Debug.Log("Stress fuzzy: " + stressFuzzy.Low + ", " + stressFuzzy.Medium + ", " + stressFuzzy.High);
		FuzzyVariable grudgeFuzzy = fuzzify.ToFuzzy(crispGrudge);
		Debug.Log("Grudge fuzzy: " + grudgeFuzzy.Low + ", " + grudgeFuzzy.Medium + ", " + grudgeFuzzy.High);

		// Make empty fuzzy variable for the output
		FuzzyVariable mentalstatus = new FuzzyVariable();

		FuzzyRule[] rules = new FuzzyRule[]
		{
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetName = "low" },
					new FuzzyCondition { Variable = grudgeFuzzy, SetName = "low" },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "high" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetName = "low" },
					new FuzzyCondition { Variable = grudgeFuzzy, SetName = "medium" },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "high" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetName = "medium" },
					new FuzzyCondition { Variable = grudgeFuzzy, SetName = "low" },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "medium" }
			},
			new FuzzyRule
			{
				Conditions = new FuzzyCondition[]
				{
					new FuzzyCondition { Variable = stressFuzzy, SetName = "medium" },
					new FuzzyCondition { Variable = grudgeFuzzy, SetName = "medium" },
				},
				Conclusion = new FuzzyConclusion { Variable = mentalstatus, SetName = "medium" }
			}
		};

		// Create the fuzzy inference system
		FuzzyInferenceSystem mentalFIS = new FuzzyInferenceSystem
		{
			Rules = rules,
			Inputs = new FuzzyVariable[] { stressFuzzy, grudgeFuzzy },
			Outputs = new FuzzyVariable[] { mentalstatus }
		};

		// Perform the fuzzy inference
		mentalFIS.Calculate();

		Debug.Log("Mental status: " + mentalstatus.Low + ", " + mentalstatus.Medium + ", " + mentalstatus.High);

		// Get the output values
		double mentalStatusValue = Math.Max(mentalstatus.Low, Math.Max(mentalstatus.Medium, mentalstatus.High));
		Debug.Log("Mental status value: " + mentalStatusValue);
	}

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
}