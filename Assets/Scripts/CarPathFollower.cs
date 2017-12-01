using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof (CarAIControl))]
[RequireComponent(typeof (CarController))]
public class CarPathFollower : MonoBehaviour {
	public PathEditor pathToFollow;
	public float minReachDistance = 1.0f;

	private int currentPathIndex = 0;
	private CarAIControl carAIControl;
	private CarController carController;

	// Use this for initialization
	void Start () {
		carAIControl = GetComponent<CarAIControl>();
		carController = GetComponent<CarController>();
		//carAIControl.SetTarget (getCurrentTarget ());
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time > 1) {
			float distanceToNextPathNode = getDistanceToTarget ();
			if (distanceToNextPathNode <= minReachDistance) {
				currentPathIndex = getNextPathIndex ();
			}
			carAIControl.SetTarget (getCurrentTarget ());
		}
	}

	private float getDistanceToTarget() {
		return Vector3.Distance (getCurrentTarget().position, transform.position);
	}

	private int getNextPathIndex() {
		return (currentPathIndex + 1) % pathToFollow.pathObjects.Count;
	}

	private Transform getCurrentTarget() {
		return pathToFollow.pathObjects [currentPathIndex];
	}

	private Transform getNextTarget() {
		return pathToFollow.pathObjects [getNextPathIndex()];
	}
}
