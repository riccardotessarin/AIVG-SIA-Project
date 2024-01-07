using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeRoamingBehaviour : MovementBehaviour {

	[SerializeField]
	protected Vector3 targetRandomPosition = new Vector3(20, 1, 20);
	protected float time = 0f;
	protected float timeToTarget = 5f;
	protected float maxRange = 20f;
	
	// Takes random positions and moves towards them for free roaming movement
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

			return (tangentComponent * gas) + (normalComponent * steer);
		} else {
			return Vector3.zero;
		}
    }

	// Returns a random position within a range
	protected virtual Vector3 GetRandomPosition(){
		return Random.onUnitSphere*Random.Range(10f, maxRange);
	}
}
