using System;
using Unity.VisualScripting;
using UnityEngine;
namespace EasyBehaviorTree
{

	/// <summary>
	/// This node will run until a flag that is set by the user is true, either a blackboard value or a field in an attached component.
	/// </summary>
	[Serializable]
    public class BTRunningActionNode : BTNode
    {
		//[Tooltip("The inspector name of the object you want to execute the function of.")]
		//public string targetName;
		[Tooltip("The name of the function you want to execute.")]
		public string functionName;

		[Tooltip("Stop running and reture SUCCESS when this value is true. It can be a blackboard value or a field in an attached component script.")]
		public string doUntilConditionName;

		public override void LoadPorts()
		{
			InputPort = new NodePort(this, PortType.INPUT);
		}
		protected override NodeState Execute(GameObject context)
		{
			if (context == null || string.IsNullOrEmpty(functionName) || string.IsNullOrEmpty(doUntilConditionName))
				return NodeState.FAILURE;

			var components = context.GetComponents<MonoBehaviour>();
			foreach (var comp in components)
			{
				var method = comp.GetType().GetMethod(functionName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (method != null)
				{
					method.Invoke(comp, null);

					var finishedField = comp.GetType().GetField(doUntilConditionName);
					if(finishedField != null && finishedField.GetValue(comp) is bool finished ) //Check if there is a bool field with the provided string
					{
						if (finished) //If the bool value is true
						{
							return NodeState.SUCCESS;
						}
						return NodeState.RUNNING;
					}
					else if (GetBlackboard().Has(doUntilConditionName)) //Check if there is a blackboard value that matches the provided string
					{
						bool value = GetBlackboard().Get<bool>(doUntilConditionName);
						return value ? NodeState.SUCCESS : NodeState.RUNNING;
					}
				}
			}

			Debug.LogWarning($"Method '{functionName}' not found on any MonoBehaviour attached to {context.name}");
			return NodeState.FAILURE;
		}

    }
}