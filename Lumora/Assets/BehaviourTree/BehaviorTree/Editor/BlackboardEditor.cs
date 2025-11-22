using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace EasyBehaviorTree
{
	public class BlackboardEditor : EditorWindow
	{
		private BTBlackboard blackboard;
		private Vector2 scrollPos;

		public static void ShowWindow(BTBlackboard bb)
		{
			var window = GetWindow<BlackboardEditor>("Blackboard Editor");
			window.blackboard = bb;
			window.Show();
		}

		private void OnGUI()
		{
			if (blackboard == null)
			{
				EditorGUILayout.HelpBox("No blackboard assigned.", MessageType.Warning);
				return;
			}
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			EditorGUILayout.LabelField("Blackboard Entries", EditorStyles.boldLabel);
			SerializedObject so = new SerializedObject(blackboard);
			SerializedProperty entries = so.FindProperty("entries");
			so.Update();
			for (int i = 0; i < entries.arraySize; i++)
			{
				SerializedProperty entry = entries.GetArrayElementAtIndex(i);
				var keyProp = entry.FindPropertyRelative("Key");
				var typeProp = entry.FindPropertyRelative("Type");

				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.PropertyField(keyProp);
				EditorGUILayout.PropertyField(typeProp);

				switch ((ValueType)typeProp.enumValueIndex)
				{
					case ValueType.String:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("StringValue"));
						break;
					case ValueType.Int:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("IntValue"));
						break;
					case ValueType.Float:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("FloatValue"));
						break;
					case ValueType.Bool:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("BoolValue"));
						break;
					case ValueType.GameObject:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("GameObjectValue"));
						break;
					case ValueType.Vector3:
						EditorGUILayout.PropertyField(entry.FindPropertyRelative("Vector3Value"));
						break;
				}

				if (GUILayout.Button("Delete"))
					entries.DeleteArrayElementAtIndex(i);

				EditorGUILayout.EndVertical();
			}

			if (GUILayout.Button("Add Entry"))
				entries.InsertArrayElementAtIndex(entries.arraySize);

			EditorGUILayout.EndScrollView();
			so.ApplyModifiedProperties();
		}
	}
}
#endif