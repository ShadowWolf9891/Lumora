using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	[System.Serializable]
	public class BlackboardEntry
	{
		public string Key;

		public ValueType Type;

		public string StringValue;
		public int IntValue;
		public float FloatValue;
		public bool BoolValue;
		public GameObject GameObjectValue;
		public Vector3 Vector3Value;

		public object GetValue()
		{
			return Type switch
			{
				ValueType.String => StringValue,
				ValueType.Int => IntValue,
				ValueType.Float => FloatValue,
				ValueType.Bool => BoolValue,
				ValueType.GameObject => GameObjectValue,
				ValueType.Vector3 => Vector3Value,
				_ => null
			};
		}

		public void SetValue(object obj)
		{
			switch (Type)
			{
				case ValueType.String: StringValue = obj?.ToString(); break;
				case ValueType.Int: IntValue = obj is int i ? i : 0; break;
				case ValueType.Float: FloatValue = obj is float f ? f : 0f; break;
				case ValueType.Bool: BoolValue = obj is bool b && b; break;
				case ValueType.GameObject: GameObjectValue = obj as GameObject; break;
				case ValueType.Vector3: Vector3Value = obj is Vector3 v ? v : new Vector3(0, 0, 0); break;
			}
		}
	}

	public enum ValueType { String, Int, Float, Bool, GameObject, Vector3 }
}