using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EasyBehaviorTree
{
	public enum PortType { INPUT, OUTPUT }

	[Serializable]
	public class NodePort
	{
		public BTNode ParentNode { get; private set; }
		public Rect Rect { get; private set; }
		public PortType PType { get; private set; }
		public string Label { get; private set; }

		private const float Size = 10f;
		Texture2D tex;
		public NodePort(BTNode parent, PortType type, string label = "")
		{
			ParentNode = parent;
			PType = type;
			Label = label;
			Rect = new Rect(0, 0, Size, Size);
			SetPosition(parent.NodeRect.position, (parent.NodeRect.width - Size) / 2);
			tex = Resources.Load<Texture2D>("UI/CircleButton"); //Load image in resource folder
		}

		public Vector2 Center => new Vector2(Rect.x + Rect.width / 2, Rect.y + Rect.height / 2);

#if UNITY_EDITOR
		public void Draw(Vector2 viewOffset)
		{
			if (tex == null) return;
			var adjustedRect = new Rect(Rect.position + viewOffset, Rect.size);
			GUI.DrawTexture(adjustedRect, tex, ScaleMode.ScaleToFit);
		}
#endif

		public bool Contains(Vector2 point) => Rect.Contains(point);

		public void SetPosition(Vector2 nodePosition, float offsetX)
		{
			float x = nodePosition.x + offsetX;
			float y = nodePosition.y + (PType == PortType.INPUT ? -Rect.height : BTNode.DefaultHeight);
			Rect = new Rect(x, y, Size, Size);
		}
	}
}