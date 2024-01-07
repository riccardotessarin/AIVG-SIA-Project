using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeSeekBehaviour : FreeRoamingBehaviour {

	public Transform destination;
	
    private bool sensedPlayer = false;
	private float maxSeekTime = 15f;
	private Coroutine seekCoroutine = null;

    private float maxPlayerDistance = 10f;

	public override Vector3 GetAcceleration(MovementStatus status) {
		if (CanSeePlayer()) {
			if (destination != null) {
				Vector3 verticalAdj = new Vector3 (destination.position.x, transform.position.y, destination.position.z);
				Vector3 toDestination = (verticalAdj - transform.position);

				if (toDestination.magnitude > stopAt) {
					Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
					Vector3 normalComponent = (toDestination.normalized - tangentComponent);
					return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
				} else {
					return Vector3.zero;
				}
			} else {
				return Vector3.zero;
			}
		} else {
			return base.GetAcceleration(status);
		}
	}


    // This checks if the NPC can see the player (returning true will start the seek behaviour)
	private bool CanSeePlayer(){
		RaycastHit hit;
		Vector3 headPosition = transform.GetChild(2).position;
		Vector3 playerHeadPosition = destination.GetChild(2).position;
		Vector3 rayDirection = playerHeadPosition - headPosition;	// Placed empty game objects at the heads positions for convenience
		if (Physics.Raycast(headPosition, rayDirection, out hit, maxPlayerDistance)
				&& Vector3.Angle(rayDirection, transform.forward) < 90f) {
			if (hit.collider.gameObject == destination.gameObject) {
				Debug.DrawRay(headPosition, rayDirection, Color.green);
				sensedPlayer = true;
				return true;
			}
		}
		Debug.DrawRay(headPosition, rayDirection, Color.red);

		// If we saw the player at least once, we start by going to the last known position
		if (sensedPlayer) {
			if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
			sensedPlayer = false;
			ResetTargetRandomPosition(destination, true);
			seekCoroutine = StartCoroutine(TargetedSeeking(maxSeekTime));
		}
		return false;
	}

	// This function sets a timer to stop seeking the player after a certain amount of time and resume normal free roaming
	private IEnumerator TargetedSeeking(float giveUpTime) {
		Debug.Log("Start seeking targeted...");
		yield return new WaitForSecondsRealtime(giveUpTime);
		SetIsSeeking(false);
	}

	public void StopSeekingCoroutine(){
		if (seekCoroutine != null) { StopCoroutine(seekCoroutine); }
	}
}