using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// Behavior tree class that contains the root node on creation. Behavior trees are fairly complex, so I suggest researching them
	/// if you plan on using them to control AI or NPC actions.
	/// </summary>
	[CreateAssetMenu(menuName = "BehaviorTree/BehaviorTree"), Serializable]
	public class BehaviorTree : ScriptableObject
	{
		public BTNode rootNode;
		public List<BTNode> nodes = new();

#if UNITY_EDITOR
		public void AddNode(BTNode node)
		{
			nodes.Add(node);
			if (node.Name == "")
			{
				node.Name = node.GetType().Name;
			}
			node.name = node.Name;
			AssetDatabase.AddObjectToAsset(node, this);
			AssetDatabase.SaveAssets();
		}
		public void RemoveNode(BTNode node)
		{
			nodes.Remove(node);
			AssetDatabase.RemoveObjectFromAsset(node);
			AssetDatabase.SaveAssets();
		}
#endif
		/// <summary>
		/// Send a tick down the chain of the behavior tree.
		/// </summary>
		public NodeState Tick(GameObject context)
		{
			rootNode.ResetStatus();
			return rootNode != null ? rootNode.Tick(context) : NodeState.FAILURE;
		}
	}
}