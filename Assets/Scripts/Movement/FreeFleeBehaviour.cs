using Unity.VisualScripting;
using UnityEngine;

public class FreeFleeBehaviour: FreeRoamingBehaviour {

	public Transform fleeFrom;

	// Returns the flee acceleration if the player is within the flee range (annoyed status)
	public override Vector3 GetAcceleration(MovementStatus status) {
		if (fleeFrom != null) {
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
			return fleeAdj + base.GetAcceleration(status);
		} else {
			return base.GetAcceleration(status);
		}
	}

	public bool PlayerInRange(){
		return inRange;
	}

	// Returns the percentage of the flee range that the player is within
	public float GetPercentage(){
		return percentage;
	}
}