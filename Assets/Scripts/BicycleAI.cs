using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BicycleAI : BicycleController {
	public PathEditor pathToFollow;

	public float cruiseSpeed = 7.0f;
	public float reachDistance = 1.0f;

	private int currentPathIndex = 0;

	private void calculateAISteering() {
		float distanceToNextPathNode = Vector3.Distance (pathToFollow.pathObjects [currentPathIndex].position, transform.position);
		float angleToTarget = getAngleToNextTarget ();
		steeringInput = Mathf.Clamp (angleToTarget / (maxTurnAngle / 2.0f), -1.0f, 1.0f);
		if (distanceToNextPathNode <= reachDistance) currentPathIndex++;

		Debug.DrawRay (transform.position, transform.forward * 2, Color.yellow);
		Debug.DrawRay (transform.position, pathToFollow.pathObjects [currentPathIndex].position - transform.position, Color.red);
	}

	private void calculateAIPedalling() {
		if (rigidBody.velocity.magnitude < cruiseSpeed) {
			powerInput = 1.0f;
			rearBrake = false;
		} else {
			powerInput = 0.0f;
			rearBrake = true;
		}
	}

	private float getAngleToNextTarget() {
		Vector3 flatForward = transform.forward;
		Vector3 flatTargetForward = pathToFollow.pathObjects [currentPathIndex].position - transform.position;
		flatForward.y = 0;
		flatTargetForward.y = 0;

		float angleToTarget = Vector3.Angle (flatForward, flatTargetForward);

		Vector3 crossAngle = Vector3.Cross(flatForward, flatTargetForward);
		if (crossAngle.y < 0) angleToTarget = -angleToTarget;
		return angleToTarget;
	}

	protected override void getInputs() {
		calculateAIPedalling ();
		calculateAISteering ();

		float turnSqrtCoef = Mathf.Sqrt (4f * rigidBody.velocity.magnitude) + 0.01f;
		float turnQuota = Mathf.Clamp(1f / turnSqrtCoef - 0.06f, 0f, 1f);
		turningAngle = steeringInput * maxTurnAngle * turnQuota;
	}
}
