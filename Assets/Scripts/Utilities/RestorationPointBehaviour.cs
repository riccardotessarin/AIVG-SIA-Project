using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorationPointBehaviour : MonoBehaviour
{
	private GameObject NPC;
	public float targetRange = 4.0f;
	private bool replenished = false;

	private ParticleSystem circleEffect;
	private ParticleSystem verticalEffect;

	// Start is called before the first frame update
	void Start()
	{
		NPC = GameObject.FindWithTag("NPC");
		circleEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
		verticalEffect = transform.GetChild(1).GetComponent<ParticleSystem>();
		StartCoroutine(DistanceCoroutine());
	}

	private IEnumerator DistanceCoroutine() {
		while (true) {
			yield return new WaitForSeconds(1.0f);
			bool inRange = Vector3.Distance(transform.position, NPC.transform.position) <= targetRange;
			if (inRange)
			{
				transform.GetComponent<Renderer>().material.color = Color.green;
				if (!replenished)
				{
					replenished = true;
					NPC.GetComponent<MonsterBehaviour>().Replenish();
					verticalEffect.Play();
					circleEffect.Play();
				}
			}
			else
			{
				replenished = false;
				transform.GetComponent<Renderer>().material.color = Color.red;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
