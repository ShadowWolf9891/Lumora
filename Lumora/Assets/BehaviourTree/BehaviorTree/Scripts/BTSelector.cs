using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// A Selector runs it's children in order until one succeeds.
	/// </summary>
	[CreateAssetMenu(menuName = "BehaviorTree/Composite/Selector")]
	public class BTSelector : BTNode
	{
		protected override NodeState Execute(GameObject context)
		{
			foreach (var child in Children)
			{
				var result = child.Tick(context);
				if (result != NodeState.FAILURE)
					return result;
			}
			return NodeState.FAILURE;
		}
	}
}