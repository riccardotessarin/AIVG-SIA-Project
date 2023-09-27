using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fuzzify
{
	//public string Name { get; set; }
	public Dictionary<FuzzyClass, float[]> MembershipFunctions { get; set; }

	public Fuzzify()
	{
		MembershipFunctions = new Dictionary<FuzzyClass, float[]>();
	}

	public void AddMembershipFunction(FuzzyClass functionName, float[] membershipParams)
	{
		MembershipFunctions.Add(functionName, membershipParams);
	}

	
	/// <summary>
	/// Calculates the membership value of a crisp value in a trapezoidal membership function
	/// </summary>
	/// <param name="crispValue"></param>
	/// <param name="functionName"></param>
	/// <returns></returns>
	public float CalculateTrapezoidalMembershipValue(float crispValue, FuzzyClass functionName)
	{
		float[] membershipParams = MembershipFunctions[functionName];
		float a = membershipParams[0];
		float b = membershipParams[1];
		float c = membershipParams[2];
		float d = membershipParams[3];

		float membershipValue = 0.0f;

		if (crispValue >= a && crispValue < b)
		{
			membershipValue = (crispValue - a) / (b - a);
		}
		else if (crispValue >= b && crispValue <= c)
		{
			membershipValue = 1.0f;
		}
		else if (crispValue > c && crispValue <= d)
		{
			membershipValue = (d - crispValue) / (d - c);
		}

		return membershipValue;
	}

	private float CalculateTriangularMembershipValue(float crispValue, FuzzyClass functionName)
	{
		float[] membershipParams = MembershipFunctions[functionName];
		float a = membershipParams[0];
		float b = membershipParams[1];
		float c = membershipParams[2];

		float membershipValue = 0.0f;

		if (crispValue >= a && crispValue < b)
		{
			membershipValue = (crispValue - a) / (b - a);
		}
		else if (crispValue == b)
		{
			membershipValue = 1.0f;
		}
		else if (crispValue > b && crispValue <= c)
		{
			membershipValue = (c - crispValue) / (c - b);
		}

		return membershipValue;
	}

	public void ShowFuzzify(float crispValue)
	{
		foreach (var membershipFunction in MembershipFunctions)
		{
			float membershipValue = CalculateTrapezoidalMembershipValue(crispValue, membershipFunction.Key);
			Debug.Log("Membership value for " + membershipFunction.Key + ": " + membershipValue);
		}
	}

	public FuzzyVariable ToFuzzy(float crispValue)
	{
		FuzzyVariable fuzzyVariable = new FuzzyVariable();
		foreach (var membershipFunction in MembershipFunctions)
		{
			if (membershipFunction.Value.Length == 3) {
				fuzzyVariable.MembershipValues[membershipFunction.Key] = CalculateTriangularMembershipValue(crispValue, membershipFunction.Key);
			} else if (membershipFunction.Value.Length == 4) {
				fuzzyVariable.MembershipValues[membershipFunction.Key] = CalculateTrapezoidalMembershipValue(crispValue, membershipFunction.Key);	
			} else {
				Debug.LogError("Invalid membership function");
			}
		}
		return fuzzyVariable;
	}

	public float DefuzzifyMax(FuzzyVariable fuzzyVariable)
	{
		float max = 0.0f;
		FuzzyClass maxFuzzyClass = FuzzyClass.low;

		// If there are no duplicate membership values, return the max value
		if ( fuzzyVariable.MembershipValues.GroupBy(x => x.Value).Where(x => x.Count() > 1) == null ) {
			maxFuzzyClass = fuzzyVariable.MembershipValues.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
			return fuzzyVariable.DefuzzifyValues[maxFuzzyClass];
		}

		// If we have duplicate membership values, we choose the one with the highest defuzzify value (optimistic) or the lowest (pessimistic)
		foreach (var membershipValue in fuzzyVariable.MembershipValues) {
			if (membershipValue.Value > max) {
				max = (float) membershipValue.Value;
				maxFuzzyClass = membershipValue.Key;
			} else if (membershipValue.Value == max) {
				if (fuzzyVariable.DefuzzifyValues[membershipValue.Key] > fuzzyVariable.DefuzzifyValues[maxFuzzyClass]) {
					max = (float) membershipValue.Value;
					maxFuzzyClass = membershipValue.Key;
				}
			}
		}

		return fuzzyVariable.DefuzzifyValues[maxFuzzyClass];

		//return (float) fuzzyVariable.MembershipValues.Max(kvp => kvp.Value);
		/*
		foreach (var membershipValue in fuzzyVariable.MembershipValues)
		{
			if (membershipValue.Value > max)
			{
				max = membershipValue.Value;
				crispValue = (float)membershipValue.Key;
			}
		}
		return crispValue;
		*/
	}

	public float DefuzzifyMean(FuzzyVariable fuzzyVariable) {
		float sum = 0.0f;
		float total = 0.0f;
		foreach (var membershipValue in fuzzyVariable.MembershipValues)
		{
			sum += (float)membershipValue.Value * fuzzyVariable.DefuzzifyValues[membershipValue.Key];
			total += (float)membershipValue.Value;
		}
		return sum / total;
	}
}