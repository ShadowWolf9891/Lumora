using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyBehaviorTree
{
	/// <summary>
	/// Base class for all behavior tree nodes.
	/// </summary>
	public abstract class BTNode : ScriptableObject
	{
		[SerializeField] private BTBlackboard blackboard; //A reference to the objects we care about

		public static float DefaultWidth = 120f;
		public static float DefaultHeight = 60;
		public static float DefaultIndexWidth = 30;
		public static float DefaultIndexHeight = 20;

		public string Name = "";
		public Rect NodeRect;

		[HideInInspector]
		public NodePort InputPort;
		[HideInInspector]
		public NodePort OutputPort;

		public BTNode Parent;
		public List<BTNode> Children;

		private NodeState CurState = NodeState.IDLE;

		/// <summary>
		/// Initialize the Node at a specific location in the editor.
		/// </summary>
		/// <param name="position"></param>
		public virtual void Initialize(Vector2 position, BTBlackboard bb)
		{
			blackboard = bb;
			NodeRect = new Rect(position, new Vector2(DefaultWidth, DefaultHeight));
			Children = new List<BTNode>();
		}

		public virtual void LoadPorts()
		{
			InputPort = new NodePort(this, PortType.INPUT);
			OutputPort = new NodePort(this, PortType.OUTPUT);
		}


		/// <summary>
		/// Tick the current node and set its state depending on the result of executing it.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public NodeState Tick(GameObject context)
		{
			CurState = Execute(context);
			return CurState;
		}

		/// <summary>
		/// What happens when this node is Ticked. Returns either running, success, or failure.
		/// </summary>
		/// <returns></returns>
		protected abstract NodeState Execute(GameObject context);


#if UNITY_EDITOR
		/// <summary>
		/// Draw the node and change it's colour depending on what state it is in.
		/// </summary>
		/// <param name="viewOffset"></param>
		public virtual void Draw(Vector2 viewOffset)
		{
			var adjustedRect = new Rect(NodeRect.position + viewOffset, NodeRect.size);
			Color originalColor = GUI.color;
			Color originalTextColor = GUI.contentColor;

			switch (CurState)
			{
				case NodeState.RUNNING:
					GUI.color = Color.yellow;
					break;
				case NodeState.SUCCESS:
					GUI.color = Color.green;
					break;
				case NodeState.FAILURE:
					GUI.color = Color.red;
					break;
				default:
					GUI.color = Color.white;
					break;
			}
			GUI.contentColor = Color.white;
			GUI.Box(adjustedRect, Name);
			DrawIndex(adjustedRect);
			InputPort?.Draw(viewOffset);
			OutputPort?.Draw(viewOffset);
			GUI.color = originalColor;
			GUI.contentColor = originalTextColor;
		}
		/// <summary>
		/// Draw a number to signify which order this node will be executed by the parent.
		/// </summary>
		/// <param name="adjNodeRect"></param>
		private void DrawIndex(Rect adjNodeRect)
		{
			var indexRect = new Rect(
				adjNodeRect.xMax - DefaultIndexWidth,
				adjNodeRect.yMax - DefaultIndexHeight,
				DefaultIndexWidth,
				DefaultIndexHeight);

			if (GetIndex() != -1)
			{
				GUI.Box(indexRect, GetIndex().ToString());
			}
		}
#endif
		/// <summary>
		/// Set this node and all child nodes back to IDLE status.
		/// </summary>
		public void ResetStatus()
		{
			CurState = NodeState.IDLE;
			foreach (var child in Children)
			{
				child.ResetStatus();
			}
		}
		/// <summary>
		/// Change the position of the node when the user moves it in the editor.
		/// </summary>
		/// <param name="position"></param>
		public virtual void Move(Vector2 position)
		{
			NodeRect.position = position;
			InputPort?.SetPosition(position, (NodeRect.width - InputPort.Rect.width) / 2);
			OutputPort?.SetPosition(position, (NodeRect.width - OutputPort.Rect.width) / 2);
		}

		/// <summary>
		/// Get the index of this node within its parents list.
		/// </summary>
		/// <returns> -1 if there is no parent or it is invalid, the index otherwise.</returns>
		private int GetIndex()
		{
			if (Parent == null || Parent.Children == null) return -1;
			return Parent.Children.IndexOf(this);
		}

		/// <summary>
		/// Useful for if you are building the tree first then injecting the blackboard later.
		/// </summary>
		/// <param name="blackboard"></param>
		public void SetBlackboard(BTBlackboard blackboard)
		{
			this.blackboard = blackboard;
		}
		public BTBlackboard GetBlackboard() => blackboard;
	}
	/// <summary>
	/// The state of the current node.
	/// </summary>
	public enum NodeState { IDLE, RUNNING, SUCCESS, FAILURE }
}