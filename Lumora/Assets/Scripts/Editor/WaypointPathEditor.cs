using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(WaypointPath))]
public class WaypointPathEditor : Editor
{
    private WaypointPath path;

	private void OnEnable()
	{
		path = (WaypointPath)target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if(GUILayout.Button("Add Point"))
		{
			//Track last change
			Undo.RecordObject(path, "Add Waypoint"); 
			//Set the new point to be offset from the previous point if it exists
			Vector3 newPoint = path.points.Count > 0 ? path.points[path.points.Count - 1] + Vector3.forward : Vector3.zero;
			path.points.Add(newPoint);
			EditorUtility.SetDirty(path);
		}
		if (GUILayout.Button("Remove Last Point"))
		{
			if (path.points.Count > 0)
			{
				//Track last change
				Undo.RecordObject(path, "Remove Waypoint");
				//Set the new point to be offset from the previous point if it exists

				path.points.RemoveAt(path.points.Count - 1);
				EditorUtility.SetDirty(path);
			}
		}
	}

	private void OnSceneGUI()
	{
		if (path.points == null) return;

		for (int i = 0; i < path.points.Count; i++)
		{
			EditorGUI.BeginChangeCheck();

			Vector3 worldPos = path.transform.TransformPoint(path.points[i]);
			Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);

			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(path, "Move Waypoint");
				//Convert back from world space to local space relative to the parent path
				path.points[i] = path.transform.InverseTransformPoint(newWorldPos);
				EditorUtility.SetDirty(path);
			}

			Vector3 offset = new Vector3(0,0.2f,0);
			Handles.Label(worldPos + offset, $"Point {i}");

			if(i > 0)
			{
				Handles.DrawLine(path.GetPointWorld(i - 1), path.GetPointWorld(i));
			}
		}

		if(path.loop && path.points.Count > 1)
		{
			Handles.DrawLine(path.GetPointWorld(path.points.Count - 1), path.GetPointWorld(0));
		}

	}
}
#endif