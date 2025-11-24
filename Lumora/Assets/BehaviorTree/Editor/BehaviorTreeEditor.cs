using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace EasyBehaviorTree
{
	/// <summary>
	/// Class for the behavior tree editor window. Access it from Window > Behavior Tree Editor.
	/// </summary>
	internal class BehaviorTreeEditor : EditorWindow
	{
		private BehaviorTree tree;
		private BTBlackboard bb;
		private NodeInspector nodeEditor;
		private BTNode selectedNode;
		private NodePort selectedPort;
		private bool movingNode;
		private bool drawingConnection;
		Vector2 curOffset = Vector2.zero;
		Vector2 viewOffset = Vector2.zero;

		/// <summary>
		/// Open the window
		/// </summary>
		[MenuItem("Window/Behavior Tree Editor")]
		static void OpenWindow()
		{
			BehaviorTreeEditor window = GetWindow<BehaviorTreeEditor>("Behavior Tree");
			window.Show();
		}
		/// <summary>
		/// Unity event that draws the graphic user interface when something changes.
		/// </summary>
		private void OnGUI()
		{
			//Create visual elements for the BT window
			Rect graphRect = new Rect(0, 0, position.width * 0.7f, position.height); // 70% width for graph
			Rect inspectorRect = new Rect(graphRect.width, 0, position.width - graphRect.width, position.height); // Remaining 30%

			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			if (GUILayout.Button("New Tree", EditorStyles.toolbarButton))
			{
				CreateNewTree();
			}
			if (GUILayout.Button("Load Tree", EditorStyles.toolbarButton))
			{
				LoadTree();
			}
			//This code is useful if the tree you are making does not require runtime information. It allows you to tick the tree in editor.
			//if (GUILayout.Button("Tick Tree", EditorStyles.toolbarButton))
			//{
			//	tree.Tick();
			//}
			GUILayout.Space(300);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			DrawGraphArea(graphRect);
			DrawInspectorArea(inspectorRect);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			if (tree != null)
			{
				ProcessEvents(Event.current, graphRect);
			}

		}
		/// <summary>
		/// Draw the area that contains information about the node that was last clicked on.
		/// </summary>
		/// <param name="inspectorRect"></param>
		private void DrawInspectorArea(Rect inspectorRect)
		{
			GUILayout.BeginArea(inspectorRect, EditorStyles.helpBox);
			if (nodeEditor != null)
			{
				GUILayout.Label("Node Inspector", EditorStyles.boldLabel);
				nodeEditor.OnInspectorGUI();
			}
			GUILayout.EndArea();
		}
		/// <summary>
		/// Draw the area where the behavior tree will appear.
		/// </summary>
		/// <param name="graphRect"></param>
		private void DrawGraphArea(Rect graphRect)
		{
			GUILayout.BeginArea(graphRect);
			if (tree != null)
			{
				DrawNodes();
			}
			GUILayout.EndArea();
		}
		/// <summary>
		/// Method that runs when the user presses the "New Tree" button.
		/// </summary>
		private void CreateNewTree()
		{
			//Open a window for saving the tree
			string path = EditorUtility.SaveFilePanelInProject("Save Behavior Tree", "NewBehaviorTree", "asset", "Save your new behavior tree");
			if (string.IsNullOrEmpty(path)) { return; }

			// Create the BehaviorTree asset
			tree = ScriptableObject.CreateInstance<BehaviorTree>();

			//Create blackboard
			bb = ScriptableObject.CreateInstance<BTBlackboard>();
			bb.name = "Blackboard";

			// Create the root node of BTSequence type. This can be change to BTSelector if necissary
			var rootNode = ScriptableObject.CreateInstance<BTSequence>();
			rootNode.Initialize(new Vector2(100, 100), bb);
			rootNode.name = "RootNode"; //Make sure the name is always "RootNode" when saving, but the user can change the display name.
			rootNode.Name = rootNode.name;
			rootNode.OutputPort = new NodePort(rootNode, PortType.OUTPUT);

			// Save root node as a subasset of the tree
			AssetDatabase.CreateAsset(tree, path);
			AssetDatabase.AddObjectToAsset(rootNode, tree);
			AssetDatabase.AddObjectToAsset(bb, tree);
			// Assign root node and mark tree dirty (means it must be saved by editor)
			tree.rootNode = rootNode;
			EditorUtility.SetDirty(tree);
			EditorUtility.SetDirty(bb);

			// Save and refresh
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			Debug.Log("Created new Behavior Tree with default root node.");
		}

		/// <summary>
		/// Loads the tree from a file. The ports are not saved, so they need to be loaded seperate.
		/// </summary>
		private void LoadTree()
		{
			string path = EditorUtility.OpenFilePanel("Load Behavior Tree", "Assets", "asset");
			path = FileUtil.GetProjectRelativePath(path);
			tree = AssetDatabase.LoadAssetAtPath<BehaviorTree>(path);

			if (tree != null)
			{
				// Find individual assets among subassets
				var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
				foreach (var asset in subAssets)
				{
					if (asset is BTBlackboard blackboard)
					{
						bb = blackboard; //Get the blackboard reference
					}
					if(asset is BTNode node && asset.name != "RootNode")
					{
						node.LoadPorts(); //Load and initialize each nodes input / output ports
					}
					if (asset is BTNode rootNode && asset.name == "RootNode")
					{
						//Root node is different because it only has output even though it is a BTSequence
						rootNode.OutputPort = new NodePort(rootNode, PortType.OUTPUT); 
					}
				}

				if (bb == null)
				{
					Debug.LogWarning("Blackboard not found in tree asset.");
				}
			}
		}
		/// <summary>
		/// Draw each node based on the nodes on the tree, and draw the lines connecting them.
		/// </summary>
		private void DrawNodes()
		{
			tree.rootNode.Draw(viewOffset);
			DrawLines(tree.rootNode);
			foreach (BTNode node in tree.nodes)
			{
				node.Draw(viewOffset);
				DrawLines(node);
			}
		}
		/// <summary>
		/// Draw the lines between each node and it's children.
		/// </summary>
		/// <param name="node"></param>
		private void DrawLines(BTNode node)
		{
			foreach (var child in node.Children)
			{
				DrawConnection(node.OutputPort.Center + viewOffset, child.InputPort.Center + viewOffset);
			}
		}



		/// <summary>
		/// Process GUI events like when nodes are clicked.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="GraphArea"></param>
		private void ProcessEvents(Event e, Rect GraphArea)
		{
			//Right Click
			if (e.type == EventType.ContextClick) // or e.button == 1 && e.type == MouseDown
			{
				
				if (ClickedOnNode(e.mousePosition, out BTNode clickedNode)) //Right clicked on a node
				{
					ShowNodeContextMenu(clickedNode);
					e.Use(); // Mark event as used
				}
				else if (ClickedOnPort(e.mousePosition, out NodePort clickedPort)) //Right clicked on a port
				{
					ShowPortContextMenu(clickedPort);
				}
				else //Right clicked empty space
				{
					ShowContextMenu(e.mousePosition); //Display menu for adding new nodes.
					e.Use(); // Mark event as used
				}

			}
			//Middle mouse click and drag
			if (e.type == EventType.MouseDrag && e.button == 2)
			{
				//Change offset to reflect where things should be drawn
				viewOffset += e.delta;
				tree.rootNode?.ResetStatus();
				GUI.changed = true;
				e.Use();
			}

			//Left Click Graph area
			if (e.type == EventType.MouseDown && GraphArea.Contains(e.mousePosition) && e.button == 0)
			{
				if (ClickedOnNode(e.mousePosition, out BTNode clickedNode))//Left click on node
				{
					//Reset inspector window
					if (nodeEditor != null)
						DestroyImmediate(nodeEditor);

					// Create a cached editor for the selected node
					nodeEditor = (NodeInspector)Editor.CreateEditor(clickedNode);
					curOffset = clickedNode.NodeRect.position - e.mousePosition;
					movingNode = true; //Show that the user has started moving the node until they release the mouse button
					selectedNode = clickedNode;
					e.Use();
				}
				else if (ClickedOnPort(e.mousePosition, out NodePort clickedPort)) //Left click on port
				{
					selectedPort = clickedPort;
					drawingConnection = true; //Start drawing a connection
				}
				else //Left click empty space
				{
					//Reset inspector window
					DestroyImmediate(nodeEditor);
					nodeEditor = null;
					curOffset = Vector2.zero;
					selectedNode = null;
					e.Use();
				}
			}
			//Release Left mouse button and were moving a node
			if (e.type == EventType.MouseUp && movingNode && e.button == 0)
			{
				//Stop moving the node and reload the tree
				movingNode = false;
				curOffset = Vector2.zero;
				tree.rootNode?.ResetStatus();
				e.Use();
			}
			//Holding down left mouse and moving a node
			if (movingNode && e.type == EventType.MouseDrag && e.button == 0)
			{
				//Move the position of the node
				selectedNode.Move(e.mousePosition + curOffset);
				tree.rootNode?.ResetStatus();
				GUI.changed = true;
				e.Use();
			}
			//Holding down left mouse and drawing a connection
			if (drawingConnection)
			{
				//Draw a connection from the clicked on port to the mouse position
				DrawConnection(selectedPort.Center + viewOffset, e.mousePosition);
				tree.rootNode?.ResetStatus();
				GUI.changed = true;
			}
			//Release the left mouse button while drawing a connection
			if (e.type == EventType.MouseUp && drawingConnection && e.button == 0)
			{
				drawingConnection = false; //Stop drawing the connection

				//Check if there is a port at the current position and connect the nodes if there is
				if (ClickedOnPort(e.mousePosition, out NodePort clickedPort))
				{
					if (selectedPort.PType != clickedPort.PType && selectedPort != clickedPort)
					{
						if (selectedPort.PType == PortType.OUTPUT && !selectedPort.ParentNode.Children.Contains(clickedPort.ParentNode))
						{
							if (selectedPort.ParentNode is BTDecorator) //Decorators can only have 1 child.
							{
								RemoveConnection(selectedPort);
								tree.rootNode?.ResetStatus();
								GUI.changed = true;
							}
							selectedPort.ParentNode.Children.Add(clickedPort.ParentNode);
							RemoveConnection(clickedPort);
							clickedPort.ParentNode.Parent = selectedPort.ParentNode;
						}
						else if (selectedPort.PType == PortType.INPUT && !clickedPort.ParentNode.Children.Contains(selectedPort.ParentNode))
						{
							if (clickedPort.ParentNode is BTDecorator) //Decorators can only have 1 child.
							{
								RemoveConnection(clickedPort);
								tree.rootNode?.ResetStatus();
								GUI.changed = true;
							}
							clickedPort.ParentNode.Children.Add(selectedPort.ParentNode);
							RemoveConnection(selectedPort);
							selectedPort.ParentNode.Parent = clickedPort.ParentNode;
						}
					}
				}
				e.Use();
			}

		}
		/// <summary>
		/// The menu that pops up when the user right clicks on empty space.
		/// </summary>
		/// <param name="position"></param>
		private void ShowContextMenu(Vector2 position)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Add/Sequence Node"), false, () => CreateNode<BTSequence>(position));
			menu.AddItem(new GUIContent("Add/Selector Node"), false, () => CreateNode<BTSelector>(position));
			menu.AddItem(new GUIContent("Add/Condition Node"), false, () => CreateNode<BTConditionNode>(position));
			menu.AddItem(new GUIContent("Add/Action Node/One Time Action"), false, () => CreateNode<BTActionNode>(position));
			menu.AddItem(new GUIContent("Add/Action Node/Continuous Action"), false, () => CreateNode<BTRunningActionNode>(position));
			
			menu.AddItem(new GUIContent("Add/Decorator/Inverter Node"), false, () => CreateNode<BTInverter>(position));
			//New nodes go here...
			//menu.AddItem(new GUIContent("Add/Example Node"), false, () => CreateNode<ExampleNode>(position));

			menu.ShowAsContext();
		}
		/// <summary>
		/// The menu that pops up when the user right clicks a node
		/// </summary>
		/// <param name="node"></param>
		private void ShowNodeContextMenu(BTNode node)
		{
			if (node == tree.rootNode) { return; }

			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Delete Node"), false, () => DeleteNode(node));
			menu.ShowAsContext();
		}
		/// <summary>
		/// The menu that pops up when the user right clicks a port
		/// </summary>
		/// <param name="node"></param>
		private void ShowPortContextMenu(NodePort port)
		{
			if (port.ParentNode.Children.Count > 0 || port.ParentNode.Parent != null)
			{
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Break Connection"), false, () => RemoveConnection(port));
				menu.ShowAsContext();
			}

		}
		/// <summary>
		/// Check if a user clicked on a node in the editor and output which node.
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="clickedNode"></param>
		/// <returns>If any node was clicked.</returns>
		private bool ClickedOnNode(Vector2 mousePosition, out BTNode clickedNode)
		{
			mousePosition -= viewOffset;
			if (tree.rootNode.NodeRect.Contains(mousePosition))
			{
				clickedNode = tree.rootNode;
				return true;
			}

			foreach (BTNode node in tree.nodes)
			{
				if (node.NodeRect.Contains(mousePosition))
				{
					clickedNode = node;
					return true;
				}
			}
			clickedNode = null;
			return false;
		}
		/// <summary>
		/// Check if a user clicked on a port in the editor and output which port.
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="clickedPort"></param>
		/// <returns>If any port was clicked.</returns>
		private bool ClickedOnPort(Vector2 mousePosition, out NodePort clickedPort)
		{
			mousePosition -= viewOffset;
			if (tree.rootNode.OutputPort.Contains(mousePosition))
			{
				clickedPort = tree.rootNode.OutputPort;
				return true;
			}

			foreach (BTNode node in tree.nodes)
			{
				if (node.InputPort != null && node.InputPort.Contains(mousePosition))
				{
					clickedPort = node.InputPort;
					return true;
				}
				if (node.OutputPort != null && node.OutputPort.Contains(mousePosition))
				{
					clickedPort = node.OutputPort;
					return true;
				}
			}
			clickedPort = null;
			return false;
		}
		/// <summary>
		/// Create a new node of type <paramref name="T"/> at the mouse position as long as the type inherits from BTNode.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pos"></param>
		private void CreateNode<T>(Vector2 pos) where T : BTNode
		{
			pos -= viewOffset;
			var node = ScriptableObject.CreateInstance<T>();
			node.Initialize(pos, bb);
			node.LoadPorts();
			tree.AddNode(node);
			tree.rootNode?.ResetStatus();
		}
		/// <summary>
		/// Delete any node except the root and remove connections from it.
		/// </summary>
		/// <param name="node"></param>
		private void DeleteNode(BTNode node)
		{
			RemoveConnection(node.InputPort);
			RemoveConnection(node.OutputPort);

			if (selectedNode == node)
			{
				selectedNode = null;
				nodeEditor = null;
			}
			tree.RemoveNode(node);
			GUI.changed = true;
		}
		/// <summary>
		/// Draw a connection from one <paramref name="startPos"/> to <paramref name="endPos"/>.
		/// </summary>
		/// <param name="startPos"></param>
		/// <param name="endPos"></param>
		private void DrawConnection(Vector2 startPos, Vector2 endPos)
		{
			Handles.BeginGUI();
			Vector2 startTangent = startPos + Vector2.right * 50f;
			Vector2 endTangent = endPos + Vector2.left * 50f;

			Handles.DrawBezier(
				startPos,
				endPos,
				startTangent,
				endTangent,
				Color.white,
				null,
				4f
			);
			Handles.EndGUI();
		}
		/// <summary>
		/// Remove a connection from <paramref name="nodePort"/>.
		/// </summary>
		/// <param name="nodePort"></param>
		private void RemoveConnection(NodePort nodePort)
		{
			if (nodePort == null || nodePort.ParentNode == null)
			{
				return;
			}

			if (nodePort.PType == PortType.INPUT)
			{
				BTNode child = nodePort.ParentNode;
				BTNode parent = child.Parent;

				if (parent != null)
				{
					parent.Children.Remove(child);
					child.Parent = null;
				}
			}
			else if (nodePort.PType == PortType.OUTPUT)
			{
				foreach (var child in nodePort.ParentNode.Children)
				{
					child.Parent = null;
				}
				nodePort.ParentNode.Children.Clear();
			}

			tree.rootNode?.ResetStatus();
			GUI.changed = true;
		}

	}
}
#endif