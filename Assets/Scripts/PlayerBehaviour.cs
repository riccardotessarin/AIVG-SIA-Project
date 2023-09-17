using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
	public float health = 100.0f;

	public void TakeDamage(float damage) {
		// Only for demo purposes, the player can't die
		health = health - damage > 0 ? health - damage : 0;
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
