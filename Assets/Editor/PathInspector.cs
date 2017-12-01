using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathEditor))]
public class PathInspector : Editor {
	private Vector3 mouseHoverPos;

	public override void OnInspectorGUI () {
		serializedObject.Update ();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmoRadius"));
		PathPointsList.Show(serializedObject.FindProperty("pathObjects"), PathListOptions.NoElementLabels);
		serializedObject.ApplyModifiedProperties ();
	}

	void OnEnable()	{
		SceneView.onSceneGUIDelegate += this.OnSceneMouseOver;
		SceneView.onSceneGUIDelegate += this.OnSceneMouseClick;
		mouseHoverPos = new Vector3 (0, 0, 0);
	}

	void OnSceneMouseOver(SceneView view) {
		Transform selected = Selection.activeTransform;
		if (selected != null) {
			PathEditor selectedEditor = selected.GetComponent<PathEditor>();
			if (selectedEditor) {
				//Ray ray = Camera.current.ScreenPointToRay(Event.current.mousePosition
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				RaycastHit hit;
				//And add switch Event.current.type for checking Mouse click and switch tiles
				if (Physics.Raycast(ray, out hit, 1000f)) {
					mouseHoverPos = hit.point + (hit.normal * selectedEditor.gizmoRadius);
					// Debug.DrawLine (hit.point, hit.point + hit.normal, Color.green);

					selectedEditor.selectedIndex = -1;
					for (int i = 0; i < selectedEditor.pathObjects.Count; i++) {
						Transform pathObject = selectedEditor.pathObjects[i];
						if (Vector3.Distance(pathObject.position, mouseHoverPos) <= selectedEditor.gizmoRadius * 2) {
							selectedEditor.selectedIndex = i;
						}
					}

					if (selectedEditor.isMovingPoint) {
						Transform pathObject = selectedEditor.pathObjects[selectedEditor.selectedIndex];
						pathObject.position = mouseHoverPos;
					}
				}
				view.Repaint();
			}
		}
	}

	void OnSceneMouseClick(SceneView view) {
		// Only on Right Clicks
		if (Event.current.type == EventType.MouseUp && Event.current.button == 1) {
			Transform selected = Selection.activeTransform;
			PathEditor selectedEditor = selected.GetComponent<PathEditor> ();
			if (selectedEditor && selectedEditor.selectedIndex != -1) selectedEditor.isMovingPoint = !selectedEditor.isMovingPoint;
		}
	}
}
