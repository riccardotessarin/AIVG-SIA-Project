using UnityEngine;

public class FreeRoamingBehaviour : MovementBehaviour {
	//public Transform destination;

	public Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	private float time = 0f;
	private float timeToTarget = 5f;

	public float gas = 3f;
	//public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

    public override Vector3 GetAcceleration(MovementStatus status) {
        if (targetRandomPosition != null) {
			Vector3 verticalAdj = new Vector3(targetRandomPosition.x, transform.position.y, targetRandomPosition.z);
			Vector3 toDestination = verticalAdj - transform.position;
			
			if (stopAt > toDestination.magnitude || time > timeToTarget) {
				Debug.Log("Reached destination, new destination...");
				targetRandomPosition = Random.onUnitSphere*Random.Range(10f, 20f);
				time = 0f;
			}

			Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
			Vector3 normalComponent = (toDestination.normalized - tangentComponent);
			time += Time.deltaTime;
			return (tangentComponent * gas) + (normalComponent * steer);
			//return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);

		} else {
			return Vector3.zero;
		}
    }
	

	public void ResetTargetRandomPosition(Transform newTarget) {
		targetRandomPosition = new Vector3(newTarget.position.x, transform.position.y, newTarget.position.z);
	}
}

/*
if (toDestination.magnitude > stopAt && timeToTarget > time) {
				Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
				Vector3 normalComponent = (toDestination.normalized - tangentComponent);
				time += Time.deltaTime;
				return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
			} else {
				Debug.Log("Reached destination, new destination...");
				targetRandomPosition = UnityEngine.Random.onUnitSphere*UnityEngine.Random.Range(10f, 20f);
				time = 0f;
				return Vector3.zero;
			}


*/