using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Grouping
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public partial class GroupPagePreviewScreen : BaseDialogForm, ISelfDataHandling
	{
		private enum TreeElementLevel
		{
			/// <summary>
			/// Virtual Root in the tree
			/// </summary>
			VirtualRoot,
			/// <summary>
			/// Root in the tree
			/// </summary>
			Root,
			/// <summary>
			/// Children in the tree
			/// </summary>
			Children
		}

		private TreeNodeAdv _currentSourceNode; // Helps keep track of the node that is being dragged.
		private TreeViewAdvDragHighlightTracker _treeViewDragHighlightTracker;
		private readonly Color _highlighter = Color.Red;
		private SelectedNodesCollection _rightMouseDownNodeCached;
		private readonly TreeViewCreator _treeViewCreator;
		private SelectedNodesCollection _cutSourceNode;

		private readonly IApplicationFunction _myApplicationFunction =
			ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
								   DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

		private readonly IGroupPageHelper _groupPageHelper;

		public string GroupPageName { get; set; }

		public DynamicOptionType GroupPageType { get; set; }

		public IGroupPage GroupPage { get; set; }

		public bool IsNewGroupPage { get; set; }

		public Guid? OptionalColumnId { get; set; }

		public void Persist()
		{
		}

		public GroupPagePreviewScreen()
		{
			GroupPageName = string.Empty;
			InitializeComponent();

			if (!DesignMode)
			{
				SetTexts();
			}
		}

		public GroupPagePreviewScreen(IGroupPageHelper groupPageHelper)
			: this()
		{
			_groupPageHelper = groupPageHelper;
			_treeViewCreator = new TreeViewCreator(_groupPageHelper);
			treeViewAdvPreviewTree.AutoScrolling = ScrollBars.Vertical;
		}

		void treeViewAdvPreviewTreeNodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
		{
			var group = (PersonGroupBase)e.Node.TagObject;
			var len = e.Node.Text.Length;
			if (len > 50) len = 50;
			e.Node.Text = e.Node.Text.Substring(0, len);
			group.Description = new Description(e.Node.Text);
		}

		private void groupPagePreviewScreenLoad(object sender, EventArgs e)
		{
			if (DesignMode) return;

			loadGroupPage();

			_treeViewDragHighlightTracker = new TreeViewAdvDragHighlightTracker(treeViewAdvPreviewTree);

			_treeViewDragHighlightTracker.QueryAllowedPositionsForNode += treeDragDropQueryAllowedPositionsForNode;
			_treeViewDragHighlightTracker.QueryDragInsertInfo += treeViewDragHighlightTrackerQueryDragInsertInfo;


			findPersonsView1.Initialize(new FindPersonsModel(_groupPageHelper.PersonCollection), _myApplicationFunction);
			SetTexts();
			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelTitle.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();
		}

		private void treeViewAdvPreviewTreeDragOver(object sender, DragEventArgs e)
		{
			// Determine drag effects
			TreeNodeAdv destinationNode = null;
			var treeView = sender as TreeViewAdv;

			if (treeView != null)
			{
				Point pointInTree = treeView.PointToClient(new Point(e.X, e.Y));
				_currentSourceNode = null;

				// Looking for a single tree node.
				bool droppable;
				if (e.Data.GetDataPresent(typeof(TreeNodeAdv)))
				{
					// Get the destination and source node.
					destinationNode = treeView.GetNodeAtPoint(pointInTree);
					var sourceNode = (TreeNodeAdv)e.Data.GetData(typeof(TreeNodeAdv));
					// Cache this for use later in the treeViewAdvPreviewTree_QueryAllowedPositionsForNode event handler.
					_currentSourceNode = sourceNode;
					droppable = true;// CanDrop(sourceNode, destinationNode);
				}
				else
					droppable = false;

				e.Effect = droppable ? DragDropEffects.Move : DragDropEffects.None;

				// Let the highlight tracker keep track of the current highlight node.
				_treeViewDragHighlightTracker.SetHighlightNode(destinationNode, pointInTree);
			}
		}

		private void treeViewAdvPreviewTreeDragLeave(object sender, EventArgs e)
		{
			// Let the highlight tracker keep track of the current highlight node.
			_treeViewDragHighlightTracker.ClearHighlightNode();
		}

		private void treeViewAdvPreviewTreeDragDrop(object sender, DragEventArgs e)
		{
			var treeView = sender as TreeViewAdv;

			if (treeView != null)
			{
				// Get the source node.
				TreeNodeAdv destinationNode = _treeViewDragHighlightTracker.HighlightNode;
				TreeViewDropPositions dropPosition = _treeViewDragHighlightTracker.DropPosition;
				// Clear the highlight info in the tracker.
				_treeViewDragHighlightTracker.ClearHighlightNode();
				_currentSourceNode = null;

				// Move the source node based on the tracked info.
				if (destinationNode != null)
				{
					// Get the destination node 
					var sourceNode = (TreeNodeAdv)e.Data.GetData(typeof(TreeNodeAdv));

					//Clone the nodes to avoid the override issue
					var selectedNodesCollection = (SelectedNodesCollection)sourceNode.TreeView.SelectedNodes.Clone();

					foreach (TreeNodeAdv node in selectedNodesCollection)
					{
						var nodeInDestinationTree = treeViewAdvPreviewTree.FindNodesWithTagObject(node.TagObject).First();
					  
						switch (dropPosition)
						{
							case TreeViewDropPositions.AboveNode:
								{
									performNodeMoveInGroupPage(nodeInDestinationTree, destinationNode.Parent);
									nodeInDestinationTree.Move(destinationNode, NodePositions.Previous);
									break;
								}
							case TreeViewDropPositions.BelowNode:
								{
									performNodeMoveInGroupPage(nodeInDestinationTree, destinationNode.Parent);
									nodeInDestinationTree.Move(destinationNode, NodePositions.Next);
									break;
								}
							case TreeViewDropPositions.OnNode:
								{
									performNodeMoveInGroupPage(nodeInDestinationTree, destinationNode);
									nodeInDestinationTree.Move(destinationNode.Nodes);
									destinationNode.Expand();
									break;
								}
						}
					}

					//treeView.SelectedNode = sourceNode;
				}
			}
		}

		private void treeViewAdvPreviewTreeItemDrag(object sender, ItemDragEventArgs e)
		{
			var treeView = sender as TreeViewAdv;

			if (treeView != null)
			{
				// The TreeViewAdv always provides an array of selected nodes.
				var nodes = e.Item as TreeNodeAdv[];

				if (nodes != null && nodes.Length >= 1)
				{
					//check whether the root node also selected
					if (isRootSelected(nodes))
					{
						var ea = new QueryContinueDragEventArgs(1, true, DragAction.Continue);
						treeViewAdvPreviewTree_QueryContinueDrag(ea);
						return;
					}

					// Let us get only the first selected node.
					TreeNodeAdv node = nodes[0];
					// Only allow move
					treeView.DoDragDrop(node, DragDropEffects.Move);
				}
			}
		}

		private void treeViewAdvPreviewTree_QueryContinueDrag(QueryContinueDragEventArgs e)
		{
			// Cancel dragging when Escape was pressed.
			if (e.EscapePressed)
			{
				e.Action = DragAction.Cancel;
			}
		}

		private void treeDragDropQueryAllowedPositionsForNode(object sender, QueryAllowedPositionsEventArgs e)
		{
			if (isPersonNode(e.HighlightNode))
			{
				// If this a person node, only allow drop above or below it.
				if (e.HighlightNode != _currentSourceNode)
					e.AllowedPositions = TreeViewDropPositions.AboveNode | TreeViewDropPositions.BelowNode;
				else
					// Cannot drop beside itself
					e.AllowedPositions = TreeViewDropPositions.None;

				_treeViewDragHighlightTracker.EdgeSensitivityOnTop =
					_treeViewDragHighlightTracker.EdgeSensitivityAtBottom = e.HighlightNode.Bounds.Height / 2;
				e.ShowSelectionHighlight = false;
			}
			else
			{
				// If this is a group node allow all drop positions (default behavior).
				_treeViewDragHighlightTracker.EdgeSensitivityOnTop =
					_treeViewDragHighlightTracker.EdgeSensitivityAtBottom = e.HighlightNode.Bounds.Height / 4;
				e.ShowSelectionHighlight =
					// Only if the source node is droppable
					canDrop(_currentSourceNode, e.HighlightNode)
					// and droppable ON the node (not beside it)
					&& e.NewDropPosition == TreeViewDropPositions.OnNode;
			}
		}

		private void treeViewDragHighlightTrackerQueryDragInsertInfo(object sender, QueryDragInsertInfoEventArgs args)
		{
			args.DragInsertColor = _highlighter;
		}

		private void buttonAdvOkClick(object sender, EventArgs e)
		{
			_groupPageHelper.UpdateCurrent();

			DialogResult = DialogResult.OK;
		}

		private void buttonAdvCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void toolStripMenuItemNewGroupClick(object sender, EventArgs e)
		{
			handleCreateNewGroup();
		}

		private void contextMenuStripTreeActionsOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
			toolStripMenuItemCut.Enabled = true;
			toolStripMenuItemPaste.Enabled = true;
			toolStripMenuItemDelete.Enabled = false;
			toolStripMenuItemChangeNameOnGroup.Enabled = false;
			if (_rightMouseDownNodeCached == null) return;
			if (_cutSourceNode == null && findPersonsView1.CutNodes == null)
				toolStripMenuItemPaste.Enabled = false;
			if (string.Compare(GroupingConstants.NodeTypeRoot, _rightMouseDownNodeCached[0].Tag.ToString(),
							   StringComparison.OrdinalIgnoreCase) == 0)
			{
				toolStripMenuItemCut.Enabled = false;
			}
			else if (string.Compare(GroupingConstants.NodeTypePerson, _rightMouseDownNodeCached[0].Tag.ToString(),
							   StringComparison.OrdinalIgnoreCase) == 0)
			{
				toolStripMenuItemDelete.Enabled = canPerformPersonDeletion(_rightMouseDownNodeCached[0]); 
			}
			else if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
									StringComparison.OrdinalIgnoreCase) == 0)
			{
				toolStripMenuItemDelete.Enabled = canPerformGroupDeletion(_rightMouseDownNodeCached[0]); 
			}
			if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
							   StringComparison.OrdinalIgnoreCase) == 0)
			{
				toolStripMenuItemChangeNameOnGroup.Enabled = true;
			}
		}

		private void toolStripMenuItemDeleteClick(object sender, EventArgs e)
		{
			handleDeletion();
		}

		private void toolStripMenuItemCutClick(object sender, EventArgs e)
		{
			handleCut();
		}

		private void toolStripMenuItemPasteClick(object sender, EventArgs e)
		{
			handlePaste();
		}

		private void toolStripMenuItemChangeNameOnGroupClick(object sender, EventArgs e)
		{
			handleChangeNameOnGroup();
		}

		private void toolStripMenuItemCollapseAllNodesClick(object sender, EventArgs e)
		{
			handleCollapseAllNodes();
		}

		private void toolStripMenuItemExpandOnlyRootNodesClick(object sender, EventArgs e)
		{
			handleExpandOnlyRootNodes();

		}

		private void toolStripMenuItemExpandAllNodesClick(object sender, EventArgs e)
		{
			handleExpandAllNodes();

		}

		private void loadGroupPage()
		{
			if (GroupPage != null)
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					uow.Reassociate(GroupPage);
					IList<TreeNodeAdv> treeNodes = _treeViewCreator.CreateView(GroupPage, _myApplicationFunction);
					GroupPageName = GroupPage.Description.Name;
					constructTreeNodes(treeNodes, GroupPage.Description.ToString());
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(GroupPageName) &&
					GroupPageType != DynamicOptionType.BusinessHierarchy)
				{
					using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
					{
						uow.Reassociate(_groupPageHelper.PersonCollection);

						var treeConstructor = new TreeConstructor(_groupPageHelper);
						GroupPage = treeConstructor.CreateGroupPage(GroupPageName, GroupPageType, OptionalColumnId);
					}

					if (GroupPage != null)
					{
						IList<TreeNodeAdv> treeNodes = _treeViewCreator.CreateView(GroupPage, _myApplicationFunction);

						constructTreeNodes(treeNodes, GroupPage.Description.ToString());
					}
				}
			}
		}

		private bool canDrop(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			if ( // Support drag and drop only within the same tree
				sourceNode.TreeView != treeViewAdvPreviewTree ||
				// Cannot drop into empty area
				destinationNode == null ||
				// Cannot drop over the source's parent
				destinationNode == sourceNode.Parent ||
				// Or over itself
				destinationNode == sourceNode
				)
				return false;

			return true;
		}

		private static bool isRootSelected(IEnumerable<TreeNodeAdv> nodes)
		{
			IEnumerable<TreeNodeAdv> filteredNodes = Enumerable.Where(
														 nodes, n =>
													 string.Compare(GroupingConstants.NodeTypeRoot, n.Tag.ToString(),
																	StringComparison.OrdinalIgnoreCase) == 0);

			return filteredNodes.Any(node => node != null);
		}

		private static bool isPersonNode(TreeNodeAdv node)
		{
			if (node.Tag != null &&
				string.Compare(GroupingConstants.NodeTypePerson, node.Tag.ToString(), StringComparison.OrdinalIgnoreCase) ==
				0)
				return true;

			return false;
		}

		private void handleCreateNewGroup()
		{
			using (var nameDialog = new NameDialog("xxEnterNewGroupName", "xxNewGroupName", ""))
			{
				nameDialog.Text = UserTexts.Resources.NewGroupName;
				if (nameDialog.ShowDialog(this) == DialogResult.OK)
				{
					IRootPersonGroup rootPersonGroup = new RootPersonGroup(nameDialog.NameValue);
					GroupPage.AddRootPersonGroup(rootPersonGroup);

					var newGroupNode = new TreeNodeAdv(nameDialog.NameValue);
					newGroupNode.TagObject = rootPersonGroup;
					newGroupNode.Tag = GroupingConstants.NodeTypeGroup;
					newGroupNode.LeftImageIndices = new[] { 2 };

					treeViewAdvPreviewTree.BeginUpdate();

					if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
						treeViewAdvPreviewTree.RowIndexToNode(1).Nodes.Insert(0, newGroupNode);

					treeViewAdvPreviewTree.EndUpdate();

					treeViewAdvPreviewTree.CollapseAll();

					if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
						treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
				}
			}
		}

		private void handleChangeNameOnGroup()
		{
			if (_rightMouseDownNodeCached != null &&
				string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
							   StringComparison.OrdinalIgnoreCase) == 0)
			{
				treeViewAdvPreviewTree.BeginEdit(_rightMouseDownNodeCached[0]);
			}
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
		}
		
		private void performNodeMoveInGroupPage(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			if (sourceNode != null && sourceNode.TagObject != null &&
				destinationNode != null && destinationNode.TagObject != null)
			{
				if (string.Compare(GroupingConstants.NodeTypePerson, sourceNode.Tag.ToString(),
								   StringComparison.OrdinalIgnoreCase) == 0)
				{
					var nodeInDestinationTree = sourceNode;
					if (findPersonsView1.CutNodes != null)
					{
						var selectedNodesCollection = (SelectedNodesCollection)sourceNode.TreeView.SelectedNodes.Clone();
						var node = selectedNodesCollection[0];
						for (int i = 0; i < selectedNodesCollection.Count; i++)
						{
							if (selectedNodesCollection[i] == sourceNode)
								node = selectedNodesCollection[i];
						}
						nodeInDestinationTree = treeViewAdvPreviewTree.FindNodesWithTagObject(node.TagObject).First();
					}
					handlePersonMove(nodeInDestinationTree, destinationNode);
				}

				if (string.Compare(GroupingConstants.NodeTypeGroup, sourceNode.Tag.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
				{
					handleGroupMove(sourceNode, destinationNode);
				}
			}
		}

		private static void handlePersonMove(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			IPerson person = sourceNode.TagObject as Person;

			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

				(getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
			{
				//Add this person to the destination group
				addPerson(person, sourceNode, destinationNode);
			}

			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
			   getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.VirtualRoot))
			{
				if (sourceNode.Parent != null)
				{
					//Remove the person from the source group
					removePerson(sourceNode.Parent, person);
				}
			}

			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
			  getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

			  (getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
			  getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
			{
				if (sourceNode.Parent != null)
				{
					//Remove the person from the source group
					removePerson(sourceNode.Parent, person);

					//Add this person to the destination group
					addPerson(person, sourceNode, destinationNode);
				}
			}
		}

		private static void addPerson(IPerson person, TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			//Add this person to the destination group
			var personRootGroup = destinationNode.TagObject as IRootPersonGroup;
			var personChildGroup = destinationNode.TagObject as IChildPersonGroup;
			var destinationPerson = destinationNode.TagObject as IPerson;

			if (personRootGroup != null)
			{
				personRootGroup.AddPerson(person);
				sourceNode.Move(destinationNode.Nodes);
			}

			if (personChildGroup != null)
			{
				personChildGroup.AddPerson(person);
				sourceNode.Move(destinationNode.Nodes);
			}

			if (destinationPerson != null)
			{
				var parentNode = destinationNode.Parent.TagObject as IChildPersonGroup;
				var parentRootNode = destinationNode.Parent.TagObject as IRootPersonGroup;
				if (parentNode != null)
					parentNode.AddPerson(person);
				if (parentRootNode != null)
					parentRootNode.AddPerson(person);
				sourceNode.Move(destinationNode.Parent.Nodes);
			}
		}

		private static void removePerson(TreeNodeAdv treeNode, IPerson person)
		{
			//Remove this person from the source group
			var personRootGroup = treeNode.TagObject as IRootPersonGroup;
			var personChildGroup = treeNode.TagObject as IChildPersonGroup;

			if (personRootGroup != null)
				personRootGroup.RemovePerson(person);

			if (personChildGroup != null)
				personChildGroup.RemovePerson(person);
		}

		private static TreeElementLevel getTreeNodeLevel(int level)
		{
			var result = TreeElementLevel.Children;

			switch (level)
			{
				case 1:
					result = TreeElementLevel.VirtualRoot;
					break;
				case 2:
					result = TreeElementLevel.Root;
					break;
			}

			return result;
		}

		private static void addChildMembersToDestinatioNodeRecursively(TreeNodeAdv sourceNode, PersonGroupBase childPersonGroup)
		{
			if (sourceNode.HasChildren)
			{
				TreeNodeAdvCollection childNodeCollection = sourceNode.Nodes;

				foreach (TreeNodeAdv node in childNodeCollection)
				{
					if (string.Compare(GroupingConstants.NodeTypePerson, node.Tag.ToString(),
								 StringComparison.OrdinalIgnoreCase) == 0)
					{
						//Add person
						var person = node.TagObject as Person;

						if (person != null)
							childPersonGroup.AddPerson(person);
					}
					else
					{
						//Group Node add this
						var childGroup = node.TagObject as ChildPersonGroup;

						if (childGroup != null)
						{
							var cPersonGroup = new ChildPersonGroup(childGroup.Description.ToString());
							childPersonGroup.AddChildGroup(cPersonGroup);
							addChildMembersToDestinatioNodeRecursively(node, cPersonGroup);
						}
					}
				}
			}
		}

		private void handleGroupMove(TreeNodeAdv sourceNode, TreeNodeAdv destinationNode)
		{
			if (sourceNode.Equals(destinationNode))
				return;
			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

				(getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Root &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
			{
				//1
				// Add the source node items to the destination node recursively
				// Remove the source node items from the source node
				var sourceGroup = sourceNode.TagObject as IRootPersonGroup;
				var destinationGroup = destinationNode.TagObject as PersonGroupBase;

				if (sourceGroup != null && destinationGroup != null)
				{
					GroupPage.RemoveRootPersonGroup(sourceGroup);
					var childPersonGroup = new ChildPersonGroup(sourceGroup.Description.ToString());
					sourceNode.TagObject = childPersonGroup;
					//Add other sub tree recursively here
					addChildMembersToDestinatioNodeRecursively(sourceNode, childPersonGroup);
					destinationGroup.AddChildGroup(childPersonGroup);
				}
			}

			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.VirtualRoot))
			{
				//2
				var sourceRootPersonGroup = sourceNode.TagObject as ChildPersonGroup;
				var parentPersonGroupBase = sourceNode.Parent.TagObject as PersonGroupBase;    
 
				if (sourceRootPersonGroup != null && parentPersonGroupBase != null)
				{
					//remove the group from the parent of the source node
					parentPersonGroupBase.RemoveChildGroup(sourceRootPersonGroup);

					//add to root collection
					var rootPersonGroup = new RootPersonGroup(sourceRootPersonGroup.Description.ToString());
					sourceNode.TagObject = rootPersonGroup;
					//Add other sub tree recursively here
					addChildMembersToDestinatioNodeRecursively(sourceNode, rootPersonGroup);
					GroupPage.AddRootPersonGroup(rootPersonGroup);
				}
			}


			if ((getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Root) ||

				(getTreeNodeLevel(sourceNode.Level) == TreeElementLevel.Children &&
				getTreeNodeLevel(destinationNode.Level) == TreeElementLevel.Children))
			{
				//3
				//remove from it's parent
				// add to the node
				var sourceGroup = sourceNode.TagObject as ChildPersonGroup;
				var destinationGroup = destinationNode.TagObject as PersonGroupBase;
				var parentPersonGroupBase = sourceNode.Parent.TagObject as PersonGroupBase;

				if (sourceGroup != null && destinationGroup != null && parentPersonGroupBase != null)
				{
					parentPersonGroupBase.RemoveChildGroup(sourceGroup);
					var childPersonGroup = new ChildPersonGroup(sourceGroup.Description.ToString());
					sourceNode.TagObject = childPersonGroup;
					addChildMembersToDestinatioNodeRecursively(sourceNode, childPersonGroup);
					destinationGroup.AddChildGroup(childPersonGroup);
				}
			}

			if (_cutSourceNode != null)
			{
				for (var i = 0; i < _cutSourceNode.Count; i++)
				{
					_cutSourceNode[i].Move(_rightMouseDownNodeCached[0].Nodes, i);
				}
			}
		}

		private void constructTreeNodes(IEnumerable<TreeNodeAdv> treeNodes, string rootName)
		{
			treeViewAdvPreviewTree.BeginUpdate();

			var virtualNode = new TreeNodeAdv(rootName)
			{
			    TagObject = "RootGroup",
			    Tag = GroupingConstants.NodeTypeRoot,
			    LeftImageIndices = new[] {1},
			    RightImagePadding = 10
			};

		    foreach (TreeNodeAdv node in treeNodes)
				virtualNode.Nodes.Add(node);

			treeViewAdvPreviewTree.Nodes.Add(virtualNode);
			treeViewAdvPreviewTree.EndUpdate();

			if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
				treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
		}

		private void performPersonDeletion(TreeNodeAdv selectedNode)
		{
			if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children)
			{
				var person = selectedNode.TagObject as Person;

				if (person != null)
				{
					var parentGroup = selectedNode.Parent.TagObject as PersonGroupBase;

					if (parentGroup != null)
					{
						//Confirm before delete
						DialogResult response =
								ShowYesNoMessage(
									string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.PersonFromGroupingDeletionQuestion,
												 person.Name), UserTexts.Resources.ConfirmDeletion);

						if (response == DialogResult.Yes)
						{
							//Remove the person from the node and assign to the virtual root node
							parentGroup.RemovePerson(person);

							if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
								selectedNode.Move(treeViewAdvPreviewTree.RowIndexToNode(1).Nodes);
						}
					}
				}
			}
		}

		private static bool canPerformPersonDeletion(TreeNodeAdv selectedNode)
		{
			if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children)
			{
				var person = selectedNode.TagObject as Person;

				if (person != null)
				{
					var parentGroup = selectedNode.Parent.TagObject as PersonGroupBase;

					if (parentGroup != null)
					{
						return true;
					}
				}
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		private void performGroupDeletion(TreeNodeAdv selectedNode)
		{
			if (selectedNode.HasChildren)
			{
				ViewBase.ShowWarningMessage(UserTexts.Resources.TheGroupHasToBeEmptyBeforeDeleted, UserTexts.Resources.GroupIsNotEmpty);
			}
			else
			{
				if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Root)
				{
					var sourceGroup = selectedNode.TagObject as IRootPersonGroup;
					if (sourceGroup != null)
					{
						GroupPage.RemoveRootPersonGroup(sourceGroup);
						selectedNode.Remove();
						return;
					}
				}

				if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children &&
					selectedNode.Parent != null)
				{
					var sourceGroup = selectedNode.TagObject as IChildPersonGroup;
					var parentRootPersonGroup = selectedNode.Parent.TagObject as IRootPersonGroup;
					var parentChildPersonGroup = selectedNode.Parent.TagObject as IChildPersonGroup;

					if (parentRootPersonGroup != null)
					{
						parentRootPersonGroup.RemoveChildGroup(sourceGroup);
						selectedNode.Remove();
					}
					if (parentChildPersonGroup != null)
					{
						parentChildPersonGroup.RemoveChildGroup(sourceGroup);
						selectedNode.Remove();
					}
				}
			}
		}

		private static bool canPerformGroupDeletion(TreeNodeAdv selectedNode)
		{
			if (selectedNode.HasChildren)
			{
				return true;
			}

			if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Root)
			{
				return true;
			}

			if (getTreeNodeLevel(selectedNode.Level) == TreeElementLevel.Children &&
				selectedNode.Parent != null)
			{
				var parentRootPersonGroup = selectedNode.Parent.TagObject as IRootPersonGroup;
				var parentChildPersonGroup = selectedNode.Parent.TagObject as IChildPersonGroup;

				if (parentRootPersonGroup != null)
				{
					return true;
				}
				if (parentChildPersonGroup != null)
				{
					return true;
				}
			}
			return false;
		}

		private void handleDeletion()
		{
			if (_rightMouseDownNodeCached != null)
			{
				if (string.Compare(GroupingConstants.NodeTypePerson, _rightMouseDownNodeCached[0].Tag.ToString(),
							  StringComparison.OrdinalIgnoreCase) == 0)
				{
					performPersonDeletion(_rightMouseDownNodeCached[0]);
				}
				else if (string.Compare(GroupingConstants.NodeTypeGroup, _rightMouseDownNodeCached[0].Tag.ToString(),
							  StringComparison.OrdinalIgnoreCase) == 0)
				{
					performGroupDeletion(_rightMouseDownNodeCached[0]);
				}
			}
		}

		private void handleCollapseAllNodes()
		{
			if (treeViewAdvPreviewTree.RowIndexToNode(1).Expanded)
				treeViewAdvPreviewTree.CollapseAll();
		}

		private void handleExpandOnlyRootNodes()
		{
			if (treeViewAdvPreviewTree.RowIndexToNode(1).Expanded)
				treeViewAdvPreviewTree.CollapseAll();

			if (treeViewAdvPreviewTree.RowIndexToNode(1) != null)
				treeViewAdvPreviewTree.RowIndexToNode(1).Expand();
		}

		private void handleExpandAllNodes()
		{
			treeViewAdvPreviewTree.ExpandAll();
		}

		private void handleCut()
		{
			if (_rightMouseDownNodeCached != null)
			{
				_cutSourceNode = (SelectedNodesCollection) _rightMouseDownNodeCached.Clone();
			}
		}

		private void handlePaste()
		{
			if (findPersonsView1.CutNodes != null)
				_cutSourceNode = findPersonsView1.CutNodes;

			if (_rightMouseDownNodeCached != null)
			{
				if (_cutSourceNode != null)
				{
					for (var i = 0; i < _cutSourceNode.Count; i++)
					{
						performNodeMoveInGroupPage(_cutSourceNode[i], _rightMouseDownNodeCached[0]);   
					}
					_rightMouseDownNodeCached[0].Expand();
					_cutSourceNode = null;
					findPersonsView1.ClearNodes();
				}
			}
		}

		private void treeViewAdvPreviewTreeKeyDown(object sender, KeyEventArgs e)
		{
			 if (e.KeyCode == Keys.X && e.Modifiers == Keys.Control)
			 {
				 _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
				 handleCut();
			 }

			 if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control)
			 {
				 _rightMouseDownNodeCached = treeViewAdvPreviewTree.SelectedNodes;
				 handlePaste();
			 }

			 base.OnKeyDown(e);
		}
	}
}
