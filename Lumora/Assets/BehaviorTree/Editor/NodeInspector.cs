using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace EasyBehaviorTree
{
	[CustomEditor(typeof(BTNode), true)]
	public class NodeInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			BTNode node = (BTNode)target;
			var bb = node.GetBlackboard();

			if (bb == null)
			{
				EditorGUILayout.HelpBox("Blackboard is null", MessageType.Warning);
				return;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Blackboard (Preview)", EditorStyles.boldLabel);

			foreach (var entry in bb.Entries)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(entry.Key);

				string val = entry.Type switch
				{
					ValueType.String => entry.StringValue,
					ValueType.Int => entry.IntValue.ToString(),
					ValueType.Float => entry.FloatValue.ToString("F2"),
					ValueType.Bool => entry.BoolValue.ToString(),
					ValueType.GameObject => entry.GameObjectValue != null ? entry.GameObjectValue.name : "null",
					ValueType.Vector3 => entry.Vector3Value.ToString(),
					_ => "Unknown"
				};

				EditorGUILayout.LabelField(val);
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("Edit Blackboard"))
			{
				BlackboardEditor.ShowWindow(bb); // You can customize this as needed
			}
		}
	}
}
#endif