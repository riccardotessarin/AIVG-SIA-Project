using System;
using System.Collections.Generic;

public class FuzzySet
{
	public string Name { get; set; }
	public double Membership { get; set; }
}

public class BaseFuzzyRule
{
	public List<FuzzySet> Inputs { get; set; }
	public FuzzySet Output { get; set; }
	public List<FuzzySet> Outputs { get; set; }
	public double Weight { get; set; }
}

public class FuzzyAssociativeMemory
{
	private List<BaseFuzzyRule> rules;

	public FuzzyAssociativeMemory()
	{
		rules = new List<BaseFuzzyRule>();
	}

	public void AddRule(BaseFuzzyRule rule)
	{
		rules.Add(rule);
	}

	//public List<FuzzySet> Infer(List<FuzzySet> inputs)
	public FuzzySet Infer(List<FuzzySet> inputs)
	{
		//List<FuzzySet> outputs = new List<FuzzySet>();
		FuzzySet output = null;
		double maxMembership = 0;

		foreach (BaseFuzzyRule rule in rules)
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
				foreach (FuzzySet output in rule.Outputs)
				{
					outputs.Add(new FuzzySet { Name = output.Name, Membership = output.Membership });
				}
			}
		}

		return outputs;
	*/

	
	/*
			if (isMatch)
			{
				outputs.AddRange(rule.Outputs);
			}
		}

		return outputs;
	*/
	}

	public List<FuzzySet> WeightedInfer(List<FuzzySet> inputs)
	{
		List<FuzzySet> outputs = new List<FuzzySet>();

		foreach (BaseFuzzyRule rule in rules)
		{
			double ruleWeightedMembership = 1.0;

			for (int i = 0; i < inputs.Count; i++)
			{
				double inputMembership = inputs[i].Membership;
				double ruleInputMembership = rule.Inputs[i].Membership;

				ruleWeightedMembership = Math.Min(ruleWeightedMembership, Math.Min(inputMembership, ruleInputMembership));
			}

			if (ruleWeightedMembership > 0)
			{
				foreach (FuzzySet output in rule.Outputs)
				{
					double outputMembership = output.Membership * ruleWeightedMembership;

					FuzzySet existingOutput = outputs.Find(o => o.Name == output.Name);
					if (existingOutput != null)
					{
						existingOutput.Membership = Math.Max(existingOutput.Membership, outputMembership);
					}
					else
					{
						outputs.Add(new FuzzySet { Name = output.Name, Membership = outputMembership });
					}
				}
			}
		}

		return outputs;
	}
}


/*


public class Program
{
	public static void Main(string[] args)
	{
		FuzzyAssociativeMemory fam = new FuzzyAssociativeMemory();

		// Define fuzzy rules
		FuzzyRule rule1 = new FuzzyRule
		{
			Inputs = new List<FuzzySet>
			{
				new FuzzySet { Name = "Input1", Membership = 0.7 },
				new FuzzySet { Name = "Input2", Membership = 0.5 },
				new FuzzySet { Name = "Input3", Membership = 0.8 }
			},
			Output = new FuzzySet { Name = "Output", Membership = 0.9 }
		};

		FuzzyRule rule2 = new FuzzyRule
		{
			Inputs = new List<FuzzySet>
			{
				new FuzzySet { Name = "Input1", Membership = 0.4 },
				new FuzzySet { Name = "Input2", Membership = 0.6 },
				new FuzzySet { Name = "Input3", Membership = 0.7 }
			},
			Output = new FuzzySet { Name = "Output", Membership = 0.6 }
		};

		// Add fuzzy rules to the fuzzy associative memory
		fam.AddRule(rule1);
		fam.AddRule(rule2);

		// Perform inference
		List<FuzzySet> inputs = new List<FuzzySet>
		{
			new FuzzySet { Name = "Input1", Membership = 0.6 },
			new FuzzySet { Name = "Input2", Membership = 0.4 },
			new FuzzySet { Name = "Input3", Membership = 0.7 }
		};

		FuzzySet output = fam.Infer(inputs);

		// Print the inferred output
		Console.WriteLine("Output: " + output.Name + ", Membership: " + output.Membership);
	}
}







// Example of use

FuzzyAssociativeMemory fam = new FuzzyAssociativeMemory();

// Define fuzzy rules
FuzzyRule rule1 = new FuzzyRule
{
	Inputs = new List<FuzzySet>
	{
		new FuzzySet { Name = "Input1", Membership = 0.7 },
		new FuzzySet { Name = "Input2", Membership = 0.5 }
	},
	Outputs = new List<FuzzySet>
	{
		new FuzzySet { Name = "Output1", Membership = 0.9 },
		new FuzzySet { Name = "Output2", Membership = 0.8 }
	}
};

// Add fuzzy rules to the fuzzy associative memory
fam.AddRule(rule1);

// Perform inference
List<FuzzySet> inputs = new List<FuzzySet>
{
	new FuzzySet { Name = "Input1", Membership = 0.6 },
	new FuzzySet { Name = "Input2", Membership = 0.4 }
};

List<FuzzySet> outputs = fam.Infer(inputs);

// Print the inferred outputs
foreach (FuzzySet output in outputs)
{
	Console.WriteLine("Output: " + output.Name + ", Membership: " + output.Membership);
}
*/