using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixWheelCollider : MonoBehaviour {
	public WheelCollider wheelCollider;

	// Use this for initialization
	void Start () {
		wheelCollider.ConfigureVehicleSubsteps(4f, 12, 15);
	}
}
