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
 
 
	Vector3 moveDirection = Vector3.zero;
	float rotationX = 0;
 
	public bool canMove = true;
 
	
	CharacterController characterController;

	public Transform target;  // The GameObject to rotate around

	void Start()
	{
		characterController = GetComponent<CharacterController>();
		target = transform.GetChild(2);
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
 
 	float mouseX = 0.0f;
	float mouseY = 0.0f;

	void Update()
	{
 
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
 
		#region Handles Rotation
		characterController.Move(moveDirection * Time.deltaTime);
 
		if (canMove)
		{
			mouseX += Input.GetAxis("Mouse X") * lookSpeed;
			mouseY -= Input.GetAxis("Mouse Y") * lookSpeed;
			mouseY = Mathf.Clamp(mouseY, -lookXLimit, lookXLimit);

			if (false) {
				//rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
				//rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
				//playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
				//transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

				playerCamera.transform.localRotation = Quaternion.Euler(mouseY, 0, 0);
				transform.rotation *= Quaternion.Euler(0, mouseX, 0);
			} else {
				// Check for vertical input
				float verticalInput = Input.GetAxis("Vertical");

				if (Mathf.Abs(verticalInput) > 0.01f)
				{
					// Align the player's rotation with the camera's rotation
					//transform.rotation *= Quaternion.Euler(0, playerCamera.transform.localRotation.y * lookSpeed, 0);
					//playerCamera.transform.localRotation *= Quaternion.Euler (0.0f, -transform.rotation.y * lookSpeed, 0.0f);

					playerCamera.transform.parent = null;
					// Align the player's rotation with the camera's rotation
					transform.rotation = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0);
					playerCamera.transform.parent = transform;

				}

				// Rotate the camera around the target based on mouse input
				playerCamera.transform.LookAt(target.position);
				playerCamera.transform.RotateAround(target.position, Vector3.up, mouseX);
				playerCamera.transform.RotateAround(target.position, playerCamera.transform.right, mouseY);

				mouseX = 0f;
				mouseY = 0f;
			}
			
		}
 
		#endregion
	}
}