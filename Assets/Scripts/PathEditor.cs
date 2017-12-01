using System.Collections.Generic;
using UnityEngine;

public class PathEditor : MonoBehaviour {
	public float gizmoRadius = 0.3f;
	public List<Transform> pathObjects = new List<Transform> ();
	public int selectedIndex = -1;
	public bool isMovingPoint = false;
	private Transform[] pathArray;

	public float CalculateLength() {
		float length = 0;
		if (pathObjects.Count < 2)
			return 0;
		for (int i = 1; i < pathObjects.Count; i++) {
			Vector3 pos1 = pathObjects[i - 1].position;
			Vector3 pos2 = pathObjects[i].position;
			length += Vector3.Distance(pos1, pos2);
		}
		// Distance between last and first, looping the circuit
		//length += Vector3.Distance(pathArray[pathArray.Length - 1].position, pathArray[0].position);
		return length;
	}
	
	public void OnDrawGizmos() {
		pathArray = GetComponentsInChildren<Transform> ();
		pathObjects.Clear ();

		for (int i = 0; i < pathArray.Length; i++) {
			if (pathArray [i] != this.transform) {
				pathObjects.Add(pathArray[i]);
				int j = pathObjects.Count - 1;
				Vector3 position = pathObjects [j].position;
				Gizmos.color = ChooseRayColor();
				if (j > 0) {
					Vector3 previous = pathObjects [j - 1].position;
					Gizmos.DrawLine (previous, position);
					DrawGizmo (position, pathObjects.Count - 1);
				} else {
					DrawGizmo (position, pathObjects.Count - 1);
				}
			}
		}
	}

	private void DrawGizmo(Vector3 position, int index) {
		Gizmos.color = ChooseGizmoColor (index);
		Gizmos.DrawWireSphere (position, (index == selectedIndex) ? gizmoRadius * 1.5f : gizmoRadius);
	}

	private Color ChooseRayColor() {
		return isSelected () ? Color.yellow : Color.white;
	}

	private Color ChooseGizmoColor(int pointIndex) {
		if (pointIndex != selectedIndex)
			return ChooseRayColor();
		return isMovingPoint ? Color.red : Color.green;
	}

	public bool isSelected() {
		return false;
	}
}
