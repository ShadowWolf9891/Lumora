using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// A sequence node runs its children in order until it fails.
	/// </summary>
	[Serializable]
	public class BTSequence : BTNode
	{
		protected override NodeState Execute(GameObject context)
		{
			foreach (var child in Children)
			{
				var result = child.Tick(context);
				if (result != NodeState.SUCCESS)
					return result;
			}

			return NodeState.SUCCESS;
		}
	}
}