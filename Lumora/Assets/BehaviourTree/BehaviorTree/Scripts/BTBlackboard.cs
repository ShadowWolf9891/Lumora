using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// A blackboard holds all of the neccisary references a behavior tree node may need. Typically you have 1 for each AI or NPC.
	/// This version is dynamic, allowing for any type of object to be store, but it is not type safe, meaning if used incorrectly
	/// it will break. Make sure you know what kind of data you are trying to get, and try cast to make sure it is the correct type.
	/// </summary>
	/// 
	[CreateAssetMenu(menuName = "BehaviorTree/Blackboard"), Serializable]
	public class BTBlackboard : ScriptableObject
	{
		private Dictionary<string, object> data = new Dictionary<string, object>();
		[SerializeField]
		List<BlackboardEntry> entries = new List<BlackboardEntry>();

		public void Set<T>(string key, T value)
		{
			var entry = entries.Find(e => e.Key == key);
			if (entry == null)
			{
				entry = new BlackboardEntry { Key = key, Type = GetTypeEnum(typeof(T)) };
				entries.Add(entry);
			}
			entry.SetValue(value);
		}

		public T Get<T>(string key)
		{
			var entry = entries.Find(e => e.Key == key);
			return entry != null && entry.GetValue() is T val ? val : default;
		}

		public bool Has(string key) => entries.Exists(e => e.Key == key);
		public List<BlackboardEntry> Entries => entries;

		private ValueType GetTypeEnum(Type t)
		{
			if (t == typeof(string)) return ValueType.String;
			if (t == typeof(int)) return ValueType.Int;
			if (t == typeof(float)) return ValueType.Float;
			if (t == typeof(bool)) return ValueType.Bool;
			if (t == typeof(GameObject)) return ValueType.GameObject;
			if (t == typeof(Vector3)) return ValueType.Vector3;
			throw new ArgumentException("Unsupported type " + t);
		}
	}
}