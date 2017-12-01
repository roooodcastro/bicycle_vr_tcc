using UnityEngine;

[System.Serializable]
public class PID {
	public float pFactor, iFactor, dFactor;

	public float integral;
	public float lastError;

	public PID(float pFactor, float iFactor, float dFactor) {
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}


	public float Update(float setpoint, float actual, float timeFrame) {
		float present = setpoint - actual;
		integral += present * timeFrame;
		integral = Mathf.Clamp (integral, -0.1f, 0.1f);
		float deriv = (present - lastError) / timeFrame;
		lastError = present;
		return present * pFactor + integral * iFactor + deriv * dFactor;
	}
}
