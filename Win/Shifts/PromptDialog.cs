using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Shifts;
using Teleopti.Ccc.WinCode.Shifts.Events;
using Teleopti.Ccc.WinCode.Shifts.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Shifts
{
	public partial class PromptDialog : BaseDialogForm
	{
		private readonly IExplorerPresenter _explorerPresenter;
		private readonly IWorkShiftRuleSet _ruleSet;
		private readonly IRuleSetBag _ruleSetBag;
		private readonly IEventAggregator _eventAggregator;

		public PromptDialog()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				SetTexts();
			}
		}

		public PromptDialog(IExplorerPresenter presenter, IWorkShiftRuleSet ruleSet, IEventAggregator eventAggregator)
			: this()
		{
			_explorerPresenter = presenter;
			_ruleSet = ruleSet;
			_eventAggregator = eventAggregator;
		}

		public PromptDialog(IExplorerPresenter presenter, IRuleSetBag ruleSetBag, IEventAggregator eventAggregator)
			: this()
		{
			_explorerPresenter = presenter;
			_ruleSetBag = ruleSetBag;
			_eventAggregator = eventAggregator;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (_explorerPresenter.Model.SelectedView == ShiftCreatorViewType.RuleSet)
				createRuleSetBagNodes(_explorerPresenter.Model.RuleSetBagCollection);
			else
				createRuleSetNodes(_explorerPresenter.Model.RuleSetCollection);

			Text = UserTexts.Resources.SelectBags;
			if (_explorerPresenter.Model.SelectedView != ShiftCreatorViewType.RuleSet)
				Text = UserTexts.Resources.SelectRuleSets;
			//to handle bug? in syncfusion when Space is pressed
			KeyDown += formKeyDown;
			KeyPress += formKeyPress;
		}

		void formKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyValue.Equals(32))
			{
				foreach (TreeNodeAdv selectedNode in treeViewAdvLoV.SelectedNodes)
				{
					selectedNode.Checked = !selectedNode.Checked;
				}
				e.Handled = true;
			}
		}

		void formKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar.Equals((Char)Keys.Space))
			{
				e.Handled = true;
			}
		}

		private void createRuleSetBagNodes(IEnumerable<IRuleSetBag> ruleSetBags)
		{
			treeViewAdvLoV.Nodes.Clear();
			treeViewAdvLoV.BeginUpdate();
			foreach (IRuleSetBag ruleSetBag in ruleSetBags)
			{
				TreeNodeAdv node = createNode(ruleSetBag.Description.ToString(), ruleSetBag);
				node.ShowPlusMinus = false;
				if (_ruleSet.RuleSetBagCollection.Contains(ruleSetBag))
					node.CheckState = CheckState.Checked;
				treeViewAdvLoV.Nodes.Add(node);
			}
			treeViewAdvLoV.EndUpdate();
		}

		private void createRuleSetNodes(IEnumerable<IWorkShiftRuleSet> ruleSets)
		{
			treeViewAdvLoV.Nodes.Clear();
			treeViewAdvLoV.BeginUpdate();
			foreach (IWorkShiftRuleSet ruleSet in ruleSets)
			{
				TreeNodeAdv node = createNode(ruleSet.Description.ToString(), ruleSet);
				node.ShowPlusMinus = false;
				if (_ruleSetBag.RuleSetCollection.Contains(ruleSet))
					node.CheckState = CheckState.Checked;
				treeViewAdvLoV.Nodes.Add(node);
			}
			treeViewAdvLoV.EndUpdate();
		}

		private static TreeNodeAdv createNode(string name, object tagObject)
		{
			var node = new TreeNodeAdv { Text = name, TagObject = tagObject };
			return node;
		}

		private void btnOkClick(object sender, EventArgs e)
		{
			IEnumerable<TreeNodeAdv> treeNodes = treeViewAdvLoV.CheckedNodes.Cast<TreeNodeAdv>();
			
			if (_explorerPresenter.Model.SelectedView == ShiftCreatorViewType.RuleSet)
			{
				_eventAggregator.GetEvent<RuleSetChanged>().Publish(new List<IWorkShiftRuleSet> { _ruleSet });
				foreach (TreeNodeAdv adv in treeViewAdvLoV.Nodes)
				{
					IRuleSetBag bag;
					if (adv.Level == 1)
					{
						bag = ((IRuleSetBag) adv.TagObject);
					}
					else
					{
						bag = (IRuleSetBag)adv.Parent.TagObject;
					}
   
					if (!_ruleSet.RuleSetBagCollection.Contains(bag) && adv.Checked)
					{
						((IRuleSetBag)adv.TagObject).AddRuleSet(_ruleSet);
						_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(bag);
					}
					if (_ruleSet.RuleSetBagCollection.Contains(bag) && !adv.Checked)
					{
						((IRuleSetBag)adv.TagObject).RemoveRuleSet(_ruleSet);
						_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(bag);
					}
				}
			}       
			else
			{
				_ruleSetBag.ClearRuleSetCollection();
				_eventAggregator.GetEvent<RuleSetBagChanged>().Publish(_ruleSetBag);
				foreach (TreeNodeAdv adv in treeNodes)
				{
					if (adv.Level == 1)
					{
						if (!_ruleSetBag.RuleSetCollection.Contains(((IWorkShiftRuleSet) adv.TagObject)))
							_ruleSetBag.AddRuleSet((IWorkShiftRuleSet) adv.TagObject);
					}
					else
					{
						var set = (IWorkShiftRuleSet)adv.Parent.TagObject;
						if (!((IRuleSetBag)adv.TagObject).RuleSetCollection.Contains(set))
							((IRuleSetBag)adv.TagObject).AddRuleSet(set);
					}
				}
			}

			_explorerPresenter.View.RefreshChildViews();
			Close();
		}

		private void promptDialogFormClosed(object sender, FormClosedEventArgs e)
		{
			KeyDown -= formKeyDown;
			KeyPress -= formKeyPress;
		}
	}
}
