using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovementStatus {
	public Vector3 movementDirection;
	public float linearSpeed;
	public float angularSpeed;
}

// To be extended by all movement behaviours
public abstract class MovementBehaviour : MonoBehaviour {
	protected float sightRange;
	protected float sightAngle;
	[SerializeField]
	protected float steer;
	protected float backpedal;
	protected float gas;
	protected float brake;
	[SerializeField]
	protected float brakeAt;
	protected float stopAt;
	[SerializeField]
	protected float fleeRange;
	
	void Start()
	{
		sightRange = GameManager.Instance.sightRange;
		sightAngle = GameManager.Instance.sightAngle;
		steer = GameManager.Instance.steer;
		backpedal = GameManager.Instance.backpedal;
		gas = GameManager.Instance.gas;
		brake = GameManager.Instance.brake;
		brakeAt = GameManager.Instance.brakeAt;
		stopAt = GameManager.Instance.stopAt;
		fleeRange = GameManager.Instance.fleeRange;
	}

	public abstract Vector3 GetAcceleration (MovementStatus status);
}

public class Blender {
	public static Vector3 Blend (List<Vector3> vl) {
		Vector3 result = Vector3.zero;
		foreach (Vector3 v in vl) result += v;
		return result;
	}

	public static Vector3 DictBlend (Dictionary<MovementBehaviour, bool> mbDict, MovementStatus status) {
		List<Vector3> vl = mbDict.Where(kvp => kvp.Value).Select(kvp => kvp.Key.GetAcceleration(status)).ToList();
		return Blend (vl);
	}

	public static Vector3 Blend (Dictionary<MovementBehaviour, bool> mbDict, MovementStatus status) {
		Vector3 result = Vector3.zero;
		foreach (KeyValuePair<MovementBehaviour, bool> kvp in mbDict) {
			if (kvp.Value) result += kvp.Key.GetAcceleration(status);
		}
		return result;
	}
}

// Steer applies the given blended acceleration vector to the rigidbody
public class Driver {
	public static void Steer (Rigidbody body, MovementStatus status, Vector3 acceleration,
		                                float minV, float maxV, float maxSigma) {

		Vector3 tangentComponent = Vector3.Project (acceleration, status.movementDirection);
		Vector3 normalComponent = acceleration - tangentComponent;

		float tangentAcc = tangentComponent.magnitude * Vector3.Dot (tangentComponent.normalized, status.movementDirection);
		Vector3 right = Quaternion.Euler (0f, 90f, 0f) * status.movementDirection.normalized;
		float rotationAcc = normalComponent.magnitude * Vector3.Dot (normalComponent.normalized, right) * 360f;

		float t = Time.deltaTime;

		float tangentDelta = status.linearSpeed * t + 0.5f * tangentAcc * t * t;
		float rotationDelta = status.angularSpeed * t + 0.5f * rotationAcc * t * t;

		status.linearSpeed += tangentAcc * t;
		status.angularSpeed += rotationAcc * t;

		status.linearSpeed = Mathf.Clamp (status.linearSpeed, minV, maxV);
		status.angularSpeed = Mathf.Clamp (status.angularSpeed, -maxSigma, maxSigma);

		body.MovePosition (body.position + status.movementDirection * tangentDelta);
		body.MoveRotation (body.rotation * Quaternion.Euler (0f, rotationDelta, 0f));
	}
}

