using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestorationPointBehaviour : MonoBehaviour
{
	private GameObject NPC;
	public float targetRange = 4.0f;

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
				MonsterBehaviour monsterBehaviour = NPC.GetComponent<MonsterBehaviour>();
				if (monsterBehaviour.GetCurrentState() == MonsterState.replenish)
				{
					monsterBehaviour.Replenish();
					verticalEffect.Play();
					circleEffect.Play();
				}
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
