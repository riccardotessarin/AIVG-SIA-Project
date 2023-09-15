using UnityEngine;

public class SeekRestoreBehaviour : MovementBehaviour {

	public Transform destination;
	public Transform fleeFrom;

	public float gas = 3f;
	public float steer = 30f;
	public float brake = 20f;

	public float brakeAt = 5f;
	public float stopAt = 0.01f;

	public float fleeRange = 10f;

	public override Vector3 GetAcceleration (MovementStatus status) {
		if (destination != null) {
			Vector3 verticalAdj = new Vector3 (destination.position.x, transform.position.y, destination.position.z);
			Vector3 toDestination = (verticalAdj - transform.position);

			if (toDestination.magnitude > stopAt) {
				Vector3 tangentComponent = Vector3.Project (toDestination.normalized, status.movementDirection);
				Vector3 normalComponent = (toDestination.normalized - tangentComponent);
				
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
					return fleeAdj + ((tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer));
				} else {
					return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
				}
			} else {
				return Vector3.zero;
			}
		} else {
			return Vector3.zero;
		}
	}
}