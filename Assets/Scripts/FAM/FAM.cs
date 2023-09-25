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
	public Dictionary<FuzzyClass, float> DefuzzifyValues { get; set; }

	public FuzzyVariable()
	{
		MembershipValues = new Dictionary<FuzzyClass, double>() {
			{ FuzzyClass.low, 0 },
			{ FuzzyClass.medium, 0 },
			{ FuzzyClass.high, 0 }
		};

		DefuzzifyValues = new Dictionary<FuzzyClass, float>() {
			{ FuzzyClass.low, 0f },
			{ FuzzyClass.medium, 0.5f },
			{ FuzzyClass.high, 1f }
		};
	}
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
	public FuzzyClass SetClass { get; set; }	// This will become a list to evaluate and/or conditions
	public FuzzyClass[] SetClasses { get; set; }

	public bool Evaluate()
	{
		if (Variable == null)
		{
			return false;
		}

		// This sets the fuzzyclass to the one with the highest membership value
		// We're doing the fuzzy OR operation here
		// If all is 0 or two membership values are the same, it will return the first one
		// This is not a problem since we just need the membership value and not the specific fuzzy class,
		// and if everything is 0 the switch will handle it
		if (SetClasses != null)
		{
			SetClass = Variable.MembershipValues
			.Where(kvp => SetClasses.Contains(kvp.Key))
			.OrderByDescending(kvp => kvp.Value)
			.Select(kvp => kvp.Key).FirstOrDefault();

			Debug.Log("SETCLASS IS: " + SetClass);
		}

		switch (SetClass)
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
	public FuzzyClass SetClass { get; set; }
}

public class FuzzyInferenceSystem
{
	public FuzzyRule[] Rules { get; set; }
	public FuzzyVariable[] Inputs { get; set; }
	public FuzzyVariable[] Outputs { get; set; }

	public void Calculate()
	{
		FuzzyRule[] trueRules = Rules.Where(rule => rule.Evaluate()).ToArray();
		// We get the minimum membership value for each requested variable class
		foreach (FuzzyRule rule in trueRules) {
			rule.Conclusion.Variable.MembershipValues[rule.Conclusion.SetClass] = 
			rule.Conditions.Min(condition => condition.Variable.MembershipValues[condition.SetClass]);
		}

		for (int i = 0; i < Outputs.Length; i++) {
			Outputs[i] = trueRules.Where(rule => rule.Conclusion.Variable == Outputs[i]).Select(rule => rule.Conclusion.Variable).FirstOrDefault();
		}
	}
}