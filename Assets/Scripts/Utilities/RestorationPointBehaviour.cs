using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorationPointBehaviour : MonoBehaviour
{
	private GameObject NPC;
	public float targetRange = 5.0f;
	private bool replenished = false;

	// Start is called before the first frame update
	void Start()
	{
		NPC = GameObject.FindWithTag("NPC");
		StartCoroutine(DistanceCoroutine());
	}

	private IEnumerator DistanceCoroutine() {
		while (true) {
			yield return new WaitForSeconds(1.0f);
			bool inRange = Vector3.Distance(transform.position, NPC.transform.position) <= targetRange;
			if (inRange)
			{
				transform.parent.GetComponent<Renderer>().material.color = Color.green;
				if (!replenished)
				{
					replenished = true;
					NPC.GetComponent<MonsterBehaviour>().Replenish();
				}
			}
			else
			{
				replenished = false;
				transform.parent.GetComponent<Renderer>().material.color = Color.red;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
