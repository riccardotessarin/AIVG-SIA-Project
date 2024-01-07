using Unity.VisualScripting;
using UnityEngine;

public class FreeFleeBehaviour : MovementBehaviour {

	public Transform fleeFrom;

	[SerializeField]
	protected Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	[SerializeField]
	protected Vector3 lastSeenPosition = new Vector3(20, 1, 20);
	protected float time = 0f;
	protected float timeToTarget = 5f;
	protected bool inRange = false;
	protected float percentage = 0f;
	[SerializeField]
	protected bool isFleeing = false;
	[SerializeField]
	protected bool isSeeking = false;
	protected float minRange = 0f;
	protected float maxRange = 20f;
	protected float maxTargetedRange = 3f;
	
	// Takes random positions and moves towards them for free roaming movement
	// If it is also fleeing (annoyed status) while roaming, it takes the flee acceleration into account
    public override Vector3 GetAcceleration(MovementStatus status) {
        if (targetRandomPosition != null) {
			Vector3 verticalAdj = new Vector3(targetRandomPosition.x, transform.position.y, targetRandomPosition.z);
			Vector3 toDestination = verticalAdj - transform.position;
			
			if (stopAt > toDestination.magnitude || time > timeToTarget) {
				//Debug.Log("Reached destination, new destination...");
				targetRandomPosition = GetRandomPosition();
				time = 0f;
			}

			Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
			Vector3 normalComponent = (toDestination.normalized - tangentComponent);
			time += Time.deltaTime;

			if (isFleeing) {
				return GetFleeAcceleration(status) + ((tangentComponent * gas) + (normalComponent * steer));
			} else {
				return (tangentComponent * gas) + (normalComponent * steer);
			}
		} else {
			return Vector3.zero;
		}
    }

	// Returns a random position within a range
	// If it is also seeking (angry or berserk) while free roaming after losing sight of the player,
	// it returns a random position within a smaller range around the last seen position
	public Vector3 GetRandomPosition(){
		if (isSeeking) {
			Debug.Log("Seeking targeted...");

			return lastSeenPosition + Random.onUnitSphere*Random.Range(minRange, maxTargetedRange);
		}
		return Random.onUnitSphere*Random.Range(10f, maxRange);
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

	public bool PlayerInRange(){
		return inRange;
	}

	// Returns the percentage of the flee range that the player is within
	public float GetPercentage(){
		return percentage;
	}

	public void SetIsFleeing(bool flee){
		isFleeing = flee;
	}

	// Mainly used from the outside to stop seeking
	public void SetIsSeeking(bool seek){
		Debug.Log("Giving up seeking...");

		isSeeking = seek;
	}

	// Returns the flee acceleration if the player is within the flee range (annoyed status)
	private Vector3 GetFleeAcceleration(MovementStatus status){
		Vector3 fleeAdj = new Vector3();
		Vector3 vAdj = new Vector3 (fleeFrom.position.x, transform.position.y, fleeFrom.position.z);
		Vector3 fromFleeTarg = (transform.position - vAdj);
		if (fromFleeTarg.magnitude < fleeRange) {
			Vector3 tanComponent = Vector3.Project (fromFleeTarg.normalized, status.movementDirection);
			Vector3 normComponent = (fromFleeTarg.normalized - tanComponent);
			fleeAdj = (tanComponent * gas) + (normComponent * steer);
			inRange = true;
			percentage = 1 - (fromFleeTarg.magnitude / fleeRange);
		} else {
			inRange = false;
			fleeAdj = Vector3.zero;
		}
		return fleeAdj;
	}
}