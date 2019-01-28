using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Views;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Events;
using Wizard = Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard.Wizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
{
	//ARGHH... To much code in this thing.... Own presenter for Tree?
	//I might create my own datasource binding for the tree
	public partial class BudgetGroupGroupNavigatorView : AbstractNavigator, IBudgetGroupNavigatorView
	{
		private readonly IBudgetGroupMainViewFactory _budgetGroupMainViewFactory;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler;
		private readonly IEventAggregator _globalEventAggregator;

		public BudgetGroupGroupNavigatorView(IBudgetGroupMainViewFactory budgetGroupMainViewFactory,IEventAggregatorLocator eventAggregatorLocator, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory, IGracefulDataSourceExceptionHandler dataSourceExceptionHandler)
		{
			_dataSourceExceptionHandler = dataSourceExceptionHandler;
			_budgetGroupMainViewFactory = budgetGroupMainViewFactory;
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();
			_repositoryFactory = repositoryFactory;
			_unitOfWorkFactory = unitOfWorkFactory;
			InitializeComponent();
			
			if (!DesignMode)
			{
				SetTexts();
				checkPermissons();
				eventSubscription();
			}
		}

		private void eventSubscription()
		{
			_globalEventAggregator.GetEvent<BudgetGroupTreeNeedsRefresh>().Subscribe(budgetGroupTreeNeedsRefresh);
		}

		private void budgetGroupTreeNeedsRefresh(string obj)
		{
			using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var selectedBudgetGroup = SelectedModel.ContainedEntity;
				if (selectedBudgetGroup == null) return;
				var budgetGroup = _repositoryFactory.CreateBudgetGroupRepository(uow).Get(selectedBudgetGroup.Id.GetValueOrDefault());
				updateTree(budgetGroup);
			}
		}

		private void checkPermissons()
		{
			splitContainer1.Visible = PrincipalAuthorization.Current_DONTUSE().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenBudgets);
		}

		public BudgetGroupNavigatorPresenter Presenter { get; set; }

		public IBudgetModel SelectedModel { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public BudgetGroupRootModel BudgetGroupRootModel
		{
			set { addBudgetGroups(value); }
			get { throw new NotImplementedException(); }
		}

		public int BudgetingActionPaneHeight
		{
			get { return splitContainer1.Height - splitContainer1.SplitterDistance; }
			set 
			{
				var calculatedSplitterDistance = splitContainer1.Height - value;
				
				if(calculatedSplitterDistance > 0) 
					splitContainer1.SplitterDistance = calculatedSplitterDistance;
			}
		}

		private void showWizard()
		{
			using (var page = new BudgetGroupWizardPage(_repositoryFactory,_unitOfWorkFactory))
			{
				page.Initialize(PropertyPagesHelper.BudgetGroupPages(), new LazyLoadingManagerWrapper());
				
				using (var wizard = new Wizard(page))
				{
					page.Saved += pageAfterSave;
					wizard.ShowDialog(this);
				}
			}
		}

		private void pageAfterSave(object sender, AfterSavedEventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
				var planningGroupPropertyPages = (AbstractPropertyPages<IBudgetGroup>)sender;
				planningGroupPropertyPages.LoadAggregateRootWorkingCopy();
				var createdNode = planningGroupPropertyPages.AggregateRootObject;
				updateTree(createdNode);
			});
		}

		#region TreeStuff

		private void updateTree(IBudgetGroup budgetGroup)
		{
			updateTree();
			selectNode(budgetGroup);
		}

		private void updateTree()
		{
			treeViewAdvBudgetGroups.Nodes.Clear();
			Presenter.Initialize();
		}

		private void selectNode(IBudgetGroup createdNode)
		{
			foreach (TreeNodeAdv node in treeViewAdvBudgetGroups.Nodes)
			{
				node.Expand();
				foreach (TreeNodeAdv budgetNode in node.Nodes)
				{
					if (createdNode.Equals(((IBudgetModel)budgetNode.Tag).ContainedEntity))
					{
						treeViewAdvBudgetGroups.SelectedNode = budgetNode;
						budgetNode.Expand();
					}
				}
			}
		}

		private void addBudgetGroups(BudgetGroupRootModel budgetGroupRootModel)
		{
			var rootNode = new TreeNodeAdv(budgetGroupRootModel.DisplayName)
			{
				LeftImageIndices = new[] { budgetGroupRootModel.ImageIndex },
				Tag = budgetGroupRootModel
			};

			addBudgetModelNodes(budgetGroupRootModel, rootNode);
			treeViewAdvBudgetGroups.Nodes.Add(rootNode);
		}

		private static void addBudgetModelNodes(BudgetGroupRootModel budgetGroupRootModel, TreeNodeAdv rootNode)
		{
			foreach (var budgetGroup in budgetGroupRootModel.BudgetGroups.OrderBy(g => g.DisplayName))
			{
				var budgetNode = new TreeNodeAdv(budgetGroup.DisplayName)
				{
					Tag = budgetGroup,
					LeftImageIndices = new[] { budgetGroup.ImageIndex },
					CollapseImageIndex = budgetGroup.ImageIndex
				};

				rootNode.Nodes.Add(budgetNode);
				addSkillModelNodes(budgetGroup, budgetNode);
			}
		}

		private static void addSkillModelNodes(BudgetGroupModel budgetGroup, TreeNodeAdv treeNodeAdv)
		{
			foreach (var skillModel in budgetGroup.SkillModels)
			{
				var skillModelNode = new TreeNodeAdv(skillModel.DisplayName)
				{
					Tag = skillModel,
					LeftImageIndices = new[] {skillModel.ImageIndex}
				};
				treeNodeAdv.Nodes.Add(skillModelNode);
			}
		}

		private void treeViewAdvBudgetGroupsAfterSelect(object sender, EventArgs e)
		{
			var node = ((TreeViewAdv)sender).SelectedNode;
			if (node == null) return;

			var selectedModel = (IBudgetModel)node.Tag;
			prepareContext(selectedModel);
			SelectedModel = selectedModel;
		}

		#endregion

		private void prepareContext(IBudgetModel selectedModel)
		{
			toolStripMenuItemPropertiesFromSkill.Visible = false;
			treeViewAdvBudgetGroups.ContextMenuStrip = contextMenuStripManagePlanningGroupFromSkill;
			toolStripBudgetGroups.Visible = false;
			toolStripRoot.Visible = false;

			if (selectedModel is BudgetGroupRootModel)
			{
				toolStripRoot.Visible = true;
				treeViewAdvBudgetGroups.ContextMenuStrip = contextMenuStripBudgetGroup;
			}
			else if (selectedModel is BudgetGroupModel)
			{
				toolStripBudgetGroups.Visible = true;
				treeViewAdvBudgetGroups.ContextMenuStrip = contextMenuBudgetGroup;
			}
		}

		private void toolStripButtonDeleteClick(object sender, EventArgs e)
		{
			string questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.AreYouSureYouWantToDelete);
			if (ViewBase.ShowYesNoMessage(questionString, UserTexts.Resources.Delete) != DialogResult.Yes) return;
			
			var attemptSucceeded = _dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(Presenter.DeleteBudgetGroup);
			_globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Publish(Presenter.LoadBudgetGroup());
			if (!attemptSucceeded) return;

			var node = treeViewAdvBudgetGroups.SelectedNode.Parent;
			treeViewAdvBudgetGroups.SelectedNode.Remove();
			if (node.NextSelectableNode != null)
				if (node.NextSelectableNode.Tag != null)
					selectNode((IBudgetGroup) ((IBudgetModel)node.NextSelectableNode.Tag).ContainedEntity);
		}

		private void toolStripButtonOpenBudgetGroupClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
			using (var openBudget = new OpenScenarioForPeriod(new OpenPeriodBudgetsMode()))
			{
				if (openBudget.ShowDialog() != DialogResult.OK) return;
				var budgetMainForm = _budgetGroupMainViewFactory.Create(Presenter.LoadBudgetGroup(),
																		openBudget.SelectedPeriod,
																		openBudget.Scenario);
				((Control)budgetMainForm).Show();
			}
			});
		}

		private void toolStripButtonBudgetGroupPropertiesClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() =>
			{
			using (var page = new BudgetGroupPropertiesPage((IBudgetGroup)Presenter.GetSelectedEntity(),_repositoryFactory,_unitOfWorkFactory))
			{
				page.Initialize(PropertyPagesHelper.BudgetGroupPages(), new LazyLoadingManagerWrapper());
				using (var propertiesPages = new PropertiesPages(page))
				{
					propertiesPages.ShowDialog(this);
				}
				
				updateTree(page.AggregateRootObject);
				_globalEventAggregator.GetEvent<BudgetGroupNeedsRefresh>().Publish(page.AggregateRootObject);
				
			}});
		}

		private void toolStripButtonNewBudgetGroupClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(showWizard);
		}

		private void toolStripMenuItemNewBudgetGroupClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(showWizard);
		}

		private void budgetGroupGroupNavigatorViewLoad(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(() => Presenter.Initialize());
		}
	}
}
