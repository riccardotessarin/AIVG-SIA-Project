using System;

/// <summary>
/// Defines a fuzzy variable with its membership values
/// </summary>
public class FuzzyVariable
{
	public double Low { get; set; }
	public double Medium { get; set; }
	public double High { get; set; }
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
	public string SetName { get; set; }

	public bool Evaluate()
	{
		if (Variable == null)
		{
			return false;
		}

		switch (SetName)
		{
			case "low":
				return Variable.Low > 0;
			case "medium":
				return Variable.Medium > 0;
			case "high":
				return Variable.High > 0;
			default:
				return false;
		}
	}
}

public class FuzzyConclusion
{
	public FuzzyVariable Variable { get; set; }
	public string SetName { get; set; }
}

public class FuzzyInferenceSystem
{
	public FuzzyRule[] Rules { get; set; }
	public FuzzyVariable[] Inputs { get; set; }
	public FuzzyVariable[] Outputs { get; set; }

	public void Calculate()
	{
		foreach (FuzzyVariable output in Outputs)
		{
			output.Low = 0;
			output.Medium = 0;
			output.High = 0;
		}

		foreach (FuzzyRule rule in Rules)
		{
			if (rule.Evaluate())
			{
				FuzzyVariable output = rule.Conclusion.Variable;
				
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
	}
}