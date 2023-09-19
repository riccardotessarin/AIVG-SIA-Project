using System;
using System.Collections.Generic;

public class FuzzySet
{
	public string Name { get; set; }
	public double Membership { get; set; }
}

public class FuzzyRule
{
	public List<FuzzySet> Inputs { get; set; }
	public FuzzySet Output { get; set; }
	//public List<FuzzySet> Outputs { get; set; }
}

public class FuzzyAssociativeMemory
{
	private List<FuzzyRule> rules;

	public FuzzyAssociativeMemory()
	{
		rules = new List<FuzzyRule>();
	}

	public void AddRule(FuzzyRule rule)
	{
		rules.Add(rule);
	}

	//public List<FuzzySet> Infer(List<FuzzySet> inputs)
	public FuzzySet Infer(List<FuzzySet> inputs)
	{
		//List<FuzzySet> outputs = new List<FuzzySet>();
		FuzzySet output = null;
		double maxMembership = 0;

		foreach (FuzzyRule rule in rules)
		{
			bool isMatch = true;

			for (int i = 0; i < inputs.Count; i++)
			{
				if (rule.Inputs[i].Membership < inputs[i].Membership)
				{
					isMatch = false;
					break;
				}
			}

			if (isMatch)
            {
                double minMembership = double.MaxValue;

                foreach (FuzzySet input in rule.Inputs)
                {
                    if (input.Membership < minMembership)
                    {
                        minMembership = input.Membership;
                    }
                }

                if (minMembership > maxMembership)
                {
                    maxMembership = minMembership;
                    output = rule.Output;
                }
            }
        }

        return output;
	/*
			if (isMatch)
			{
				outputs.AddRange(rule.Outputs);
			}
		}

		return outputs;
	*/
	}
}

