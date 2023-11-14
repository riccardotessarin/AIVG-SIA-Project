using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(CharacterController))]
public class TPSController : MonoBehaviour
{
	public Camera playerCamera;
	private bool isFPS = true;
	public float walkSpeed = 6f;
	public float runSpeed = 12f;
	public float jumpPower = 7f;
	public float gravity = 10f;
 
 
	public float lookSpeed = 2f;
	public float lookXLimit = 45f;
 
 
	Vector3 moveDirection = Vector3.zero;
	//float rotationX = 0;
 
	public bool canMove = true;
 
	
	CharacterController characterController;

	public Transform target;  // The GameObject to rotate around
	public float rotationSpeed = 1.0f;  // Adjust this to control the rotation sensitivity
	private float mouseX, mouseY;

	void Start()
	{
		characterController = GetComponent<CharacterController>();
		target = transform.GetChild(2);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
 
	void Update()
	{
 
		#region Handles Movment
		Vector3 forward = transform.TransformDirection(Vector3.forward);
		Vector3 right = transform.TransformDirection(Vector3.right);

		forward = playerCamera.transform.TransformDirection(Vector3.forward);
		right = playerCamera.transform.TransformDirection(Vector3.right);
 
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
 
		#region Handles Rotation
		characterController.Move(moveDirection * Time.deltaTime);

		// Update player position without affecting the camera's vertical movement
		Vector3 newPosition = playerCamera.transform.position + new Vector3(moveDirection.x, 0, moveDirection.z) * Time.deltaTime;

		// Update camera height during jumps
		if (!characterController.isGrounded)
		{
			
			playerCamera.transform.position = new Vector3(playerCamera.transform.position.x, transform.position.y, playerCamera.transform.position.z);
		} else {
			playerCamera.transform.position = new Vector3(newPosition.x, playerCamera.transform.position.y, newPosition.z);
		}

	


		//playerCamera.transform.position += moveDirection * Time.deltaTime;

		mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
		mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;

 		// Limit the vertical rotation to a certain range to avoid flipping
		mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);

		if (canMove)
		{
			//rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
			//rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
			//playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
			//transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);


			if (isFPS) {
				playerCamera.transform.rotation = Quaternion.Euler(mouseY, 0, 0);
				transform.rotation *= Quaternion.Euler(0, mouseX, 0);
			} else {
				// Check for vertical input
				float verticalInput = Input.GetAxis("Vertical");

				if (Mathf.Abs(verticalInput) > 0.01f)
				{
					// Align the player's rotation with the camera's rotation
					//transform.rotation *= Quaternion.Euler(0, playerCamera.transform.localRotation.y * lookSpeed, 0);
					//playerCamera.transform.localRotation *= Quaternion.Euler (0.0f, -transform.rotation.y * lookSpeed, 0.0f);

					// Align the player's rotation with the camera's rotation
					transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
				}

				// Rotate the camera around the target based on mouse input
				playerCamera.transform.LookAt(target.position);
				playerCamera.transform.RotateAround(target.position, Vector3.up, mouseX);
				playerCamera.transform.RotateAround(target.position, playerCamera.transform.right, mouseY);
			}
		}
 
		#endregion



		// Reset mouse input values
		mouseX = 0;
		mouseY = 0;
		
	}

	void LateUpdate()
	{
		
	}
}