using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public enum FuzzyClass { low, medium, high };

/// <summary>
/// Defines a fuzzy variable with its membership values
/// </summary>
public class FuzzyVariable
{
	public Dictionary<FuzzyClass, double> MembershipValues { get; set; }

	public FuzzyVariable()
	{
		MembershipValues = new Dictionary<FuzzyClass, double>() {
			{ FuzzyClass.low, 0 },
			{ FuzzyClass.medium, 0 },
			{ FuzzyClass.high, 0 }
		};
	}

	/*
	public double Low { get; set; }
	public double Medium { get; set; }
	public double High { get; set; }
	*/
}

/// <summary>
/// Defines a fuzzy rule with its conditions and conclusion
/// Uses the Evalute() method to determine if the rule is true and gives the conclusion
/// </summary>
public class FuzzyRule
{
	public FuzzyCondition[] Conditions { get; set; }
	public FuzzyConclusion Conclusion { get; set; }

	public bool Evaluate()
	{
		foreach (FuzzyCondition condition in Conditions)
		{
			if (!condition.Evaluate())
			{
				return false;
			}
		}
		// Return true only if all conditions are true, so the rule is true
		return true;
	}
}

public class FuzzyCondition
{
	public FuzzyVariable Variable { get; set; }
	public FuzzyClass SetName { get; set; }	// This will become a list to evaluate and/or conditions
	//public string SetName { get; set; }	// This will become a list to evaluate and/or conditions

	public bool Evaluate()
	{
		if (Variable == null)
		{
			return false;
		}

		switch (SetName)
		{
			case FuzzyClass.low:
				return Variable.MembershipValues[FuzzyClass.low] > 0;
			case FuzzyClass.medium:
				return Variable.MembershipValues[FuzzyClass.medium] > 0;
			case FuzzyClass.high:
				return Variable.MembershipValues[FuzzyClass.high] > 0;
			default:
				return false;
		}
	}
}

public class FuzzyConclusion
{
	public FuzzyVariable Variable { get; set; }
	public FuzzyClass SetName { get; set; }
	//public string SetName { get; set; }
}

public class FuzzyInferenceSystem
{
	public FuzzyRule[] Rules { get; set; }
	public FuzzyVariable[] Inputs { get; set; }
	public FuzzyVariable[] Outputs { get; set; }

	public void Calculate()
	{
		/*
		foreach (FuzzyVariable output in Outputs)
		{
			output.Low = 0;
			output.Medium = 0;
			output.High = 0;
		}
		*/

		FuzzyRule[] trueRules = Rules.Where(rule => rule.Evaluate()).ToArray();
		// We get the minimum membership value for each requested variable class
		foreach (FuzzyRule rule in trueRules) {
			rule.Conclusion.Variable.MembershipValues[rule.Conclusion.SetName] = 
			rule.Conditions.Min(condition => condition.Variable.MembershipValues[condition.SetName]);
		}
		foreach (FuzzyVariable output in Outputs) {
			for (int i = 0; i < Outputs.Length; i++) {
				Outputs[i] = trueRules.Where(rule => rule.Conclusion.Variable == Outputs[i]).Select(rule => rule.Conclusion.Variable).FirstOrDefault();
			}
		}
		
		/*
		foreach (FuzzyRule rule in Rules)
		{
			if (rule.Evaluate())
			{
				FuzzyVariable output = rule.Conclusion.Variable;
				Debug.Log("Output from rule conclusion: " + output.Low + ", " + output.Medium + ", " + output.High);
				switch (rule.Conclusion.SetName)
				{
					case "low":
						output.Low = Math.Max(output.Low, 1);
						break;
					case "medium":
						output.Medium = Math.Max(output.Medium, 1);
						break;
					case "high":
						output.High = Math.Max(output.High, 1);
						break;
				}
			}
		}
		*/
	}
}