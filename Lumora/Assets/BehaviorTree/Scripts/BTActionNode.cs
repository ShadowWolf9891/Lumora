using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// Execute an event "functionName" in class "target". 
	/// </summary>
	[Serializable]
	public class BTActionNode : BTNode
	{
		//[Tooltip("The inspector name of the object you want to execute the function of.")]
		//public string targetName;
		[Tooltip("The name of the function you want to execute.")]
		public string functionName;
		public override void LoadPorts()
		{
			InputPort = new NodePort(this, PortType.INPUT);
		}
		protected override NodeState Execute(GameObject context)
		{
			if (context == null || string.IsNullOrEmpty(functionName))
				return NodeState.FAILURE;

			var components = context.GetComponents<MonoBehaviour>();
			foreach (var comp in components)
			{
				var method = comp.GetType().GetMethod(functionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(comp, null);
					return NodeState.SUCCESS;
				}
			}

			Debug.LogWarning($"Method '{functionName}' not found on any MonoBehaviour attached to {context.name}");
			return NodeState.FAILURE;
		}
	}
}