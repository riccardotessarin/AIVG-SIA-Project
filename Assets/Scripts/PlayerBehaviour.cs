using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
	public float health = 100.0f;
	private Transform headTransform;
	private GameObject rock;

	public void TakeDamage(float damage) {
		// Only for demo purposes, the player can't die
		health = health - damage > 0 ? health - damage : 0;
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		headTransform = transform.GetChild(2);
	}

	// Start is called before the first frame update
	void Start()
	{
		rock = Resources.Load("Prefabs/stone-oval") as GameObject;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.T)) {
			Debug.Log("Throwing rock");
			Instantiate(rock, transform.GetChild(2).position + (Vector3.down + transform.forward) * 0.5f, headTransform.rotation);
		}
	}
}
