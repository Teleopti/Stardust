using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Payroll.DefinitionSets;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.WinCode.Shifts.Views;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
	public partial class NavigationView : BaseUserControl, INavigationView
	{
		private readonly IEventAggregator _eventAggregator;
		private TreeViewAdv _defaultTreeView;
		private ShiftCreatorViewType _currentView = ShiftCreatorViewType.RuleSet;

		public ShiftCreatorViewType CurrentView
		{
			get { return _currentView; }
		}

		public TreeViewAdv DefaultTreeView
		{
			get { return _defaultTreeView; }
			set { _defaultTreeView = value; }
		}

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
				throw new ArgumentNullException("presenter");

			InitializeComponent();
			SetTexts();
			createTreeView();

			createDefaultContextMenuStrips();
			createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			
			_defaultTreeView.Name = "treeViewRuleSet";
			ExplorerView.AddControlHelpContext(_defaultTreeView);

			presenter.GeneralPresenter.GeneralTemplatePresenter.OnlyForRestrictionsChanged += generalTemplatePresenterOnlyForRestrictionsChanged;
		}

		void generalTemplatePresenterOnlyForRestrictionsChanged(object sender, EventArgs e)
		{
			UpdateTreeIcons();
		}

		private void createTreeView()
		{
			_defaultTreeView = new TreeViewAdv
			{
				Style = TreeStyle.Metro,
				Dock = DockStyle.Fill,
				ShouldSelectNodeOnEnter = false,
				SelectionMode = TreeSelectionMode.MultiSelectAll,
				BorderStyle = BorderStyle.None
			};
			tabPageWorkShiftRule.Controls.Add(_defaultTreeView);
			tabPageWorkShiftRule.Focus();
			
			_defaultTreeView.NodeEditorValidating += treeViewNodeEditorValidating;
			_defaultTreeView.NodeEditorValidated += treeViewNodeEditorValidated;
			_defaultTreeView.AfterSelect += defaultTreeViewAfterSelect;
			_defaultTreeView.KeyUp += treeViewKeyUp;
			_defaultTreeView.MouseDown += treeViewMouseDown;
		}

		void treeViewNodeEditorValidating(object sender, TreeNodeAdvCancelableEditEventArgs e)
		{
			if(string.IsNullOrEmpty(e.Label))
				e.Cancel = true;
		}

		private void treeViewNodeEditorValidated(object sender, TreeNodeAdvEditEventArgs e)
		{
			if (_defaultTreeView.SelectedNode != null)
			{
				if (_defaultTreeView.SelectedNodes.Count == 1)
				{
					TreeNodeAdv currentNode = _defaultTreeView.SelectedNodes[0];
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
						_defaultTreeView.BeginEdit();
						return;
					}
				}
			}
			_defaultTreeView.EndEdit();
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
			_defaultTreeView.Nodes.Clear();
			_defaultTreeView.SelectedNodes.Clear();
			_defaultTreeView.BeginUpdate();
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
				_defaultTreeView.Nodes.Add(ruleSetNode);
				if (ExplorerPresenter.Model.FilteredRuleSetCollection != null &&
					ExplorerPresenter.Model.FilteredRuleSetCollection.Contains(ruleSet))
					if (!_defaultTreeView.SelectedNodes.Contains(ruleSetNode))
						_defaultTreeView.SelectedNodes.Add(ruleSetNode);
			}

			_defaultTreeView.EndUpdate();

			if (_defaultTreeView.SelectedNodes.Count > 0)
				_defaultTreeView.Focus();
		}

		private void createRuleSetBagNodes(IEnumerable<IRuleSetBag> ruleSetBags)
		{
			_defaultTreeView.Nodes.Clear();
			_defaultTreeView.SelectedNodes.Clear();
			_defaultTreeView.BeginUpdate();
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
				_defaultTreeView.Nodes.Add(ruleSetNode);
				if (ExplorerPresenter.Model.FilteredRuleSetBagCollection != null &&
					ExplorerPresenter.Model.FilteredRuleSetBagCollection.Contains(ruleSetBag))
					if (!_defaultTreeView.SelectedNodes.Contains(ruleSetNode))
						_defaultTreeView.SelectedNodes.Add(ruleSetNode);
			}

			_defaultTreeView.EndUpdate();

			if (_defaultTreeView.SelectedNodes.Count > 0)
			{
				_defaultTreeView.Focus();
				ForceRefresh();
			}

		}

		public void UpdateTreeIcons()
		{
			foreach (TreeNodeAdv node in _defaultTreeView.Root.Nodes)
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
				node.LeftImageIndices = new[] { 0 };
			else if(ruleSet != null)
				node.LeftImageIndices = new[] { 1 };	
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

			_defaultTreeView.Name = "treeViewRuleSet";
			if (tabControlShiftCreator.SelectedIndex == 1)
			{
				_currentView = ShiftCreatorViewType.RuleSetBag;           
				from = tabPageWorkShiftRule;
				to = tabPageRuleSetBag;
				_defaultTreeView.Name = "treeViewBag";
			}
			if (from.Contains(_defaultTreeView))
				from.Controls.Remove(_defaultTreeView);
			to.Controls.Add(_defaultTreeView);

			_defaultTreeView.AfterSelect -= defaultTreeViewAfterSelect;
			if (_currentView == ShiftCreatorViewType.RuleSet)
			{
				createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			}
			else if (_currentView == ShiftCreatorViewType.RuleSetBag)
			{
				createRuleSetBagNodes(ExplorerPresenter.Model.RuleSetBagCollection);
			}
			_defaultTreeView.AfterSelect += defaultTreeViewAfterSelect;

			ExplorerView.AddControlHelpContext(_defaultTreeView);
			ExplorerPresenter.Model.SetSelectedView(_currentView);

			if (_defaultTreeView.SelectedNodes != null && _defaultTreeView.SelectedNodes.Count > 0)
				_defaultTreeView.ContextMenuStrip.Items[3].Enabled = true;
			else
				_defaultTreeView.ContextMenuStrip.Items[3].Enabled = false;
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

			_defaultTreeView.ContextMenuStrip = contextMenu;
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
			_defaultTreeView.ContextMenuStrip.Items[3].Enabled = true;
			OnShowModifyCollection(this, EventArgs.Empty);
			Cursor = Cursors.Default;
		}

		#region INavigationView Members

		public new event EventHandler<EventArgs> Refresh;
		public event EventHandler<EventArgs> ShowModifyCollection;

		protected virtual void OnShowModifyCollection(object sender, EventArgs e)
		{
			if (ShowModifyCollection != null)
				ShowModifyCollection(this, e);
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
					_defaultTreeView.BeginEdit();

					TreeNodeAdv node = createNode(name, tagObject);
					_defaultTreeView.Nodes.Add(node);
					_defaultTreeView.SelectedNode = node;

					_defaultTreeView.EndEdit();
					Rename();
				}
				catch (ArgumentOutOfRangeException ex)
				{
					showMessagebox(UserTexts.Resources.YouHaveToHaveAtLeastOnActivityAndShiftCategoryBeforeCreatingARuleset,UserTexts.Resources.ErrorMessage + " " + ex );
				}
		}

		private void endAndSaveCurrentEditing()
		{
			_defaultTreeView.EndEdit();
		}

		public override void Delete()
		{
			if (ExplorerView.AskForDelete())
			{
				deleteSelected();
				RefreshView();
			}
		}

		private void deleteSelected()
		{
			int selectedNodeCount = (_defaultTreeView.SelectedNodes.Count - 1);
			for (int i = selectedNodeCount; i >= 0; i--)
			{
				TreeNodeAdv selectedNode = _defaultTreeView.SelectedNodes[i];
				var workShiftRuleSetToDelete = selectedNode.TagObject as IWorkShiftRuleSet;
				var ruleSetBagToDelete = selectedNode.TagObject as IRuleSetBag;
				bool designatedView = (_currentView == ShiftCreatorViewType.RuleSet && workShiftRuleSetToDelete != null) ||
									  (_currentView == ShiftCreatorViewType.RuleSetBag && ruleSetBagToDelete != null);

				IWorkShiftRuleSet workShiftRuleSetToDeleteParent = null;
				IRuleSetBag ruleSetBagToDeleteParent= null;
				if (!designatedView &&
					selectedNode.Parent!=null)
				{
					workShiftRuleSetToDeleteParent = selectedNode.Parent.TagObject as IWorkShiftRuleSet;
					ruleSetBagToDeleteParent = selectedNode.Parent.TagObject as IRuleSetBag;
				}

				if (workShiftRuleSetToDelete != null)
				{
					ExplorerPresenter.NavigationPresenter.RemoveRuleSet(workShiftRuleSetToDelete, ruleSetBagToDeleteParent);
					if(ruleSetBagToDeleteParent != null)
						_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(ruleSetBagToDeleteParent);
				}

				if (ruleSetBagToDelete != null)
				{
					ExplorerPresenter.NavigationPresenter.RemoveRuleSetBag(ruleSetBagToDelete,workShiftRuleSetToDeleteParent );
				}

				_defaultTreeView.Nodes.Remove(selectedNode);
			}
		}

		/// <summary>
		/// Refreshes the view.
		/// </summary>
		public override void RefreshView()
		{
			_defaultTreeView.AfterSelect -= defaultTreeViewAfterSelect;
			if (_currentView == ShiftCreatorViewType.RuleSet)
				createRuleSetNodes(ExplorerPresenter.Model.RuleSetCollection);
			else
				createRuleSetBagNodes(ExplorerPresenter.Model.RuleSetBagCollection);
			_defaultTreeView.AfterSelect += defaultTreeViewAfterSelect;
		}

		/// <summary>
		/// Renames this instance.
		/// </summary>
		public override void Rename()
		{
			if (_defaultTreeView.SelectedNodes.Count == 1)
				_defaultTreeView.BeginEdit(_defaultTreeView.SelectedNodes[0]);
		}

		public override void Cut()
		{
			Copy();
			deleteSelected();
		}

		public override void Copy()
		{
			ClipboardHandler.Instance.CopySelection(_defaultTreeView);
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
					_defaultTreeView.Nodes.Add(newNode);
					_defaultTreeView.Refresh();
				}
			}
		}

		public void ForceRefresh()
		{
			defaultTreeViewAfterSelect(_defaultTreeView, EventArgs.Empty);
		}

		private void loadSelectedRuleSets()
		{
			var selectedRuleSets = new List<IWorkShiftRuleSet>();
			var selectedRuleSetBags = new List<IRuleSetBag>();
			ExplorerPresenter.Model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(selectedRuleSets));

			int selectedNodeCount = _defaultTreeView.SelectedNodes.Count;
			for (int i = 0; i <= (selectedNodeCount - 1); i++)
			{
				TreeNodeAdv currentNode = _defaultTreeView.SelectedNodes[i];
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
				_defaultTreeView.Select();
				callback.RuleSetToComplex -= callbackRuleSetToComplex;
			}
			
		}

		void callbackRuleSetToComplex(object sender, ComplexRuleSetEventArgs e)
		{
			ViewBase.ShowErrorMessage(this, UserTexts.Resources.ShiftGenerationStop, e.RuleSetName + UserTexts.Resources.TooComplexRuleset);
		}
	}
}
