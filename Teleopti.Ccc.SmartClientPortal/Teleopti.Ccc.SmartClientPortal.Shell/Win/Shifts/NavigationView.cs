using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Payroll.DefinitionSets;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Shifts
{
	public partial class NavigationView : BaseUserControl, INavigationView
	{
		private readonly IEventAggregator _eventAggregator;
		private ShiftCreatorViewType _currentView = ShiftCreatorViewType.RuleSet;

		public ShiftCreatorViewType CurrentView => _currentView;

		public TreeViewAdv DefaultTreeView { get; set; }

		private string ContextMenuText
		{
			get
			{
				string text = UserTexts.Resources.ManageRuleSets;
				if (tabControlShiftCreator.SelectedIndex == 1)
					text = UserTexts.Resources.ManageRuleSets;
				return text;
			}
		}

		public NavigationView(IExplorerPresenter presenter, IEventAggregator eventAggregator)
			: base(presenter)
		{
			_eventAggregator = eventAggregator;
			if(presenter == null)
				throw new ArgumentNullException(nameof(presenter));

			InitializeComponent();
			SetTexts();
			createTreeView();

			createDefaultContextMenuStrips();
			createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			
			DefaultTreeView.Name = "treeViewRuleSet";
			ExplorerView.AddControlHelpContext(DefaultTreeView);

			ExplorerPresenter.GeneralPresenter.GeneralTemplatePresenter.OnlyForRestrictionsChanged += generalTemplatePresenterOnlyForRestrictionsChanged;
		}

		void generalTemplatePresenterOnlyForRestrictionsChanged(object sender, EventArgs e)
		{
			UpdateTreeIcons();
		}

		private void createTreeView()
		{
			DefaultTreeView = new TreeViewAdv
			{
				Style = TreeStyle.Metro,
				Dock = DockStyle.Fill,
				ShouldSelectNodeOnEnter = false,
				SelectionMode = TreeSelectionMode.MultiSelectAll,
				BorderStyle = BorderStyle.None,
				LeftImageList = imageList1,
				HideSelection = false,
				InactiveSelectedNodeBackground = new BrushInfo(Color.PaleTurquoise)
			};
			tabPageWorkShiftRule.Controls.Add(DefaultTreeView);
			tabPageWorkShiftRule.Focus();
			
			DefaultTreeView.NodeEditorValidating += treeViewNodeEditorValidating;
			DefaultTreeView.NodeEditorValidated += treeViewNodeEditorValidated;
			DefaultTreeView.AfterSelect += defaultTreeViewAfterSelect;
			DefaultTreeView.KeyUp += treeViewKeyUp;
			DefaultTreeView.MouseDown += treeViewMouseDown;
		}

		void treeViewNodeEditorValidating(object sender, TreeNodeAdvCancelableEditEventArgs e)
		{
			if(string.IsNullOrWhiteSpace(e.Label))
				e.Cancel = true;
		}

		private void treeViewNodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
		{
			if (DefaultTreeView.SelectedNode != null)
			{
				if (DefaultTreeView.SelectedNodes.Count == 1)
				{
					TreeNodeAdv currentNode = DefaultTreeView.SelectedNodes[0];
					try
					{
						var description = new Description(currentNode.Text);
						var ruleSet = currentNode.TagObject as IWorkShiftRuleSet;
						if (ruleSet != null)
						{
							ruleSet.Description = description;
							ExplorerPresenter.DataWorkHelper.Save(ruleSet);
						}
						else
						{
							((IRuleSetBag)currentNode.TagObject).Description = description;
							_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(((IRuleSetBag)currentNode.TagObject));
						}
					}
					catch (ArgumentOutOfRangeException)
					{
						showMessagebox(UserTexts.Resources.TheNameIsTooLong, UserTexts.Resources.TextTooLong);
						DefaultTreeView.BeginEdit();
						return;
					}
				}
			}
			DefaultTreeView.EndEdit();
		}

		void treeViewMouseDown(object sender, MouseEventArgs e)
		{
			// Ensure the correct view is active
			_currentView = ShiftCreatorViewType.RuleSet;
			if (tabControlShiftCreator.SelectedIndex == 1)
			{
				_currentView = ShiftCreatorViewType.RuleSetBag;
			}
			ExplorerPresenter.Model.SetSelectedView(_currentView);
		}

		private void treeViewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyValue == 46)
			{
				Delete();
			}
		}

		private void createRuleSetNodes(IEnumerable<IWorkShiftRuleSet> ruleSets)                          
		{
			DefaultTreeView.Nodes.Clear();
			DefaultTreeView.SelectedNodes.Clear();
			DefaultTreeView.BeginUpdate();
			foreach (IWorkShiftRuleSet ruleSet in ruleSets)
			{
				TreeNodeAdv ruleSetNode = createNode(ruleSet.Description.ToString(), ruleSet);
				List<IRuleSetBag> sortedBags = ruleSet.RuleSetBagCollection.ToList();
				sortedBags.Sort(
					delegate(IRuleSetBag left, IRuleSetBag right)
					{
						int value = -1;
							if (left.Description.Name != null && right.Description.Name != null)
								value = string.Compare(left.Description.Name, right.Description.Name, StringComparison.CurrentCulture);
						return value;
					});
				foreach (IRuleSetBag bag in sortedBags)
				{
					TreeNodeAdv bagNode = createNode(bag.Description.ToString(), bag);
					ruleSetNode.Nodes.Add(bagNode);
				}
				DefaultTreeView.Nodes.Add(ruleSetNode);
				if (ExplorerPresenter.Model.FilteredRuleSetCollection != null &&
					ExplorerPresenter.Model.FilteredRuleSetCollection.Contains(ruleSet))
					if (!DefaultTreeView.SelectedNodes.Contains(ruleSetNode))
						DefaultTreeView.SelectedNodes.Add(ruleSetNode);
			}

			DefaultTreeView.EndUpdate();

			if (DefaultTreeView.SelectedNodes.Count > 0)
				DefaultTreeView.Focus();
		}

		private void createRuleSetBagNodes(IEnumerable<IRuleSetBag> ruleSetBags)
		{
			DefaultTreeView.Nodes.Clear();
			DefaultTreeView.SelectedNodes.Clear();
			DefaultTreeView.BeginUpdate();
			foreach (IRuleSetBag ruleSetBag in ruleSetBags)
			{
				TreeNodeAdv ruleSetNode = createNode(ruleSetBag.Description.ToString(), ruleSetBag);
				List<IWorkShiftRuleSet> sortedList = ruleSetBag.RuleSetCollection.ToList();
				sortedList.Sort(delegate(IWorkShiftRuleSet left, IWorkShiftRuleSet right)
															{
																int value = -1;
																if (left.Description.Name != null && right.Description.Name != null)
																	value = string.Compare(left.Description.Name,
																						   right.Description.Name,
																						   StringComparison.CurrentCulture);
																return value;
															});
				foreach (IWorkShiftRuleSet ruleSet in sortedList)
				{
					TreeNodeAdv bagNode = createNode(ruleSet.Description.ToString(), ruleSet);
					ruleSetNode.Nodes.Add(bagNode);
				}
				DefaultTreeView.Nodes.Add(ruleSetNode);
				if (ExplorerPresenter.Model.FilteredRuleSetBagCollection != null &&
					ExplorerPresenter.Model.FilteredRuleSetBagCollection.Contains(ruleSetBag))
					if (!DefaultTreeView.SelectedNodes.Contains(ruleSetNode))
						DefaultTreeView.SelectedNodes.Add(ruleSetNode);
			}

			DefaultTreeView.EndUpdate();

			if (DefaultTreeView.SelectedNodes.Count > 0)
			{
				DefaultTreeView.Focus();
				ForceRefresh();
			}

		}

		public void UpdateTreeIcons()
		{
			foreach (TreeNodeAdv node in DefaultTreeView.Root.Nodes)
			{
				if (_currentView == ShiftCreatorViewType.RuleSetBag)
				{
					foreach (TreeNodeAdv bagNode in node.Nodes)
					{
						updateNodeIcon(bagNode);
					}
				}
				else
				{
					updateNodeIcon(node);
				}
			}
		}

		private static void updateNodeIcon(TreeNodeAdv node)
		{
			var ruleSet = node.TagObject as IWorkShiftRuleSet;

			if (ruleSet != null && ruleSet.OnlyForRestrictions)
				node.LeftImageIndices = new[] { 2 };
			else if(ruleSet != null)
				node.LeftImageIndices = new[] { 3 };	
		}

		private static TreeNodeAdv createNode(string name, object tagObject)
		{
			var node = new TreeNodeAdv { Text = name, TagObject = tagObject };
			updateNodeIcon(node);
			
			return node;
		}

		private void tabControlShiftCreatorSelectedIndexChanged(object sender, EventArgs e)
		{
			TabPageAdv from = tabPageRuleSetBag, to = tabPageWorkShiftRule;
			_currentView = ShiftCreatorViewType.RuleSet;

			DefaultTreeView.Name = "treeViewRuleSet";
			if (tabControlShiftCreator.SelectedIndex == 1)
			{
				_currentView = ShiftCreatorViewType.RuleSetBag;           
				from = tabPageWorkShiftRule;
				to = tabPageRuleSetBag;
				DefaultTreeView.Name = "treeViewBag";
			}
			if (from.Contains(DefaultTreeView))
			{
				try
				{
					from.Controls.Remove(DefaultTreeView);
				}
				catch (ArgumentNullException )
				{}
			}
			to.Controls.Add(DefaultTreeView);

			DefaultTreeView.AfterSelect -= defaultTreeViewAfterSelect;
			if (_currentView == ShiftCreatorViewType.RuleSet)
			{
				createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			}
			else if (_currentView == ShiftCreatorViewType.RuleSetBag)
			{
				createRuleSetBagNodes(ExplorerPresenter.Model.RuleSetBagCollection);
			}
			DefaultTreeView.AfterSelect += defaultTreeViewAfterSelect;

			ExplorerView.AddControlHelpContext(DefaultTreeView);
			ExplorerPresenter.Model.SetSelectedView(_currentView);

			if (DefaultTreeView.SelectedNodes != null && DefaultTreeView.SelectedNodes.Count > 0)
				DefaultTreeView.ContextMenuStrip.Items[3].Enabled = true;
			else
				DefaultTreeView.ContextMenuStrip.Items[3].Enabled = false;
			OnShowModifyCollection(this, EventArgs.Empty);

		}

		private void createDefaultContextMenuStrips()
		{
			var contextMenu = new ContextMenuStrip(components);

			ToolStripItem addMenuItem = new ToolStripMenuItem( (_currentView == ShiftCreatorViewType.RuleSet) ? UserTexts.Resources.NewRuleSet : UserTexts.Resources.NewRuleSetBag );
			addMenuItem.Click += delegate { Add(); };

			ToolStripItem item = new ToolStripMenuItem(ContextMenuText);
			item.Click += delegate {AssignRuleSet(); };
			
			ToolStripItem deleteMenuItem = new ToolStripMenuItem(UserTexts.Resources.Delete);
			deleteMenuItem.Click += delegate { Delete(); };
			ToolStripItem renameMenuItem = new ToolStripMenuItem(UserTexts.Resources.Rename);
			renameMenuItem.Click += delegate { Rename(); };

			contextMenu.Items.Add(deleteMenuItem);
			contextMenu.Items.Add(renameMenuItem);
			contextMenu.Items.Add(new ToolStripSeparator());
			contextMenu.Items.Add(item);
			contextMenu.Items[3].Enabled = false;

			DefaultTreeView.ContextMenuStrip = contextMenu;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void AssignRuleSet()
		{
			if (ExplorerPresenter.Model.FilteredRuleSetCollection==null || ExplorerPresenter.Model.FilteredRuleSetBagCollection==null)
			{
				showMessagebox(UserTexts.Resources.PleaseSelectANodeExclamation, UserTexts.Resources.NoNodeSelectedExclamation);
				return;
			}
			if (!(ExplorerPresenter.Model.FilteredRuleSetCollection.Count > 0 || ExplorerPresenter.Model.FilteredRuleSetBagCollection.Count > 0))
				return;
			PromptDialog dialog;
			if (_currentView == ShiftCreatorViewType.RuleSet)
			{
				if (ExplorerPresenter.Model.FilteredRuleSetCollection.Count == 0)
				{
					showMessagebox(UserTexts.Resources.PleaseSelectANodeExclamation, UserTexts.Resources.NoNodeSelectedExclamation);
					return;
				}
				ExplorerPresenter.Model.SetSelectedView(_currentView);
				dialog = new PromptDialog(ExplorerPresenter, ExplorerPresenter.Model.FilteredRuleSetCollection[0], _eventAggregator);
			}
			else
			{
				if (ExplorerPresenter.Model.FilteredRuleSetBagCollection.Count == 0)
				{
					showMessagebox(UserTexts.Resources.PleaseSelectANodeExclamation, UserTexts.Resources.NoNodeSelectedExclamation);
					return;
				}
				dialog = new PromptDialog(ExplorerPresenter, ExplorerPresenter.Model.FilteredRuleSetBagCollection[0], _eventAggregator);
			}
			dialog.ShowDialog(this);
			dialog.Dispose();
		}


		private static void showMessagebox(string errorMessage,string errorCaption)
		{
			ViewBase.ShowWarningMessage(errorMessage, errorCaption);
		}

		private void defaultTreeViewAfterSelect(object sender, EventArgs e)
		{
			Cursor = Cursors.WaitCursor;
			ExplorerView.ExitEditMode();
			loadSelectedRuleSets();
			Refresh(this, EventArgs.Empty);
			ExplorerPresenter.Model.SetSelectedView(_currentView);
			DefaultTreeView.ContextMenuStrip.Items[3].Enabled = true;
			OnShowModifyCollection(this, EventArgs.Empty);
			Cursor = Cursors.Default;
		}

		#region INavigationView Members

		public new event EventHandler<EventArgs> Refresh;
		public event EventHandler<EventArgs> ShowModifyCollection;

		protected virtual void OnShowModifyCollection(object sender, EventArgs e)
		{
			var onShowModifyCollection = ShowModifyCollection;
			if (onShowModifyCollection != null)
				onShowModifyCollection(this, e);
		}

		public void ChangeGridView(ShiftCreatorViewType viewType)
		{
			_currentView = viewType;
			if(_currentView == ShiftCreatorViewType.RuleSet)
				tabControlShiftCreator.SelectedIndex = 0;
			else if (_currentView == ShiftCreatorViewType.RuleSetBag)
				tabControlShiftCreator.SelectedIndex = 1;
			else
			{
				_currentView = ShiftCreatorViewType.RuleSet;
				if (tabControlShiftCreator.SelectedIndex == 1)
					_currentView = ShiftCreatorViewType.RuleSetBag;
			}
		}

		#endregion

		/// <summary>
		/// Adds the new.
		/// </summary>
		public override void Add()
		{
			endAndSaveCurrentEditing(); // tamasb: I did not find better way to force ending the current edit (#26260: Usability: New rule set's name is lost)
			string name = string.Empty;
			object tagObject = null;
				try
				{
					if(_currentView == ShiftCreatorViewType.RuleSet)
					{
							IWorkShiftRuleSet ruleSet = ExplorerPresenter.NavigationPresenter.CreateDefaultRuleSet();
							ExplorerPresenter.Model.AddRuleSet(ruleSet);
							ExplorerPresenter.DataWorkHelper.Save(ruleSet);
							name = ruleSet.Description.ToString();
							tagObject = ruleSet;
					}
					else if(_currentView == ShiftCreatorViewType.RuleSetBag)
					{
						IRuleSetBag ruleSetBag = ExplorerPresenter.NavigationPresenter.CreateDefaultRuleSetBag();
						ExplorerPresenter.Model.AddRuleSetBag(ruleSetBag);
						
						name = ruleSetBag.Description.ToString();
						tagObject = ruleSetBag;
					}
					DefaultTreeView.BeginEdit();

					TreeNodeAdv node = createNode(name, tagObject);
					DefaultTreeView.Nodes.Add(node);
					DefaultTreeView.SelectedNode = node;

					DefaultTreeView.EndEdit();
					Rename();
				}
				catch (ArgumentOutOfRangeException ex)
				{
					showMessagebox(UserTexts.Resources.YouHaveToHaveAtLeastOnActivityAndShiftCategoryBeforeCreatingARuleset,UserTexts.Resources.ErrorMessage + " " + ex );
				}
		}

		private void endAndSaveCurrentEditing()
		{
			DefaultTreeView.EndEdit();
		}

		public override void Delete()
		{
			if (ExplorerView.AskForDelete())
			{
				deleteSelectedWorkShiftsAndBags();
				RefreshView();
			}
		}

		private void deleteSelectedWorkShiftsAndBags()
		{
			int selectedNodeIndex = (DefaultTreeView.SelectedNodes.Count - 1);
			for (int i = selectedNodeIndex; i >= 0; i--)
			{
				TreeNodeAdv selectedNode = DefaultTreeView.SelectedNodes[i];
				var ruleSetToDelete = selectedNode.TagObject as IWorkShiftRuleSet;

				if (ruleSetToDelete != null)
				{
					var parentBag = selectedNode.Parent.TagObject as IRuleSetBag;
					ExplorerPresenter.NavigationPresenter.RemoveRuleSet(ruleSetToDelete, parentBag);
					if (parentBag != null)
						_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(parentBag);
				}
			}
			for (int i = selectedNodeIndex; i >= 0; i--)
			{
				TreeNodeAdv selectedNode = DefaultTreeView.SelectedNodes[i];
				var bagToDelete = selectedNode.TagObject as IRuleSetBag;
				if (bagToDelete != null)
				{
					ExplorerPresenter.NavigationPresenter.RemoveRuleSetBag(bagToDelete);
					_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(bagToDelete);
				}
			}
			for (int i = selectedNodeIndex; i >= 0; i--)
			{
				TreeNodeAdv selectedNode = DefaultTreeView.SelectedNodes[i];
				DefaultTreeView.SelectedNodes.Remove(selectedNode);
			}
		}

		
		/// <summary>
		/// Refreshes the view.
		/// </summary>
		public override void RefreshView()
		{
			DefaultTreeView.AfterSelect -= defaultTreeViewAfterSelect;
			if (_currentView == ShiftCreatorViewType.RuleSet)
				createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			else
				createRuleSetBagNodes(ExplorerPresenter.Model.RuleSetBagCollection);
			DefaultTreeView.AfterSelect += defaultTreeViewAfterSelect;
		}

		/// <summary>
		/// Renames this instance.
		/// </summary>
		public override void Rename()
		{
			if (DefaultTreeView.SelectedNodes.Count == 1)
				DefaultTreeView.BeginEdit(DefaultTreeView.SelectedNodes[0]);
		}

		public override void Cut()
		{
			Copy();
			deleteSelectedWorkShiftsAndBags();
		}

		public override void Copy()
		{
			ClipboardHandler.Instance.CopySelection(DefaultTreeView);
		}

		public override void Paste()
		{
			foreach (ICloneable originalEntity in ClipboardHandler.Instance.Clips)
			{
				var originalRuleSet = originalEntity as IWorkShiftRuleSet;
				var originalRuleSetBag = originalEntity as IRuleSetBag;
				TreeNodeAdv newNode = null;
				if (originalRuleSet != null && _currentView == ShiftCreatorViewType.RuleSet)
				{
					IWorkShiftRuleSet newRuleSet = originalRuleSet.NoneEntityClone();
					newRuleSet.Description =
						string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSet.Description.Name).Length > 50
							? new Description(
								  string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSet.Description.Name).Substring(0, 50))
							: new Description(string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSet.Description.Name));
					ExplorerPresenter.Model.AddRuleSet(newRuleSet);
					ExplorerPresenter.DataWorkHelper.Save(newRuleSet);
					newNode = createNode(newRuleSet.Description.ToString(), newRuleSet);
					foreach (IRuleSetBag ruleSetBag in newRuleSet.RuleSetBagCollection)
					{
						TreeNodeAdv newRuleSetBagNode = createNode(ruleSetBag.Description.ToString(), ruleSetBag);
						newNode.Nodes.Add(newRuleSetBagNode);
					}
				}
				else if (originalRuleSetBag != null && _currentView == ShiftCreatorViewType.RuleSetBag)
				{
					IRuleSetBag newRuleSetBag = originalRuleSetBag.NoneEntityClone();
					newRuleSetBag.Description =
						string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSetBag.Description.Name).Length > 50
							? new Description(
								  string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSetBag.Description.Name).Substring(0, 50))
							: new Description(string.Concat(UserTexts.Resources.CopyOf, " ", newRuleSetBag.Description.Name));
					ExplorerPresenter.Model.AddRuleSetBag(newRuleSetBag);
					_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(newRuleSetBag);
					
					newNode = createNode(string.Concat(UserTexts.Resources.CopyOf, " ", originalRuleSetBag.Description.Name), newRuleSetBag);
					foreach (IWorkShiftRuleSet ruleSet in newRuleSetBag.RuleSetCollection)
					{
						TreeNodeAdv newRuleSetNode = createNode(ruleSet.Description.ToString(), ruleSet);
						newNode.Nodes.Add(newRuleSetNode);
					}
				}
				if (newNode != null)
				{
					DefaultTreeView.Nodes.Add(newNode);
					DefaultTreeView.Refresh();
				}
			}
		}

		public void ForceRefresh()
		{
			defaultTreeViewAfterSelect(DefaultTreeView, EventArgs.Empty);
		}

		private void loadSelectedRuleSets()
		{
			var selectedRuleSets = new List<IWorkShiftRuleSet>();
			var selectedRuleSetBags = new List<IRuleSetBag>();
			ExplorerPresenter.Model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(selectedRuleSets));

			int selectedNodeCount = DefaultTreeView.SelectedNodes.Count;
			for (int i = 0; i <= (selectedNodeCount - 1); i++)
			{
				TreeNodeAdv currentNode = DefaultTreeView.SelectedNodes[i];
				if (currentNode.TagObject is IWorkShiftRuleSet)
				{
					if (currentNode.IsSelected)
					{
						selectedRuleSets.Add(currentNode.TagObject as IWorkShiftRuleSet);
					}
					else
					{
						foreach (TreeNodeAdv childNode in currentNode.Nodes)
						{
							if (childNode.IsSelected)
							{
								var selectedBag = childNode.TagObject as IRuleSetBag;
								if (selectedBag != null)
									selectedRuleSets.AddRange(selectedBag.RuleSetCollection);
							}
						}
					}
					if (_currentView == ShiftCreatorViewType.RuleSetBag)
					{
						var parentShiftbag = currentNode.Parent.TagObject as IRuleSetBag;
						if (parentShiftbag != null)
						{
							// Select or filter out the rule sets parent shift bag in tree view
							selectedRuleSetBags.Add(parentShiftbag);
						}
					}
				}
				else
				{
					var selectedBag = currentNode.TagObject as IRuleSetBag;
					if (selectedBag != null)
					{
						selectedRuleSets.AddRange(selectedBag.RuleSetCollection.OrderBy(r => r.Description.Name).ToList());
						selectedRuleSetBags.Add(selectedBag);
					}
				}
			}

			var callback = new WorkShiftAddCallback();
			callback.RuleSetToComplex += callbackRuleSetToComplex;
			using (var status = new ShiftGenerationStatus(callback))
			{
				ExplorerView.RefreshActivityGridView();
				ExplorerPresenter.View.SetViewEnabled(false);
				status.ShowDelayed(this);
				ExplorerPresenter.Model.SetFilteredRuleSetCollection(
					new ReadOnlyCollection<IWorkShiftRuleSet>(selectedRuleSets));
				ExplorerPresenter.Model.SetFilteredRuleSetBagCollection(
					new ReadOnlyCollection<IRuleSetBag>(selectedRuleSetBags));
				ExplorerPresenter.GeneralPresenter.LoadModelCollection();
				ExplorerPresenter.VisualizePresenter.LoadModelCollection(callback); // |--|
					
				status.Visible = false;
				ExplorerPresenter.View.SetViewEnabled(true);
				ExplorerView.Activate();
				DefaultTreeView.Select();
				callback.RuleSetToComplex -= callbackRuleSetToComplex;
			}
			
		}

		void callbackRuleSetToComplex(object sender, ComplexRuleSetEventArgs e)
		{
			ViewBase.ShowErrorMessage(this, UserTexts.Resources.ShiftGenerationStop, e.RuleSetName + UserTexts.Resources.TooComplexRuleset);
		}
	}
}
