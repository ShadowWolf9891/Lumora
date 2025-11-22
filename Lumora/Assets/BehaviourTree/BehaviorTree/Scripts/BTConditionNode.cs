using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// A condition node checks if a condition is true and returns either success or failure. This class currently keeps things organized
	/// and allows for condition nodes to be grouped together. Condition nodes do not have children.
	/// </summary>
	/// 
	[Serializable]
	public class BTConditionNode : BTNode
	{
		public string key = ""; // Key to check in the blackboard
		public bool expectedValue = true;     // Expected condition

		public override void LoadPorts()
		{
			InputPort = new NodePort(this, PortType.INPUT);
		}
		protected override NodeState Execute(GameObject context)
		{
			if (GetBlackboard() == null)
			{
				Debug.LogWarning("Blackboard is null in condition node.");
				return NodeState.FAILURE;
			}

			bool value = GetBlackboard().Get<bool>(key);
			return value == expectedValue ? NodeState.SUCCESS : NodeState.FAILURE;
		}
	}
}