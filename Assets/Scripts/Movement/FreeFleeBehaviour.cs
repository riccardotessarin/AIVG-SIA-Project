using Unity.VisualScripting;
using UnityEngine;

public class FreeFleeBehaviour : MovementBehaviour {

	public Transform fleeFrom;

	//public Transform destination;
	[SerializeField]
	private Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	[SerializeField]
	private Vector3 lastSeenPosition = new Vector3(20, 1, 20);
	private float time = 0f;
	private float timeToTarget = 5f;
	private bool inRange = false;
	private float percentage = 0f;
	[SerializeField]
	private bool isFleeing = false;
	[SerializeField]
	private bool isSeeking = false;
	private float minRange = 0f;
	private float maxRange = 20f;
	private float maxTargetedRange = 3f;

	/*
	public float gas = 3f;
	//public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	public float fleeRange = 5f;
	*/
	

    public override Vector3 GetAcceleration(MovementStatus status) {
        if (targetRandomPosition != null) {
			Vector3 verticalAdj = new Vector3(targetRandomPosition.x, transform.position.y, targetRandomPosition.z);
			Vector3 toDestination = verticalAdj - transform.position;
			
			if (stopAt > toDestination.magnitude || time > timeToTarget) {
				Debug.Log("Reached destination, new destination...");
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
			/*
			if (fleeFrom != null) {
				Vector3 fleeAdj = new Vector3();
				Vector3 vAdj = new Vector3 (fleeFrom.position.x, transform.position.y, fleeFrom.position.z);
				Vector3 fromDest = (transform.position - vAdj);
				if (fromDest.magnitude < fleeRange) {
					Vector3 tanComponent = Vector3.Project (fromDest.normalized, status.movementDirection);
					Vector3 normComponent = (fromDest.normalized - tanComponent);
					fleeAdj = (tanComponent * gas) + (normComponent * steer);
					inRange = true;
					percentage = 1 - (fromDest.magnitude / fleeRange);
				} else {
					inRange = false;
					fleeAdj = Vector3.zero;
				}
				return fleeAdj + ((tangentComponent * gas) + (normalComponent * steer));
				//return fleeAdj + ((tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer));
			} else {
				return (tangentComponent * gas) + (normalComponent * steer);
				//return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
			}
			*/
		} else {
			return Vector3.zero;
		}
    }

	public Vector3 GetRandomPosition(){
		if (isSeeking) {
			Debug.Log("Seeking targeted...");

			return lastSeenPosition + Random.onUnitSphere*Random.Range(minRange, maxTargetedRange);
		}
		return Random.onUnitSphere*Random.Range(10f, maxRange);
	}

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

	public float GetPercentage(){
		return percentage;
	}

	public void SetIsFleeing(bool flee){
		isFleeing = flee;
	}

	public void SetIsSeeking(bool seek){
		Debug.Log("Giving up seeking...");

		isSeeking = seek;
	}

	private Vector3 GetFleeAcceleration(MovementStatus status){
		Vector3 fleeAdj = new Vector3();
		Vector3 vAdj = new Vector3 (fleeFrom.position.x, transform.position.y, fleeFrom.position.z);
		Vector3 fromDest = (transform.position - vAdj);
		if (fromDest.magnitude < fleeRange) {
			Vector3 tanComponent = Vector3.Project (fromDest.normalized, status.movementDirection);
			Vector3 normComponent = (fromDest.normalized - tanComponent);
			fleeAdj = (tanComponent * gas) + (normComponent * steer);
			inRange = true;
			percentage = 1 - (fromDest.magnitude / fleeRange);
		} else {
			inRange = false;
			fleeAdj = Vector3.zero;
		}
		return fleeAdj;
	}
}