using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityStandardAssets.Utility;

public class TrafficController : MonoBehaviour {
	public List<GameObject> carPrefabs = new List<GameObject> ();
	public int minActiveCars;
	public int maxActiveCars = 10;
	public int maxCarAge = 300; // Maximum age of a car, in seconds
	public WaypointCircuit circuit;
	public GameObject player;
	public float minPlayerDist = 200;
	private Dictionary<float, GameObject> activeCars;

	void Awake() {
		activeCars = new Dictionary<float, GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		// Check if there's X cars active, randomly despawn and respawn different cars with different driver styles and different colors, etc.
		// Ideally we will have a number of different car models too.
		if (Random.Range(0, 10) == 0 && activeCars.Count < maxActiveCars) {
			GameObject newCar = InstantiateNewCar();
			if (newCar != null) {
				activeCars.Add(Time.time, newCar);
			}
		}

		if (Random.Range(0, 100) == 50 && activeCars.Count > minActiveCars) {
			CheckCarsAge();
		}
	}

	public void Reset() {
		foreach (KeyValuePair<float, GameObject> activeCarPair in activeCars) {
			Destroy(activeCarPair.Value);
		}
		activeCars.Clear();
	}

	private GameObject InstantiateNewCar() {
		// Calculates the position and where to point to
		int waypointIndex = GetRandomWaypointIndex();
		Vector3 playerPos = player.transform.position;

		int nextIndex = (waypointIndex + 1) % circuit.transform.childCount;
		Vector3 pos = circuit.transform.GetChild(waypointIndex).position;
		Vector3 nextWaypointPos = circuit.transform.GetChild(nextIndex).position;
		float playerDist = Vector3.Distance(playerPos, pos);
		
		if (playerDist > minPlayerDist && CanSpawnCarHere(pos)) {
			// Instantiate the new car
			GameObject newCar = Instantiate(carPrefabs[0], pos, Quaternion.identity);
			newCar.transform.LookAt(nextWaypointPos);
			CarController carControl = newCar.GetComponent<CarController>();
			carControl.m_Topspeed = Random.Range(40, 60);
			newCar.SetActive(true);
			return newCar;
		}
		return null;
	}

	private int GetRandomWaypointIndex() {
		int waypointCount = circuit.transform.childCount;
		return Random.Range(0, waypointCount - 1);
	}

	// Destroys and removes a car if it's old enough and far enough from the player
	private void CheckCarsAge() {
		List<float> destroyedCars = new List<float>();
		foreach (KeyValuePair<float, GameObject> activeCarPair in activeCars) {
			float carAge = Time.time - activeCarPair.Key;
			Vector3 carPos = activeCarPair.Value.transform.position;
			float playerDist = Vector3.Distance(player.transform.position, carPos);
			if (carAge > maxCarAge && playerDist > minPlayerDist) {
				Destroy(activeCarPair.Value);
				destroyedCars.Add(activeCarPair.Key);
			}
		}
		foreach(float key in destroyedCars) {
			activeCars.Remove(key);
		}
	}

	// Checks the position of all other cars to check if the new car's position is too close to them.
	// This ensures 2 cars won't spawn on top of each other (it has happened...).
	private bool CanSpawnCarHere(Vector3 position) {
		float minDistance = Mathf.Infinity;
		foreach (KeyValuePair<float, GameObject> activeCarPair in activeCars) {
			Vector3 carPos = activeCarPair.Value.transform.position;
			float carDist = Vector3.Distance(position, carPos);
			minDistance = Mathf.Min(carDist, minDistance);
		}
		return minDistance > 50;
	}
}
