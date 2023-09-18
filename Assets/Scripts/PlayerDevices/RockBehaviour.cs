using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class RockBehaviour : MonoBehaviour
{
	Rigidbody rb;
	private float speed = 30f;

	private void Awake() {
		StartCoroutine(WaitAndDestroy(30.0F));
		rb = GetComponent<Rigidbody>();
	}

	// This function destroys the device after a certain time if it doesn't hit anything
	private IEnumerator WaitAndDestroy(float waitTime) {
		while (true) {
			yield return new WaitForSecondsRealtime(waitTime);
			Destroy(gameObject);
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		rb.AddForce(transform.forward * speed, ForceMode.Impulse);
	}

	// Update is called once per frame
	void Update()
	{
		//transform.Translate(Vector3.forward * (Time.unscaledDeltaTime * 3f), Space.Self);
	}

	void FixedUpdate() {
	}

	private void OnCollisionEnter(Collision collision) {
		MonsterBehaviour monster = collision.gameObject.GetComponent<MonsterBehaviour>();
		float rockDamage = 5f;
		float stressDamage = 5f;
		float grudgeDamage = 2f;
		if (monster != null) {
			Debug.Log(monster + " hitted");
			
			monster.TakeDamage(rockDamage, stressDamage, grudgeDamage);
			Destroy(gameObject);
		}
	}
}
