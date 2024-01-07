using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeSeekBehaviour : FreeRoamingBehaviour {

	public Transform destination;
	[SerializeField]
	protected Vector3 lastSeenPosition = new Vector3(20, 1, 20);
	[SerializeField]
	protected bool isSeeking = false;
    private bool sensedPlayer = false;
	private float maxSeekTime = 15f;
	private Coroutine seekCoroutine = null;
	protected float minRange = 0f;
	protected float maxTargetedRange = 3f;
    private float maxPlayerDistance = 10f;

	public override Vector3 GetAcceleration(MovementStatus status) {
		if (destination != null && CanSeePlayer()) {
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

	// Returns a random position within a range
	// If it is also seeking (angry or berserk) while free roaming after losing sight of the player,
	// it returns a random position within a smaller range around the last seen position
	protected override Vector3 GetRandomPosition(){
		if (isSeeking) {
			Debug.Log("Seeking targeted...");

			return lastSeenPosition + Random.onUnitSphere*Random.Range(minRange, maxTargetedRange);
		}
		return base.GetRandomPosition();
	}

	// Can explicitly set a target position
	// If it is seeking (flag set to true), it will set the last seen position to the target position
	public void ResetTargetRandomPosition(Transform newTarget, bool seeking = false) {
		targetRandomPosition = new Vector3(newTarget.position.x, transform.position.y, newTarget.position.z);
		if (seeking) {
			isSeeking = true;
			lastSeenPosition = targetRandomPosition;
		}
	}

	// Mainly used from the outside to stop seeking
	public void SetIsSeeking(bool seek){
		Debug.Log("Giving up seeking...");

		isSeeking = seek;
	}
}