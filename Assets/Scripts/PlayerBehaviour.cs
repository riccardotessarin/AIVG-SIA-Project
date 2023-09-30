using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
	public float health = 100.0f;
	private Transform headTransform;
	private GameObject currentDevice;
	private GameObject rock;
	private GameObject dart;
	private GameObject axe;

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
		dart = Resources.Load("Prefabs/dart") as GameObject;
		axe = Resources.Load("Prefabs/axe") as GameObject;
		currentDevice = rock;
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			Debug.Log("Selected Rock");
			currentDevice = rock;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			Debug.Log("Selected Dart");
			currentDevice = dart;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			Debug.Log("Selected Axe");
			currentDevice = axe;
		}
		if (Input.GetKeyDown(KeyCode.T)) {
			Instantiate(currentDevice, headTransform.position + (Vector3.down + transform.forward) * 0.5f, headTransform.rotation);
		}
	}
}
