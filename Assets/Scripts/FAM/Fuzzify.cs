using System;
using System.Collections.Generic;
using UnityEngine;

public class Fuzzify
{
	//public string Name { get; set; }
	public Dictionary<string, double[]> MembershipFunctions { get; set; }

	public Fuzzify()
	{
		MembershipFunctions = new Dictionary<string, double[]>();
	}

	public void AddMembershipFunction(string functionName, double[] membershipParams)
	{
		MembershipFunctions.Add(functionName, membershipParams);
	}

	
	/// <summary>
	/// Calculates the membership value of a crisp value in a trapezoidal membership function
	/// </summary>
	/// <param name="crispValue"></param>
	/// <param name="functionName"></param>
	/// <returns></returns>
	public double CalculateTrapezoidalMembershipValue(double crispValue, string functionName)
	{
		double[] membershipParams = MembershipFunctions[functionName];
		double a = membershipParams[0];
		double b = membershipParams[1];
		double c = membershipParams[2];
		double d = membershipParams[3];

		double membershipValue = 0.0;

		if (crispValue >= a && crispValue <= b)
		{
			membershipValue = (crispValue - a) / (b - a);
		}
		else if (crispValue >= b && crispValue <= c)
		{
			membershipValue = 1.0;
		}
		else if (crispValue >= c && crispValue <= d)
		{
			membershipValue = (d - crispValue) / (d - c);
		}

		return membershipValue;
	}

	private double CalculateTriangularMembershipValue(double crispValue, string functionName)
    {
        double[] membershipParams = MembershipFunctions[functionName];
        double a = membershipParams[0];
        double b = membershipParams[1];
        double c = membershipParams[2];

        double membershipValue = 0.0;

        if (crispValue >= a && crispValue <= b)
        {
            membershipValue = (crispValue - a) / (b - a);
        }
        else if (crispValue >= b && crispValue <= c)
        {
            membershipValue = (c - crispValue) / (c - b);
        }

        return membershipValue;
    }

	public void ShowFuzzify(double crispValue)
	{
		foreach (var membershipFunction in MembershipFunctions)
		{
			double membershipValue = CalculateTrapezoidalMembershipValue(crispValue, membershipFunction.Key);
			Debug.Log("Membership value for " + membershipFunction.Key + ": " + membershipValue);
		}
	}

	public FuzzyVariable ToFuzzy(double crispValue)
	{
		List<double> membershipValues = new List<double>();
		foreach (var membershipFunction in MembershipFunctions)
		{
			double membershipValue = CalculateTrapezoidalMembershipValue(crispValue, membershipFunction.Key);
			//Debug.Log("Membership value for " + membershipFunction.Key + ": " + membershipValue);
			membershipValues.Add(membershipValue);
		}
		FuzzyVariable fuzzyVariable = new FuzzyVariable();
		fuzzyVariable.MembershipValues[FuzzyClass.low] = membershipValues[0];
		fuzzyVariable.MembershipValues[FuzzyClass.medium] = membershipValues[1];
		fuzzyVariable.MembershipValues[FuzzyClass.high] = membershipValues[2];
		return fuzzyVariable;
	}
}