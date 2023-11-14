using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
	public Camera playerCamera;
	public float walkSpeed = 6f;
	public float runSpeed = 12f;
	public float jumpPower = 7f;
	public float gravity = 10f;
 
 
	public float lookSpeed = 2f;
	public float lookXLimit = 45f;
	private float mouseX = 0.0f;
	private float mouseY = 0.0f;
	private float distanceZ = 0.9f;
 
	Vector3 moveDirection = Vector3.zero;
 
	public bool canMove = true;
	private bool isFPS;
	
	CharacterController characterController;

	public Transform target;  // The GameObject to rotate around

	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		GameManager.GameStateChanged += GameManagerOnGameStateChanged;
	}

	private void OnDestroy() {
		GameManager.GameStateChanged -= GameManagerOnGameStateChanged;
	}

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
		target = transform.GetChild(2);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		isFPS = distanceZ < 1.0f;
	}

	void Update()
	{
		distanceZ += Input.GetAxis("Mouse ScrollWheel");
		distanceZ = Mathf.Clamp(distanceZ, 0.9f, 8.0f);
		if (distanceZ < 1.0f) {
			mouseX = 0f;
			playerCamera.transform.localPosition = new Vector3(0f, 1.7f, 0f);
			isFPS = true;
		} else {
			isFPS = false;
		}
 
		#region Handles Movment
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);
 
		// Press Left Shift to run
		bool isRunning = Input.GetKey(KeyCode.LeftShift);
		float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
		float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
		float movementDirectionY = moveDirection.y;
		moveDirection = (forward * curSpeedX) + (right * curSpeedY);
 
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
					playerCamera.transform.parent = null;
					// Align the player's rotation with the camera's rotation
					transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
					playerCamera.transform.parent = transform;
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