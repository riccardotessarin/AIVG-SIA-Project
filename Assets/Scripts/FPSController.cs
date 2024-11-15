using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
	private Animator animator;
	public Camera playerCamera;
	private float walkSpeed = 2f;
	private float runSpeed = 6f;
	private float jumpPower = 5f;
	private float gravity = 10f;
 
 
	private float lookSpeed = 2f;
	private float lookXLimit = 45f;
	private float mouseX = 0.0f;
	private float mouseY = 0.0f;
	private float distanceZ = 0.9f;
 
	private Vector3 moveDirection = Vector3.zero;
 
	private bool canMove = true;
	private bool isFPS;
	
	private CharacterController characterController;

	private Transform target;  // The GameObject to rotate around

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		GameManager.GameStateChanged += GameManagerOnGameStateChanged;
		animator = GetComponent<Animator>();
	}

	private void OnDestroy() {
		GameManager.GameStateChanged -= GameManagerOnGameStateChanged;
	}

	// Subscribes to GameManager's GameStateChanged event
	private void GameManagerOnGameStateChanged(GameState state) {
		switch ( state ) {
			case GameState.Pause:
				Debug.Log("Game Pause");
				canMove = false;
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				break;
			default:
				Debug.Log("Game Play");
				canMove = true;
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				break;
		}
	}

	void Start()
	{
		characterController = GetComponent<CharacterController>();
		target = transform.GetChild(2);	// Gets the empty child gameobject placed at the player's head (for convenience)
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		isFPS = distanceZ < 1.0f;
	}

	void Update()
	{
		distanceZ += Input.GetAxis("Mouse ScrollWheel");
		distanceZ = Mathf.Clamp(distanceZ, 0.9f, 8.0f);
		// If camera distance is less than 1, we snap it in first person mode
		if (distanceZ < 1.0f) {
			mouseX = 0f;
			playerCamera.transform.localPosition = new Vector3(0f, 1.7f, 0.1f);
			isFPS = true;
		} else {
			isFPS = false;
		}
 
		#region Handles Movement
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
 
		// Press Left Shift to run
		bool isRunning = Input.GetKey(KeyCode.LeftShift);
		float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
		float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
		float movementDirectionY = moveDirection.y;
		moveDirection = (forward * curSpeedX) + (right * curSpeedY);
 
		if (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0) {
			animator.SetBool("isWalking", true);
			if (isRunning) {
				animator.SetBool("isRunning", true);
			} else {
				animator.SetBool("isRunning", false);
			}
		} else {
			animator.SetBool("isWalking", false);
			animator.SetBool("isRunning", false);
		}

		#endregion
 
		#region Handles Jumping
		if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
		{
			moveDirection.y = jumpPower;
		}
		else
		{
			moveDirection.y = movementDirectionY;
		}
 
		if (!characterController.isGrounded)
		{
			moveDirection.y -= gravity * Time.deltaTime;
		}
 
		#endregion
 
		characterController.Move(moveDirection * Time.deltaTime);

		#region Handles Rotation
 
		if (canMove)
		{
			mouseX += Input.GetAxis("Mouse X") * lookSpeed;
			mouseY -= Input.GetAxis("Mouse Y") * lookSpeed;
			mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);

			if (isFPS) {
				playerCamera.transform.localRotation = Quaternion.Euler(mouseY, 0, 0);
				transform.rotation *= Quaternion.Euler(0, mouseX, 0);
			} else {
				// Check for vertical input
				float verticalInput = Input.GetAxis("Vertical");

				if (Mathf.Abs(verticalInput) > 0.01f)
				{
					playerCamera.transform.SetParent(null);
					// Align the player's rotation with the camera's rotation
					transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
					playerCamera.transform.SetParent(transform, true);
					playerCamera.transform.localScale = Vector3.one;
				}

				Vector3 direction = new Vector3(0, 0, -distanceZ); // Adjust the distance from the target if needed
				Quaternion rotation = Quaternion.Euler(mouseY, mouseX, 0);
				playerCamera.transform.position = target.position + rotation * direction;
				playerCamera.transform.LookAt(target.position);
			}
		}
 
		#endregion
	}
}