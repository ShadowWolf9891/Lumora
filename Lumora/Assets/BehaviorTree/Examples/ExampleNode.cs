using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EasyBehaviorTree; //Don't forget to include using statement for any of your custom nodes!


namespace EasyBehaviorTreeExample
{
	[Serializable]
	public class ExampleNode : BTNode
	{
		//Variables to show in the inspector of the behavior tree editor window go here. 
		//These are things that should probably not change, and are set before the game starts.
		//ex. [SerializeField] string entityToTrackName;



		public override void Initialize(Vector2 position, BTBlackboard bb)
		{
			base.Initialize(position, bb);

			//Define any unique initialization code... 

			

		}
		public override void LoadPorts()
		{
			//Choose what ports to load here for this node

			InputPort = new NodePort(this, PortType.INPUT);
			OutputPort = new NodePort(this, PortType.OUTPUT); //Remove this for a leaf node

		}


		protected override NodeState Execute(GameObject context)
		{
			//Whatever you want to do when the node executes...

			return NodeState.SUCCESS;
		}
	}
}