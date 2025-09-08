using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// A decorator node that turns the child node from a success to a failure and vice versa. Can only have one child.
	/// </summary>

	[CreateAssetMenu(menuName = "BehaviorTree/Decorator/Inverter"), Serializable]
	public class BTInverter : BTDecorator
	{
		protected override NodeState Execute(GameObject context)
		{
			var result = Children[0].Tick(context);
			if (result == NodeState.SUCCESS) return NodeState.FAILURE;
			if (result == NodeState.FAILURE) return NodeState.SUCCESS;
			return NodeState.RUNNING;
		}
	}
}