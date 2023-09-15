using UnityEngine;

public class FreeFleeBehaviour : MovementBehaviour {

	public Transform fleeFrom;

	//public Transform destination;
	public Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	private float time = 0f;
	private float timeToTarget = 5f;

	public float gas = 3f;
	public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	public float fleeRange = 10f;

    public override Vector3 GetAcceleration(MovementStatus status) {
		Debug.Log("Roaming to destination...");

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

			if (fleeFrom != null) {
				Vector3 fleeAdj = new Vector3();
				Vector3 vAdj = new Vector3 (fleeFrom.position.x, transform.position.y, fleeFrom.position.z);
				Vector3 fromDest = (transform.position - vAdj);
				if (fromDest.magnitude < fleeRange) {
					Vector3 tanComponent = Vector3.Project (fromDest.normalized, status.movementDirection);
					Vector3 normComponent = (fromDest.normalized - tanComponent);
					fleeAdj = (tanComponent * gas) + (normComponent * steer);
				} else {
					fleeAdj = Vector3.zero;
				}
				return fleeAdj + ((tangentComponent * gas) + (normalComponent * steer));
				//return fleeAdj + ((tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer));
			} else {
				return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
			}
		} else {
			return Vector3.zero;
		}
    }
}