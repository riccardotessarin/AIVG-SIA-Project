using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorationPointBehaviour : MonoBehaviour
{

	/// <summary>
	/// OnTriggerStay is called once per frame for every Collider other
	/// that is touching the trigger.
	/// </summary>
	/// <param name="other">The other Collider involved in this collision.</param>
	void OnTriggerStay(Collider other)
	{
		GameObject monster = other.gameObject;
		if (monster.tag == "Monster")
		{
			monster.GetComponent<MonsterBehaviour>().Replenish();
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}
