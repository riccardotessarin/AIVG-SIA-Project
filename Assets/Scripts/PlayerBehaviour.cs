using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour {
	private float maxHealth = 100.0f;
	private float health;
	public ScreenFlash screenFlash;
	public HealthBar healthBar;
	private Transform headTransform;
	private Transform playerCamera;
	private GameObject currentDevice;
	private GameObject rock;
	private GameObject dart;
	private GameObject axe;

	private Animator animator;

	public void TakeDamage(float damage) {
		health = health - damage > 0 ? health - damage : 0;
		screenFlash.StartFlash(0.2f, 0.5f, Color.red);
		healthBar.SetHealth(health);
		// Only for demo purposes, the player can't die in easy mode
		if (health == 0 && !GameManager.Instance.easyMode) {
			GameManager.Instance.GameOver();
		}
	}

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		health = maxHealth;
		headTransform = transform.GetChild(2);
		playerCamera = transform.GetChild(3);
		healthBar.SetMaxHealth(maxHealth);
		animator = GetComponent<Animator>();
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
		// Don't take inputs if the game is paused or ended
		if (GameManager.GameIsPaused || GameManager.GameHasEnded) {
			return;
		}
		
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			GameManager.Instance.SetGameMessage("Rock Equipped");
			currentDevice = rock;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			GameManager.Instance.SetGameMessage("Dart Equipped");
			currentDevice = dart;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			GameManager.Instance.SetGameMessage("Axe Equipped");
			currentDevice = axe;
		}
		if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.Mouse0)) {
			animator.SetTrigger("isAttacking");
			Quaternion applyRotation = Quaternion.Euler(playerCamera.eulerAngles.x, headTransform.eulerAngles.y, headTransform.eulerAngles.z);
			Instantiate(currentDevice, headTransform.position + (Vector3.down + transform.forward) * 0.5f, applyRotation);
		}
	}
}
