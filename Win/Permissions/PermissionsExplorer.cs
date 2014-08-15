using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Configuration;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers;
using Teleopti.Ccc.Win.Properties;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Permissions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using DataSourceException = Teleopti.Ccc.Infrastructure.Foundation.DataSourceException;

namespace Teleopti.Ccc.Win.Permissions
{
	public partial class PermissionsExplorer : BaseRibbonForm
	{
		private readonly IComponentContext _container;
		public event EventHandler Saved;

		private IList<IAvailableData> _availableDataCollection;
		private readonly IList<IAvailableData> _availableDataForSelectedRoles = new List<IAvailableData>();
		private IList<TreeNodeAdv> _rangeOptionCheckBoxesCollection;
		private readonly IList<IApplicationFunction> _allFunctionsCollection = new List<IApplicationFunction>();   //This list holds all functions which are found in DB
		private readonly IList<IApplicationFunction> _intersectedFunctions = new List<IApplicationFunction>();     //This list holds all the functions which are common to the selected roles
		private readonly IList<IApplicationFunction> _excludedFunctions = new List<IApplicationFunction>();        //This list holds the functions which are NOT assigned to any of the selected roles
		private readonly Dictionary<int, IList<IApplicationFunction>> _functionsDic = new Dictionary<int, IList<IApplicationFunction>>();
		private const int firstElement = 0;
		private ClipboardControl _clipboardControl;
		private static IList<IApplicationFunction> _licensedFunctions;
		private Control _lastInFocus;
		private const string space = @" ";
		private readonly IGracefulDataSourceExceptionHandler _dataSourceExceptionHandler = new GracefulDataSourceExceptionHandler();
		private IPermissionViewerRolesPresenter _permissionsViewerPresenter;
		private readonly IToggleManager _toggleManager;

		public PermissionsExplorer(IComponentContext container)
		{
			_container = container;
			_toggleManager = _container.Resolve<IToggleManager>();
			
			InitializeComponent();
			instantiateClipboardControl();
			setupLastFocus();

			if (!DesignMode)
			{
				SetTexts();
				setRightToLeftLayout();
				ColorHelper.SetRibbonQuickAccessTexts(ExplorerRibbon);
			}

			PermissionsExplorerStateHolder = new PermissionsExplorerStateHolder();
			if (StateHolderReader.Instance.StateReader.SessionScopeData.MickeMode)
				Icon = Resources.rights;

		}

		private PermissionsExplorerStateHolder PermissionsExplorerStateHolder { get; set; }

		private void showRolesHeader()
		{
			int count = listViewRoles.Items.Count;

			if (count == 0)
			{
				// No Roles available.
				RolesBarItem.Text = UserTexts.Resources.NoRolesAvailable;
			}
			else if (count == 1)
			{
				// 1 Role available.
				RolesBarItem.Text = UserTexts.Resources.OneRoleAvailable;
			}
			else if (count > 1)
			{
				// n Roles available.
				RolesBarItem.Text = count.ToString(CultureInfo.CurrentCulture) + space +
									UserTexts.Resources.RolesAvailable;
			}
			else
				throw new NotSupportedException();
		}

		private void setToolStripsToPreferredSize()
		{
			toolStripExRoles.Size = toolStripExRoles.PreferredSize;
			toolStripExPersons.Size = toolStripExPersons.PreferredSize;
			toolStripExClipboard.Size = toolStripExClipboard.PreferredSize;
		}

		private void setPermissionOnControls()
		{
			backStageButton3.Enabled = PrincipalAuthorization.Instance().IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage);
		}

		private void showPeopleHeader()
		{
			int count = listViewPeople.Items.Count;
			if (count == 0)
			{
				// No People available.
				PeopleBarItem.Text = UserTexts.Resources.NoPeopleAvailable;
			}
			else if (count == 1)
			{
				// 1 Person available.
				PeopleBarItem.Text = UserTexts.Resources.OnePersonAvailable;
			}
			else if (count > 1)
			{
				// n People available.
				PeopleBarItem.Text = count.ToString(CultureInfo.CurrentCulture) + space +
									 UserTexts.Resources.PeopleAvailable;
			}
			else
				throw new NotSupportedException();
		}

		private void showFunctionsHeader()
		{
			int count = treeViewFunctions.GetNodeCount(true);
			if (count == 0)
			{
				// No Functions available.
				FunctionsBarItem.Text = UserTexts.Resources.NoFunctionsAvailable;
			}
			else if (count == 1)
			{
				// 1 Function available.
				FunctionsBarItem.Text = UserTexts.Resources.OneFunctionAvailable;
			}
			else if (count > 1)
			{
				// n Functions available.
				FunctionsBarItem.Text = count.ToString(CultureInfo.CurrentCulture) + space +
										UserTexts.Resources.FunctionsAvailable;
			}
			else
				throw new NotSupportedException();
		}

		private void showDataHeader()
		{
			int count = treeViewData.GetNodeCount(true);
			if (count == 0)
			{
				// No Data available.
				DataBarItem.Text = UserTexts.Resources.NoDataAvailable;
			}
			else if (count > 0)
			{
				// n Data available.
				DataBarItem.Text = count.ToString(CultureInfo.CurrentCulture) + space + UserTexts.Resources.DataAvailable;
			}
			else
				throw new NotSupportedException();
		}

		private static bool canBoldItem(ListViewItem item, int totalHits)
		{
			if (item.Tag == null)
			{
				item.Tag = 0;
			}

			var hits = (int)item.Tag;
			if (hits > totalHits)
			{
				hits = totalHits;
			}

			return (totalHits > 1 && totalHits == hits);
		}

		private void bindThisList(IEnumerable<IApplicationRole> roles)
		{
			if (roles == null) return;
			listViewRoles.BeginUpdate();

			foreach (IApplicationRole role in roles)
			{
				ListViewItem newRole = new ExtentListItem
										{
											Text = role.DescriptionText,
											TagObject = role
										};
				if (role.BuiltIn)
					newRole.ForeColor = ColorHelper.ChangeInfoTextColor();
				listViewRoles.Items.Add(newRole);
			}

			listViewRoles.EndUpdate();
		}

		private static void removeFromSelectedRole(IApplicationRole applicationRole,
												   IApplicationFunction applicationFunction)
		{
			if (applicationRole.ApplicationFunctionCollection.Contains(applicationFunction))
				applicationRole.RemoveApplicationFunction(applicationFunction);

			foreach (IApplicationFunction cf in applicationFunction.ChildCollection)
			{
				removeFromSelectedRole(applicationRole, cf);
			}
		}

		private void insertFunctionIntoSelectedRoles(IApplicationFunction function)
		{
			// Add this function into selected roles.
			foreach (ExtentListItem rItem in listViewRoles.SelectedItems)
			{
				IApplicationRole applicationRole = rItem.TagObject as ApplicationRole;
				// Add all function and it's parents (son, father, grandfather, greate grandfather, ...).
				insertIntoSelectedRole(applicationRole, function);

				// Queue changes on the repository.
				PermissionsExplorerStateHolder.AddOrUpdateApplicationRole(applicationRole);
			}
		}

		private static void insertIntoSelectedRole(IApplicationRole role, IApplicationFunction function)
		{
			// Nothing to add. End of recursion.
			if (function == null) return;

			// Add function into role.
			if (!role.ApplicationFunctionCollection.Contains(function))
				role.AddApplicationFunction(function);

			// Add parent(s).
			insertIntoSelectedRole(role, function.Parent as ApplicationFunction);
		}

		private void removeFunctionFromSelectedRoles(IApplicationFunction function)
		{
			// Add this function into selected roles.
			foreach (ExtentListItem rItem in listViewRoles.SelectedItems)
			{
				IApplicationRole applicationRole = rItem.TagObject as ApplicationRole;
				removeFromSelectedRole(applicationRole, function);
				// Queue changes on repository.
				PermissionsExplorerStateHolder.AddOrUpdateApplicationRole(applicationRole);
			}
		}

		private void recursivelyAddChildNodes(TreeNodeAdv treeNode, IList<IApplicationFunction> functions)
		{
			if (functions == null) return;
			var parentApplicationFunction = treeNode.TagObject as IApplicationFunction;
			if (parentApplicationFunction == null) return;

			foreach (
				IApplicationFunction applicationFunction in
					functions.Where(f => parentApplicationFunction.Equals(f.Parent)))
			{
				if (shouldHideFunction(applicationFunction))
					continue;
				if (applicationFunction.IsPreliminary)
					continue;

				var rootNode = new TreeNodeAdv(applicationFunction.LocalizedFunctionDescription)
								{
									TagObject = applicationFunction,
									Tag = 1,
									CheckState = CheckState.Unchecked
								};

				disableFunctionsNotLicensed(applicationFunction, rootNode);

				
				treeNode.Nodes.Add(rootNode);
				recursivelyAddChildNodes(rootNode, functions);
			}
		}

		private void loadAllApplicationFunctions(IList<IApplicationFunction> functions)
		{
			if (functions == null) return;
			foreach (IApplicationFunction function in functions)
			{
				// Add functions guids to the allFunctionsCollection also 
				_allFunctionsCollection.Add(function);
			}

			// Get the root functions from the list
			IEnumerable<IApplicationFunction> rootApplicationFunctions = functions.Where(af => af.Parent == null);

			treeViewFunctions.BeginUpdate();

			foreach (IApplicationFunction applicationFunction in rootApplicationFunctions)
			{
				var rootNode = new TreeNodeAdv(applicationFunction.LocalizedFunctionDescription)
								{
									TagObject = applicationFunction,
									Tag = 1,
									CheckState = CheckState.Unchecked
								};

				disableFunctionsNotLicensed(applicationFunction, rootNode);
				disableFunctionsNotLicensed(applicationFunction, rootNode);

				recursivelyAddChildNodes(rootNode, functions);
				treeViewFunctions.Nodes.Add(rootNode);
			}

			treeViewFunctions.EndUpdate();
		}

		private static void disableFunctionsNotLicensed(IApplicationFunction applicationFunction, TreeNodeAdv rootNode)
		{
			if (applicationFunction.FunctionPath == "All") return;

			if (_licensedFunctions == null)
			{
				_licensedFunctions = (from o in LicenseSchema.GetActiveLicenseSchema(UnitOfWorkFactory.Current.Name).LicenseOptions
									  from f in o.EnabledApplicationFunctions
									  where (o.Enabled)
									  select f).ToList();
			}
			if (applicationFunction.ForeignSource == "Raptor" &&
				!_licensedFunctions.Contains(applicationFunction))
			{
				rootNode.Enabled = false;
			}
		}

		private  bool shouldHideFunction(IApplicationFunction applicationFunction)
		{
			if (applicationFunction.FunctionPath == "All") return false;
			if (applicationFunction.ForeignId == "0095" && !_toggleManager.IsEnabled(Toggles.MyReport_AgentQueueMetrics_22254))
			{
				return true;
			}
			return false;
		}

		private void changeCheckStateOfFunctionTree()
		{
			// Need to expand in order to extract nodes.
			treeViewFunctions.ExpandAll();

			int count = treeViewFunctions.GetNodeCount(true);

			//remove the event handler
			treeViewFunctions.BeforeCheck -= treeViewFunctionsBeforeCheck;
			treeViewFunctions.AfterCheck -= treeViewFunctionsAfterCheck;

			treeViewFunctions.BeginUpdate();
			for (int index = 1; index <= count; index++)
			{
				TreeNodeAdv currentNode = treeViewFunctions.RowIndexToNode(index);

				if (currentNode != null)
				{
					var currentFunction = currentNode.TagObject as IApplicationFunction;

					if (currentFunction != null)
					{
						currentNode.CheckState = getCheckState(currentFunction);
					}
				}
			}
			treeViewFunctions.EndUpdate();

			treeViewFunctions.BeforeCheck += treeViewFunctionsBeforeCheck;
			treeViewFunctions.AfterCheck += treeViewFunctionsAfterCheck;
		}

		private CheckState getCheckState(IApplicationFunction appFunction)
		{
			if (_intersectedFunctions.Contains(appFunction))
				return CheckState.Checked;

			if (_excludedFunctions.Contains(appFunction))
				return CheckState.Unchecked;

			return CheckState.Indeterminate;
		}

		private void handleRoleSelection(int key, IEnumerable<IApplicationFunction> applicationFunctionCollection)
		{
			if (_functionsDic.ContainsKey(key) || applicationFunctionCollection == null) return;
			IList<IApplicationFunction> applicationFunctions = new List<IApplicationFunction>();
			foreach (IApplicationFunction applicationFunction in applicationFunctionCollection)
				applicationFunctions.Add(applicationFunction);

			_functionsDic.Add(key, applicationFunctions);
			performSetOperations();
		}

		private void handleRoleUnSelection(int key, IList<IApplicationFunction> applicationFunctionCollection)
		{
			if (!_functionsDic.ContainsKey(key) || applicationFunctionCollection == null) return;
			_functionsDic.Remove(key);
			performSetOperations();
		}

		private void performSetOperations()
		{
			var keys = new int[_functionsDic.Count];
			_functionsDic.Keys.CopyTo(keys, 0);

			if (_functionsDic.Count > 0)
			{
				IList<IApplicationFunction> tempApplicationFunctionList = new List<IApplicationFunction>();

				//Copy the firt dictionary function list to the intersectedFunctions list 
				_intersectedFunctions.Clear();
				foreach (IApplicationFunction applicationFunction in _functionsDic[keys[firstElement]])
					_intersectedFunctions.Add(applicationFunction);

				for (int count = 0; count < _functionsDic.Count; count++)
				{
					tempApplicationFunctionList.Clear();
					foreach (IApplicationFunction applicationFunction in _intersectedFunctions)
						tempApplicationFunctionList.Add(applicationFunction);

					// Get the intersection of the all remaining collections
					IEnumerable<IApplicationFunction> enumerable = tempApplicationFunctionList.Intersect(_functionsDic[keys[count]]);
					_intersectedFunctions.Clear();
					foreach (IApplicationFunction applicationFunction in enumerable)
						_intersectedFunctions.Add(applicationFunction);
				}

				_excludedFunctions.Clear();
				foreach (IApplicationFunction applicationFunction in _allFunctionsCollection)
					_excludedFunctions.Add(applicationFunction);

				for (int count = 0; count < _functionsDic.Count; count++)
				{
					tempApplicationFunctionList.Clear();
					foreach (IApplicationFunction applicationFunction in _excludedFunctions)
						tempApplicationFunctionList.Add(applicationFunction);

					// Remove the selected functions set from the full set of functions
					IEnumerable<IApplicationFunction> enumerable = tempApplicationFunctionList.Except(_functionsDic[keys[count]]);
					_excludedFunctions.Clear();
					foreach (IApplicationFunction applicationFunction in enumerable)
						_excludedFunctions.Add(applicationFunction);
				}
			}
			else
			{
				_intersectedFunctions.Clear();
				_excludedFunctions.Clear();
				foreach (IApplicationFunction af in _allFunctionsCollection)
					_excludedFunctions.Add(af);
			}
		}

		private void initializeDataTreeRangeOptions()
		{
			_rangeOptionCheckBoxesCollection = new List<TreeNodeAdv>();

			foreach (AvailableDataRangeOption t in Enum.GetValues(typeof(AvailableDataRangeOption)))
			{
				if (t == AvailableDataRangeOption.None) continue; // skip the None check box

				var node = new TreeNodeAdv
							{
								ShowCheckBox = true,
								Text = Enum.GetName(typeof(AvailableDataRangeOption), t),
								TagObject = t,
								Tag = t
							};
				treeViewData.Nodes.Add(node);

				_rangeOptionCheckBoxesCollection.Add(node);
			}
		}

		private void buildDataTree()
		{
			IBusinessUnit bu = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;

			var buNode = new TreeNodeAdv
									 {
										 Text = bu.Description.Name,
										 TagObject = bu,
										 Tag = 1
									 };

			treeViewData.BeginUpdate();
			treeViewData.Nodes.Add(buNode);

			foreach (ISite site in bu.SiteCollection)
			{
				if (((IDeleteTag)site).IsDeleted) continue;
				var siteNode = new TreeNodeAdv
										   {
											   Text = site.Description.Name,
											   TagObject = site,
											   Tag = 2
										   };
				buNode.Nodes.Add(siteNode);

				foreach (ITeam team in site.TeamCollection)
				{
					if (((IDeleteTag)team).IsDeleted) continue;
					var teamNode = new TreeNodeAdv
											   {
												   Text = team.Description.Name,
												   TagObject = team,
												   Tag = 3
											   };
					siteNode.Nodes.Add(teamNode);
				}
			}
			treeViewData.EndUpdate();
		}

		private void validateFunctionsTreeParentNodesState(TreeViewAdv tree)
		{
			if (_functionsDic.Count >= 1)
			{
				treeViewFunctions.BeforeCheck -= treeViewFunctionsBeforeCheck;
				treeViewFunctions.AfterCheck -= treeViewFunctionsAfterCheck;

				// Need to expand in order to extract nodes.
				tree.ExpandAll();

				int count = tree.GetNodeCount(true);

				for (int index = count; index > 0; index--)
				{
					TreeNodeAdv currentNode = treeViewFunctions.RowIndexToNode(index);

					if (currentNode != null)
					{
						if (currentNode.Nodes.Count > 0)
							currentNode.CheckState = getStateSpecial(currentNode);
					}
				}

				treeViewFunctions.BeforeCheck += treeViewFunctionsBeforeCheck;
				treeViewFunctions.AfterCheck += treeViewFunctionsAfterCheck;
			}
		}

		private static CheckState getState(TreeNodeAdvCollection childrenNodes)
		{
			int noOfCheckedNodes = 0;
			int noOfUnCheckedNodes = 0;

			foreach (TreeNodeAdv node in childrenNodes)
			{
				if (node.CheckState == CheckState.Checked)
				{
					noOfCheckedNodes++;
				}
				else if (node.CheckState == CheckState.Unchecked)
				{
					noOfUnCheckedNodes++;
				}
			}

			if (noOfCheckedNodes == childrenNodes.Count)
				return CheckState.Checked;

			if (noOfUnCheckedNodes == childrenNodes.Count)
				return CheckState.Unchecked;

			return CheckState.Indeterminate;
		}

		private static CheckState getStateSpecial(TreeNodeAdv currentNode)
		{
			TreeNodeAdvCollection childrenNodes = currentNode.Nodes;
			CheckState returnCheckState;

			int noOfCheckedNodes = 0;
			int noOfUnCheckedNodes = 0;

			foreach (TreeNodeAdv node in childrenNodes)
			{
				if (node.CheckState == CheckState.Checked)
				{
					noOfCheckedNodes++;
				}
				else if (node.CheckState == CheckState.Unchecked)
				{
					noOfUnCheckedNodes++;
				}
			}

			if (noOfCheckedNodes == childrenNodes.Count)
			{
				returnCheckState = CheckState.Checked;
			}
			else if (noOfUnCheckedNodes == childrenNodes.Count)
			{
				returnCheckState = currentNode.CheckState == CheckState.Indeterminate ? CheckState.Unchecked : currentNode.CheckState;
			}
			else
			{
				returnCheckState = CheckState.Indeterminate;
			}

			return returnCheckState;
		}

		private static void changeStatusOfParentNode(TreeNodeAdv parentNode)
		{
			TreeNodeAdv currentNode = parentNode;

			if (currentNode == null)
			{
				return;
			}

			currentNode.CheckState = getStateSpecial(parentNode);
			changeStatusOfParentNode(currentNode.Parent);
		}

		private static void changeStatusOfAllChildrenNodes(TreeNodeAdv currentNode)
		{
			if (currentNode != null &&
				(currentNode.CheckState == CheckState.Checked || currentNode.CheckState == CheckState.Unchecked))
			{
				if (currentNode.HasChildren)
				{
					TreeNodeAdvCollection childrenNodes = currentNode.Nodes;

					foreach (TreeNodeAdv node in childrenNodes)
					{
						node.CheckState = currentNode.CheckState;
						changeStatusOfAllChildrenNodes(node);
					}
				}
			}
		}

		private void handleTreeViewFunctionsCheck(TreeNodeAdv currentNode, CheckState checkState)
		{
			if (currentNode != null)
			{
				treeViewFunctions.BeforeCheck -= treeViewFunctionsBeforeCheck;
				treeViewFunctions.AfterCheck -= treeViewFunctionsAfterCheck;

				if (checkState == CheckState.Unchecked)
				{
					changeStatusOfAllChildrenNodes(currentNode);
				}

				changeStatusOfParentNode(currentNode.Parent);

				treeViewFunctions.BeforeCheck += treeViewFunctionsBeforeCheck;
				treeViewFunctions.AfterCheck += treeViewFunctionsAfterCheck;
			}
		}

		private static IList<IApplicationFunction> getAllFunctionsToBeAddedOrRemoved(TreeNodeAdv currentNode)
		{
			IList<IApplicationFunction> functions = new List<IApplicationFunction>();

			if (currentNode != null)
			{
				functions.Add(currentNode.TagObject as IApplicationFunction);
				getAllChildrenNodesTagObjects(currentNode, ref functions);
			}

			return functions;
		}

		private static void getAllChildrenNodesTagObjects(TreeNodeAdv currentNode,
														  ref IList<IApplicationFunction> functions)
		{
			if (currentNode != null)
			{
				if (currentNode.HasChildren)
				{
					TreeNodeAdvCollection childrenNodes = currentNode.Nodes;

					foreach (TreeNodeAdv node in childrenNodes)
					{
						functions.Add(node.TagObject as IApplicationFunction);
						getAllChildrenNodesTagObjects(node, ref functions);
					}
				}
			}
		}

		private void removeFunctionsFromSelectedRoles(IList<IApplicationFunction> functions)
		{
			foreach (ExtentListItem rItem in listViewRoles.SelectedItems)
			{
				var applicationRole = rItem.TagObject as IApplicationRole;

				// Remove all the functions into selected roles.
				foreach (IApplicationFunction applicationFunction in functions)
					removeFromSelectedRole(applicationRole, applicationFunction);

				// Queue changes on the repository.
				PermissionsExplorerStateHolder.AddOrUpdateApplicationRole(applicationRole);
			}
		}

		private void setDataTreeParentNodesCheckState()
		{
			// Need to expand in order to extract nodes.
			treeViewData.ExpandAll();

			int count = treeViewData.GetNodeCount(true);

			for (int index = count; index > 0; index--)
			{
				TreeNodeAdv currentNode = treeViewData.RowIndexToNode(index);

				// Need to avoid AvailableDataRangeOption nodes
				if (currentNode != null && !typeof(AvailableDataRangeOption).IsInstanceOfType(currentNode.TagObject))
				{
					if (currentNode.Nodes.Count > 0)
					{
						currentNode.CheckState = getState(currentNode.Nodes);
					}
				}
			}
		}

		private void setDataTreeCheckState(ICollection<IAvailableData> dataCollection)
		{
			TreeNodeAdvCollection treeNodeAdvCollection = treeViewData.Root.Nodes;
			if (treeNodeAdvCollection != null)
			{
				foreach (TreeNodeAdv treeNodeAdv in treeNodeAdvCollection)
				{
					uncheckAllChildrenNodes(treeNodeAdv);
				}
			}

			if (dataCollection.Count == 0) //no available data for application roles selected
			{
				return;
			}

			// create collections to store different data types from selected roles
			IList<IEnumerable<ISite>> siteAll;
			IList<IEnumerable<ITeam>> teamAll;
			IList<IEnumerable<AvailableDataRangeOption>> dataRangeOptionAll;
			IList<IEnumerable<IBusinessUnit>> buAll = loadAvailableData(dataCollection, out siteAll, out teamAll, out dataRangeOptionAll);

			// Set the check state of differnt node types
			getDifferenceAndIntersection(dataCollection.Count, buAll);
			getDifferenceAndIntersection(dataCollection.Count, siteAll);
			getDifferenceAndIntersection(dataCollection.Count, teamAll);
			getDifferenceAndIntersection(dataCollection.Count, dataRangeOptionAll);
		}

		private void getDifferenceAndIntersection<T>(int rolesSelected, IList<IEnumerable<T>> sets)
		{
			IList<T> union = null;
			IList<T> intersection = null;
			IList<T> difference = null;

			if (rolesSelected > 1 && sets.Count == rolesSelected) // may be have an intersection of multiple roles
			{
				union = sets[firstElement].ToList();
				intersection = sets[firstElement].ToList();

				for (int t = 1; t < sets.Count; t++)
				{
					union = union.Union(sets[t]).ToList();
					intersection = intersection.Intersect(sets[t]).ToList();
				}
			}
			else if (sets.Count > 0) // union only
			{
				union = sets[firstElement].ToList();

				for (int t = 1; t < sets.Count; t++)
				{
					union = union.Union(sets[t]).ToList();
				}
			}

			if (union != null && intersection != null) //looks for a difference
			{
				difference = union.Except(intersection).ToList();
			}

			if (difference != null) //if there is a difference then there is an intersection
			{
				setDataTreeItemsCheckState(intersection, CheckState.Checked);
				setDataTreeItemsCheckState(difference, CheckState.Indeterminate);
			}
			else if (rolesSelected > 1 && union != null) // no intersection and multiple roles are selected
			{
				setDataTreeItemsCheckState(union, CheckState.Indeterminate);
			}
			else if (union != null) // one role is selected
			{
				setDataTreeItemsCheckState(union, CheckState.Checked);
			}
		}

		private void setDataTreeItemsCheckState<T>(IEnumerable<T> list, CheckState chkState)
		{
			foreach (T t in list)
			{
				var nodes = treeViewData.FindNodesWithTagObject(t);
				foreach (TreeNodeAdv node in nodes)
				{
					node.CheckState = chkState;
				}
			}
		}

		private void removeUnCheckedItemFromSelectedRoles(Object item, TreeNodeAdv nodeItem)
		{
			var buItem = item as IBusinessUnit;
			var siteItem = item as ISite;
			var teamItem = item as ITeam;

			foreach (IAvailableData availableData in _availableDataForSelectedRoles)
			{
				if (buItem != null)
				{
					availableData.DeleteAvailableBusinessUnit(buItem);

					// TODO Method to remove a hierarchical collection in AvailableData class
					IList<ISite> sites = buItem.SiteCollection;
					foreach (ISite site in sites) // data tree and the repository has a 1:1 map
					{
						if (((Site)site).IsDeleted)
							continue;

						availableData.DeleteAvailableSite(site);

						IList<ITeam> teams = site.TeamCollection;
						foreach (ITeam team in teams)
						{
							availableData.DeleteAvailableTeam(team);
						}
					}

					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);
				}
				else if (siteItem != null)
				{
					availableData.DeleteAvailableSite(siteItem);
					availableData.DeleteAvailableBusinessUnit(siteItem.BusinessUnit);

					IList<ITeam> teams = siteItem.TeamCollection;

					foreach (ITeam team in teams)
					{
						availableData.DeleteAvailableTeam(team);
					}

					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);

					if (removeParentNode(nodeItem))
					{
						removeUnCheckedItemFromSelectedRoles(siteItem.BusinessUnit, nodeItem.Parent);
					}
				}
				else if (teamItem != null)
				{

					availableData.DeleteAvailableTeam(teamItem);
					availableData.DeleteAvailableSite(teamItem.Site);
					availableData.DeleteAvailableBusinessUnit(teamItem.Site.BusinessUnit);
					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);

					if (removeParentNode(nodeItem))
					{
						removeUnCheckedItemFromSelectedRoles(teamItem.Site, nodeItem.Parent);
					}
				}
			}
		}

		private void insertCheckedItemForSelectedRoles(Object item, TreeNodeAdv nodeItem)
		{
			var buItem = item as IBusinessUnit;
			var siteItem = item as ISite;
			var teamItem = item as ITeam;

			foreach (IAvailableData availableData in _availableDataForSelectedRoles)
			{
				if (buItem != null)
				{
					availableData.AddAvailableBusinessUnit(buItem);

					// TODO Method to add a hierarchical collection in AvailableData class
					foreach (ISite site in buItem.SiteCollection)
					{
						if (!((IDeleteTag)site).IsDeleted)
						{
							availableData.AddAvailableSite(site);
							foreach (ITeam team in site.TeamCollection)
							{
								if (!((IDeleteTag)team).IsDeleted)
									availableData.AddAvailableTeam(team);
							}
						}
					}

					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);
				}
				else if (siteItem != null)
				{
					availableData.AddAvailableSite(siteItem);

					foreach (ITeam team in siteItem.TeamCollection)
					{
						if (!((IDeleteTag)team).IsDeleted)
							availableData.AddAvailableTeam(team);
					}

					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);

					if (insertParentNode(nodeItem))
					{
						insertCheckedItemForSelectedRoles(siteItem.BusinessUnit, nodeItem.Parent);
					}
				}
				else if (teamItem != null)
				{
					availableData.AddAvailableTeam(teamItem);
					PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);

					if (insertParentNode(nodeItem))
					{
						insertCheckedItemForSelectedRoles(teamItem.Site, nodeItem.Parent);
					}
				}

				PermissionsExplorerStateHolder.AssignAvailableDataInPermissionsDataDictionary(availableData);

			}
		}

		private void changeCheckStatusOfRangeOptionChkBoxes(Object item)
		{
			treeViewData.BeforeCheck -= treeViewDataBeforeCheck;
			treeViewData.AfterCheck -= treeViewDataAfterCheck;

			var query =
				from
			node in
					_rangeOptionCheckBoxesCollection
				where (node.CheckState == CheckState.Checked || node.CheckState == CheckState.Indeterminate) &&
				(node.TagObject.Equals(item) == false)
				select node;

			foreach (var treeNode in query)
			{
				treeNode.CheckState = CheckState.Unchecked;
			}

			treeViewData.BeforeCheck += treeViewDataBeforeCheck;
			treeViewData.AfterCheck += treeViewDataAfterCheck;
		}

		private void changeRangeOptionInRoles(AvailableDataRangeOption rangeOption)
		{
			foreach (IAvailableData availableData in _availableDataForSelectedRoles)
			{
				availableData.AvailableDataRange = rangeOption;
				PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);
			}
		}

		private static bool insertParentNode(TreeNodeAdv currentNode)
		{
			//TreeNodeAdv currentNode = treeViewData.FindNodeWithTagObject(item);
			if (currentNode != null && currentNode.Parent != null && currentNode.Parent.TagObject != null)
			{
				CheckState parentChkState = getState(currentNode.Parent.Nodes);
				return (parentChkState == CheckState.Checked);
			}

			return false;
		}

		private static bool removeParentNode(TreeNodeAdv currentNode)
		{
			//TreeNodeAdv currentNode = treeViewData.FindNodeWithTagObject(item);
			if (currentNode != null && currentNode.Parent != null && currentNode.Parent.TagObject != null)
			{
				CheckState parentChkState = getState(currentNode.Parent.Nodes);
				return (parentChkState == CheckState.Unchecked);
			}

			return false;
		}

		private static bool needToRemoveParentNode(TreeNodeAdv currentNode)
		{
			if (currentNode != null &&
				currentNode.Parent != null &&
				isAllOtherChildNodesUnchecked(currentNode.Parent.Nodes, currentNode))
			{
				return true;
			}

			return false;
		}

		private static bool isAllOtherChildNodesUnchecked(TreeNodeAdvCollection childrenNodes, TreeNodeAdv currentNode)
		{
			int noOfUnCheckedNodes = 0;
			Guid? guid = null;

			if (currentNode != null)
			{
				guid = ((IApplicationFunction)currentNode.TagObject).Id;
			}

			foreach (TreeNodeAdv node in childrenNodes)
			{
				if (node != null)
				{
					var function = node.TagObject as IApplicationFunction;

					if (function != null && function.Id != guid)
					{
						if (node.CheckState == CheckState.Unchecked)
						{
							noOfUnCheckedNodes++;
						}
					}
				}
			}

			if (noOfUnCheckedNodes == childrenNodes.Count - 1)
			{
				return true;
			}

			return false;
		}

		private void setRightToLeftLayout()
		{
			if (StateHolderReader.IsInitialized)
			{
				bool rightToLeft = (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.RightToLeftDisplay);
				listViewRoles.RightToLeftLayout = rightToLeft;
				listViewPeople.RightToLeftLayout = rightToLeft;
			}
		}

		private bool isRoleSelected()
		{
			return listViewRoles.SelectedItems.Count > 0;
		}

		private void bindPeople(ICollection<IPersonInRole> people)
		{
			if (people != null)
			{
				int count = listViewPeople.Items.Count;
				int totalHits = listViewRoles.SelectedItems.Count;
				IList<IPersonInRole> personList = new List<IPersonInRole>();

				for (int index = 0; index < count; index++)
				{
					var currentItem = listViewPeople.Items[index] as ExtentListItem;

					if (currentItem != null)
					{
						var currentPerson = currentItem.TagObject as IPersonInRole;

						if (currentPerson != null)
						{
							personList.Add(currentPerson);

							// Are there any people to find?
							if (people.Count > 0)
							{
								// Remove this person (if exists).
								if (people.Contains(currentPerson))
								{
									// Initialize the tag of the person (usually this is not supposed to happen).
									if (currentItem.Tag == null)
										currentItem.Tag = 1;

									var hits = (int)currentItem.Tag;
									// Increase the hit count of the person.
									hits++;
									currentItem.Tag = hits;
								}
							}

							// Check whether person can be bold.
							if (canBoldItem(currentItem, totalHits))
							{
								currentItem.Font = currentItem.Font.ChangeToBold();
							}
							else if (currentItem.Font.Bold)
							{
								currentItem.Font = currentItem.Font.ChangeToRegular();
							}
						}
					}
				}

				IEnumerable<IPersonInRole> enumerable = people.Except(personList);

				listViewPeople.BeginUpdate();
				using (PerformanceOutput.ForOperation("Populating Agents"))
				{
					IList<ListViewItem> listViewItems = new List<ListViewItem>();

				foreach (IPersonInRole person in enumerable)

					{
						// Create a new person and add to listViewPeople.
						ListViewItem newPerson = new ExtentListItem
						{
													 Text = person.FirstName,
							TagObject = person,
							Tag = 1
						}
						;

						var lastName = new ListViewItem.ListViewSubItem
						{
																	Text = person.LastName,
							Tag = person
						}
						;

						var teamName = new ListViewItem.ListViewSubItem
									   {
										   Text = person.Team 
						};
						newPerson.SubItems.AddRange(new[] { lastName, teamName });
						listViewItems.Add(newPerson);
						//listViewPeople.Items.Add(newPerson);
					}
					listViewPeople.Items.AddRange(listViewItems.ToArray());
				}
				listViewPeople.Sort();
				listViewPeople.EndUpdate();

				// Show number of peoples assigned.
				showPeopleHeader();
			}
		}

		private void debindPeople(IApplicationRole role)
		{
			int totalHits = listViewRoles.SelectedItems.Count;
			int count = listViewPeople.Items.Count - 1;

			// Check and remove from Bottom to Top.
			listViewPeople.BeginUpdate();
			for (int index = count; index >= 0; index--)
			{
				var currentItem = listViewPeople.Items[index] as ExtentListItem;

				if (currentItem != null)
				{
					var person = currentItem.TagObject as IPerson;

					if (person != null)
					{
						if (currentItem.Tag == null)
							currentItem.Tag = 1;

						var hits = (int)currentItem.Tag;

						// Person belongs to the role.
						if (person.PermissionInformation.ApplicationRoleCollection.Contains(role))
						{
							hits--;
							currentItem.Tag = hits;
						}

						// Can remove item from the list?
						if (hits < 1)
						{
							currentItem.Remove();
						}
						else if (canBoldItem(currentItem, totalHits))
						{
							// Bold item (if not all roles have this).
							if (!currentItem.Font.Bold)
								currentItem.Font = currentItem.Font.ChangeToBold();
						}
						else
						{
							// Unbold item (if not all roles have this).
							if (currentItem.Font.Bold)
								currentItem.Font = currentItem.Font.ChangeToRegular();
						}
					}
				}
			}

			listViewPeople.Sort();
			listViewPeople.EndUpdate();

			// Show number of peoples assigned.
			showPeopleHeader();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
			"CA2227:CollectionPropertiesShouldBeReadOnly")]
		public ICollection<Guid> SelectedPersonsToAddToRole { get; set; }

		private void clearControls()
		{
			listViewPeople.Items.Clear();
			showPeopleHeader();

			treeViewData.BeforeCheck -= treeViewDataBeforeCheck;
			treeViewData.AfterCheck -= treeViewDataAfterCheck;

			treeViewFunctions.BeforeCheck -= treeViewFunctionsBeforeCheck;
			treeViewFunctions.AfterCheck -= treeViewFunctionsAfterCheck;

			uncheckTreeNodes(treeViewFunctions);
			uncheckTreeNodes(treeViewData);

			// clear the dics also ?? 
			_functionsDic.Clear();
			_availableDataForSelectedRoles.Clear();

			treeViewData.BeforeCheck += treeViewDataBeforeCheck;
			treeViewData.AfterCheck += treeViewDataAfterCheck;

			treeViewFunctions.BeforeCheck += treeViewFunctionsBeforeCheck;
			treeViewFunctions.AfterCheck += treeViewFunctionsAfterCheck;
		}

		private static void uncheckTreeNodes(TreeViewAdv tree)
		{
			TreeNodeAdvCollection treeNodeAdvCollection = tree.Root.Nodes;
			if (treeNodeAdvCollection != null)
			{
				foreach (TreeNodeAdv treeNodeAdv in treeNodeAdvCollection)
				{
					treeNodeAdv.Checked = false;
				}
			}
		}


		private void save()
		{
			try
			{
				PermissionsExplorerStateHolder.QueueDirtyPeopleCollection();
				PermissionsExplorerStateHolder.PersistAll();
				OnSaved();
			}
			catch (OptimisticLockException)
			{
				string caption = UserTexts.Resources.YourChangesWillBeDiscarded;

				ShowErrorMessage(
					string.Concat(UserTexts.Resources.SomeoneElseHaveChanged + ". " +
								  UserTexts.Resources.YourChangesWillBeDiscarded), caption);
			}
			catch (DataSourceException dataSourceException)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(dataSourceException);
				FormKill();
			}
		}

		private void handleAddNewRole()
		{
			//Clear all the UI elements and local memory collections
			clearControls();

			IApplicationRole newRole = new ApplicationRole
										{
											Name = "NewApplicationRole",
											DescriptionText = UserTexts.Resources.NewApplicationRole
										};

			PermissionsExplorerStateHolder.AddOrUpdateApplicationRole(newRole);
			var permissionsDataHolder = new PermissionsDataHolder(true);
			PermissionsExplorerStateHolder.AddRoleToPermissionsDataDictionary(newRole, permissionsDataHolder);

			// manually add BusinessUnit
			newRole.SetBusinessUnit(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit);

			// Create an AvailableData instance for the new ApplicationRole
			IAvailableData availableData = new AvailableData { ApplicationRole = newRole };

			PermissionsExplorerStateHolder.AddOrUpdateAvailableData(availableData);
			PermissionsExplorerStateHolder.AssignAvailableDataInPermissionsDataDictionary(availableData);

			// Add this AvailableData to the runtime collection which holds all AvailableData objects - Sachintha 06/16/08
			_availableDataCollection.Add(availableData);

			var newRoleItem = new ExtentListItem
											 {
												 Text = newRole.DescriptionText,
												 TagObject = newRole
											 }
			;

			listViewRoles.BeginUpdate();
			listViewRoles.Items.Add(newRoleItem);
			listViewRoles.EndUpdate();

			showRolesHeader();

			// Make the new item editable. User must enter a name now.
			newRoleItem.BeginEdit();
		}

		private void setupLastFocus()
		{
			listViewPeople.GotFocus += controlGotFocus;
			listViewRoles.GotFocus += controlGotFocus;
		}

		private void controlGotFocus(object obj, EventArgs e)
		{
			_lastInFocus = (Control)obj;
			toolStripExPersons.Enabled = _lastInFocus == listViewPeople;
			toolStripExRoles.Enabled = _lastInFocus == listViewRoles;
		}

		private void handleRenameRole()
		{
			if (listViewRoles.SelectedItems.Count != 1)
				return;
			var item = listViewRoles.SelectedItems[firstElement] as ExtentListItem;
			if (!isBuiltIn(item))
				listViewRoles.SelectedItems[firstElement].BeginEdit();
		}

		private static bool isBuiltIn(ExtentListItem item)
		{
			if (item == null)
				return false;
			var role = item.TagObject as IApplicationRole;
			if (role != null && role.BuiltIn)
				return true;
			return false;
		}

		private void handleDeleteRole()
		{
			if (isRoleSelected())
			{
				int selectedItemsCount = listViewRoles.SelectedItems.Count;

				DialogResult response;

				if (selectedItemsCount == 1)
				{
					var item = listViewRoles.SelectedItems[0] as ExtentListItem;
					if (item == null)
						return;
					if (isBuiltIn(item))
					{
						ShowInformationMessage(UserTexts.Resources.ThisRoleCannotBeDeleted, UserTexts.Resources.NotRemovable);
						return;
					}
					response = ShowYesNoMessage(
											string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.AreYouSureYouWantToDelete,
											listViewRoles.SelectedItems[firstElement].Text),
											UserTexts.Resources.ConfirmRoleDelete);
				}
				else
				{
					response =
						ShowYesNoMessage(
							string.Format(CultureInfo.CurrentUICulture, UserTexts.Resources.AreYouSureYouWantToDelete,
										  selectedItemsCount),
							UserTexts.Resources.ConfirmRoleDelete);
				}

				if (response == DialogResult.No || response == DialogResult.Cancel)
					return;

				listViewRoles.BeginUpdate();
				foreach (ExtentListItem item in listViewRoles.SelectedItems)
				{
					//1. Delete the application role
					//2. Delete the corresponding available data as well
					//3. remove this role from the ppl attached to it 

					var role = item.TagObject as IApplicationRole;

					if (isBuiltIn(item))
						continue;
					// Gets the associated AvailableData object used for DataTree for the selected role
					IAvailableData availableData = AvailableData.FindByApplicationRole(_availableDataCollection, role);
					// Remove from runtime collection
					_availableDataCollection.Remove(availableData);
					// Queue to remove
					if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> PermissionsExplorerStateHolder.DeleteAvailableData(availableData)))
					{
						FormKill();
						return;
					}

					// Queue on repository.
					if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> PermissionsExplorerStateHolder.DeleteApplicationRole(role)))
					{
						FormKill();
						return;
					}

					removeDeletedRoleFromPeople(role);

					//listViewRoles.Items.Remove(item);
					//Remove from the dic too
					PermissionsExplorerStateHolder.RemoveRoleFromPermissionsDataDictionary(role);

					listViewRoles.Items.Remove(item);
				}
				listViewRoles.EndUpdate();


				showRolesHeader();
			}
		}

		private void handleRemovePeople()
		{
			int count = listViewPeople.SelectedItems.Count;

			if (count >= 1)
			{
				DialogResult response;

				if (count == 1)
				{
					response =
						ShowYesNoMessage(
							string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.AreYouSureYouWantToDelete,
										  listViewPeople.SelectedItems[firstElement].Text), UserTexts.Resources.ConfirmPeopleRemove);
				}
				else
				{
					response =
					   ShowYesNoMessage(
						   string.Format(CultureInfo.InvariantCulture, UserTexts.Resources.AreYouSureYouWantToDelete, count),
						   UserTexts.Resources.ConfirmPeopleRemove);
				}

				if (response == DialogResult.Yes)
				{
					listViewPeople.BeginUpdate();

					for (int index = count - 1; index >= 0; index--)
					{
						// the listitem.
						var pItem = listViewPeople.SelectedItems[index] as ExtentListItem;

						if (pItem != null)
						{
							var person = pItem.TagObject as IPersonInRole;

							if (person != null)
							{
								// Remove all selected roles from this person.
								foreach (ExtentListItem rItem in listViewRoles.SelectedItems)
								{
									var applicationRole = rItem.TagObject as IApplicationRole;
									removeApplicationRoleAndCache(person.Id, applicationRole);
									
									// Remove this person from the cache dictionary also
									PermissionsExplorerStateHolder.RemovePersonInPermissionsDataDictionary(applicationRole, person);
								}

								// Remove node from the tree.
								pItem.Remove();
							}
						}
					}

					int redrawCount = listViewPeople.Items.Count;
					if (redrawCount > 0)
					{
						listViewPeople.RedrawItems(0, redrawCount - 1, false);
					}

					listViewPeople.Sort();
					listViewPeople.EndUpdate();

					// Show number of peoples assigned.
					showPeopleHeader();
				}
			}
		}

		private void removeApplicationRoleAndCache(Guid id, IApplicationRole role)
		{
			IPerson personFromCache = PermissionsExplorerStateHolder.IsPersonInTheList(id);

			if (personFromCache != null)
			{
				if (personFromCache.PermissionInformation.ApplicationRoleCollection.Contains(role))
				{
					personFromCache.PermissionInformation.RemoveApplicationRole(role);
					PermissionsExplorerStateHolder.MarkAsDirty(personFromCache.Id);
				}
			}
			else
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					IPersonRepository personRepository = new PersonRepository(uow);
					personFromCache = personRepository.Load(id);
					PermissionsExplorerStateHolder.AddPersonAdapter(new PersonAdapter(personFromCache, true));
					if (personFromCache.PermissionInformation.ApplicationRoleCollection.Contains(role))
					{
						personFromCache.PermissionInformation.RemoveApplicationRole(role);
						PermissionsExplorerStateHolder.MarkAsDirty(personFromCache.Id);
					}
				} 
			}
		}

		private static void uncheckAllChildrenNodes(TreeNodeAdv currentNode)
		{
			if (currentNode != null)
			{
				if (currentNode.HasChildren)
				{
					TreeNodeAdvCollection childrenNodes = currentNode.Nodes;

					foreach (TreeNodeAdv node in childrenNodes)
					{
						node.CheckState = CheckState.Unchecked;
						changeStatusOfAllChildrenNodes(node);
					}
				}
			}
		}

		private void addApplicationRoleAndCache(Guid id, IApplicationRole role)
		{
			IPerson personFromCache = PermissionsExplorerStateHolder.IsPersonInTheList(id);

			if (personFromCache != null)
			{
				if (!personFromCache.PermissionInformation.ApplicationRoleCollection.Contains(role))
				{
					personFromCache.PermissionInformation.AddApplicationRole(role);
					PermissionsExplorerStateHolder.MarkAsDirty(personFromCache.Id);
				}
			}
			else
			{
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					IPersonRepository personRepository = new PersonRepository(uow);
					personFromCache = personRepository.Load(id);

					if (!personFromCache.PermissionInformation.ApplicationRoleCollection.Contains(role))
					{
						personFromCache.PermissionInformation.AddApplicationRole(role);
					}
				}

				PermissionsExplorerStateHolder.AddPersonAdapter(new PersonAdapter(personFromCache, true));
			}
		}

		private bool copy(ListView view)
		{
			PermissionsExplorerStateHolder.CopyPasteStack.Clear();
			bool canCopy = true;

			int length = view.SelectedItems.Count;
			for (int index = 0; index < length; index++)
			{
				ListViewItem sourceListViewItem = view.SelectedItems[index];
				var extentListItem = (ExtentListItem)sourceListViewItem;
				var sourceRole = (ApplicationRole)extentListItem.TagObject;
				if (sourceRole.BuiltIn)
				{
					if (length == 1)
						canCopy = false;
					continue;
				}

				ApplicationRole clonedRole = sourceRole.NoneEntityClone();
				var clonedExtentListItem = new ExtentListItem { TagObject = clonedRole }; // (ExtentListItem); extentListItem.Clone();

				var applicationRoleAdapter = new ApplicationRoleAdapter
												{
													ApplicationRole = sourceRole,
													ListViewItem = clonedExtentListItem
												};

				PermissionsExplorerStateHolder.CopyPasteStack.Push(applicationRoleAdapter);
				canCopy = true;
			}
			return canCopy;
		}

		private void paste(ListView view)
		{
			if (PermissionsExplorerStateHolder.CanPaste())
			{
				view.BeginUpdate();

				while (PermissionsExplorerStateHolder.CopyPasteStack.Count > 0)
				{
					if (view.SelectedItems.Count != 0)
					{
						view.SelectedItems.Clear();
					}

					ApplicationRoleAdapter applicationRoleAdapter = PermissionsExplorerStateHolder.CopyPasteStack.Pop();
					var extentListItemToPaste = (ExtentListItem)applicationRoleAdapter.ListViewItem;
					var applicationRole = (IApplicationRole)extentListItemToPaste.TagObject;
					applicationRole.DescriptionText = UserTexts.Resources.CopyOf + " " + applicationRole.DescriptionText;
					extentListItemToPaste.Text = applicationRole.DescriptionText;
					// Add to list view.
					view.Items.Add(extentListItemToPaste);

					PermissionsExplorerStateHolder.SaveApplicationRole(applicationRole);

					ICollection<IPersonInRole> sourcePersonCollection = new List<IPersonInRole>();
					foreach (var person in PermissionsExplorerStateHolder.GetPersonInPermissionsDataDictionary(applicationRoleAdapter.ApplicationRole))
					{
						sourcePersonCollection.Add(person);
					}

					// Add roles for each person selected.
					foreach (var person in sourcePersonCollection)
					{
						addApplicationRoleAndCache(person.Id, applicationRole);
					}

					IAvailableData sourceAvailableData = PermissionsExplorerStateHolder.GetAvailableDataInPermissionsDataDictionary(applicationRoleAdapter.ApplicationRole);

					IAvailableData availableDataToSave = new AvailableData
															{
																ApplicationRole = applicationRole,
																AvailableDataRange = sourceAvailableData.AvailableDataRange
															};
					PermissionsExplorerStateHolder.CopySitesAndTeam(sourceAvailableData, availableDataToSave);

					PermissionsExplorerStateHolder.AddAndSaveAvailableData(availableDataToSave);

					//Add the newly duplicated role to the dictionary
					var permissionsDataHolder = new PermissionsDataHolder(sourcePersonCollection, availableDataToSave, true);
					PermissionsExplorerStateHolder.AddRoleToPermissionsDataDictionary(applicationRole, permissionsDataHolder);
				}
				_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
				_clipboardControl.SetButtonState(ClipboardAction.Copy, false);

				view.EndUpdate();
			}
		}

		private void handleRoleUnSelection(IApplicationRole selectedRole, int itemIndex)
		{
			debindPeople(selectedRole);

			IList<IApplicationFunction> functionList = selectedRole.ApplicationFunctionCollection.ToList();
			handleRoleUnSelection(itemIndex, functionList);

			IAvailableData availableData = PermissionsExplorerStateHolder.GetAvailableDataInPermissionsDataDictionary(selectedRole) ??
										   AvailableData.FindByApplicationRole(_availableDataCollection, selectedRole);

			if (availableData != null)
			{
				_availableDataForSelectedRoles.Remove(availableData);
				setDataTreeCheckState(_availableDataForSelectedRoles);
			}
		}

		private void initializePermissionsExplorer()
		{

			contextMenuStripClipboard.Visible = false;
			listViewPeople.ListViewItemSorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.Ascending };

			setPermissionOnControls();
			initializeDataTreeRangeOptions();
			buildDataTree();

			panelRoles.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			PeopleAssignedBarPanel.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelFunc.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			panelData.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			RolesBarItem.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			PeopleBarItem.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			FunctionsBarItem.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			DataBarItem.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
			ExplorerRibbon.MenuButtonText = UserTexts.Resources.FileProperCase.ToUpper();
			showRolesHeader();
			showPeopleHeader();
			showFunctionsHeader();
			showDataHeader();

			treeViewData.CollapseAll();
			treeViewFunctions.CollapseAll();

			setToolStripsToPreferredSize();

			// Add event handlers at this point to eliminate unnecessary event handler calls
			listViewRoles.ItemSelectionChanged += listViewRolesItemSelectionChanged;
			listViewRoles.SelectedIndexChanged += listViewRolesSelectedIndexChanged;
			listViewRoles.KeyUp += listViewRolesKeyUp;
			listViewRoles.AfterLabelEdit += listViewRolesAfterLabelEdit;

			treeViewFunctions.BeforeCheck += treeViewFunctionsBeforeCheck;
			treeViewFunctions.AfterCheck += treeViewFunctionsAfterCheck;

			// for the data view tree
			treeViewData.BeforeCheck += treeViewDataBeforeCheck;
			treeViewData.AfterCheck += treeViewDataAfterCheck;
			if (listViewRoles.Items.Count > 0) listViewRoles.Items[0].Selected = true;
			listViewRoles.Select();
		}

		private void removeDeletedRoleFromPeople(IApplicationRole deletedApplicationRole)
		{
			ICollection<IPersonInRole> personCollection = PermissionsExplorerStateHolder.GetPersonInPermissionsDataDictionary(deletedApplicationRole);

			// Remove roles for each person selected.
			foreach (var person in personCollection)
			{
				removeApplicationRoleAndCache(person.Id, deletedApplicationRole);
			}
		}

		protected virtual void OnSaved()
		{
			var handler = Saved;
			if (handler!= null)
			{
				handler(this, EventArgs.Empty);
			}
		}

		private void permissionsExplorerLoad(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			initializePermissionsExplorer();
			listViewRoles.Select();
			Cursor.Current = Cursors.Default;
			ExplorerRibbon.QuickPanelVisible = true;
		}

		private void listViewRolesAfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			var item = listViewRoles.Items[e.Item] as ExtentListItem;
			if (!string.IsNullOrEmpty(e.Label) && !isBuiltIn(item))
			{
				// HACK: This method might invoke when adding a new item into the list.
				// TODO: Save new role on the roles list.

				if (item != null)
				{
					var role = item.TagObject as IApplicationRole;

					if (role != null)
					{
						if (role.DescriptionText.Length == e.Label.Length)
						{
							if (string.CompareOrdinal(role.DescriptionText, 1, e.Label, 1, 1) == 0)
							{
								if (string.Compare(e.Label, role.DescriptionText, StringComparison.OrdinalIgnoreCase) == 0)
								{
									e.CancelEdit = true;
									return;
								}
							}
						}

						if (e.Label.Length > 255)
						{
							e.CancelEdit = true;
							return;
						}

						role.DescriptionText = e.Label;
						role.Name = e.Label.Replace(" ", string.Empty);
						if (role.Name.Length > 50) role.Name = role.Name.Substring(0, 50);

						if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> PermissionsExplorerStateHolder.AddOrUpdateApplicationRole(role)))
						{
							FormKill();
						}
					}
				}
			}
			else
			{
				e.CancelEdit = true;
			}
		}

		private void toolStripButtonSaveClick(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			save();
			Cursor.Current = Cursors.Default;
		}

		private void listViewRolesKeyUp(object sender, KeyEventArgs e)
		{
			//check for delete key
			if (e.KeyCode == Keys.Delete && !e.Alt && !e.Control && !e.Shift)
			{
				if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(handleDeleteRole))
				{
					FormKill();
				}
			}
		}

		private void listViewPeopleKeyUp(object sender, KeyEventArgs e)
		{
			//check for delete key
			if (e.KeyCode == Keys.Delete && !e.Alt && !e.Control && !e.Shift)
			{
				handleRemovePeople();
			}
		}

		private void listViewPeopleColumnClick(object sender, ColumnClickEventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			var sorter = listViewPeople.ListViewItemSorter as ListViewColumnSorter;
			if (sorter != null)
			{
				sorter.Order = sorter.Order == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
				sorter.SortColumn = e.Column;
			}
			listViewPeople.Sort();

			Cursor.Current = Cursors.Default;
		}

		private void toggleEnabledstate()
		{
			if (listViewRoles.SelectedItems.Count > 0)
			{
				_clipboardControl.SetButtonState(ClipboardAction.Copy, true);
				foreach (ExtentListItem item in listViewRoles.SelectedItems)
				{
					if (isBuiltIn(item))
					{
						treeViewFunctions.Enabled = false;
						treeViewData.Enabled = false;
						_clipboardControl.SetButtonState(ClipboardAction.Copy, false);
						return;
					}
				}
				treeViewFunctions.Enabled = true;
				treeViewData.Enabled = true;
				_clipboardControl.SetButtonState(ClipboardAction.Copy, true);
			}
			else
				_clipboardControl.SetButtonState(ClipboardAction.Copy, false);
		}

		private void listViewRolesSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!isRoleSelected())
			{
				//Clear the controls
				clearControls();
			}
		}

		private void treeViewFunctionsAfterCheck(object sender, TreeNodeAdvEventArgs e)
		{
			if (e.Node.CheckState == CheckState.Checked)
			{
				var function = e.Node.TagObject as IApplicationFunction;

				if (function != null)
				{
					_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> insertFunctionIntoSelectedRoles(function));
				}
			}

			handleTreeViewFunctionsCheck(e.Node, e.Node.CheckState);
		}

		private void treeViewFunctionsBeforeCheck(object sender, TreeNodeAdvBeforeCheckEventArgs e)
		{
			if (e.NewCheckState == CheckState.Unchecked)
			{
				int childrenCount = e.Node.Nodes.Count;
				var function = e.Node.TagObject as IApplicationFunction;

				if (function != null)
				{
					if (childrenCount == 0)
					{
						//Check whether the parent node also has to be unchecked and removed from the roles
						if (needToRemoveParentNode(e.Node))
						{
							//Remove from the parent node
							function = e.Node.Parent.TagObject as IApplicationFunction;

							if (function != null)
							{
								IList<IApplicationFunction> functions = new List<IApplicationFunction> { function };
								if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> removeFunctionsFromSelectedRoles(functions)))
								{
									FormKill();
									return;
								}
							}
						}
						else
						{
							//Just remove only this unchecked node
							//Remove the selected function from the selected roles
							if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> removeFunctionFromSelectedRoles(function)))
							{
								FormKill();
								return;
							}
						}
					}
					else if (childrenCount > 0)
					{
						IList<IApplicationFunction> functions = getAllFunctionsToBeAddedOrRemoved(e.Node);

						if (functions != null)
						{
							functions.Clear();
							functions.Add(e.Node.TagObject as IApplicationFunction);
							if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(()=> removeFunctionsFromSelectedRoles(functions)))
							{
								FormKill();
							}
						}
					}
				}
			}
		}

		private void treeViewDataBeforeCheck(object sender, TreeNodeAdvBeforeCheckEventArgs e)
		{
			if (e.Node.TagObject is AvailableDataRangeOption)
			{
				CheckState before = _rangeOptionCheckBoxesCollection[e.Node.Index].CheckState;
				if (before == CheckState.Checked) //going to (un)check the same option
				{
					//e.Cancel = true; this is not needed without the None checkbox
					changeRangeOptionInRoles(AvailableDataRangeOption.None);
				}
				else //new range option is selected
				{
					changeCheckStatusOfRangeOptionChkBoxes(e.Node.TagObject);
					changeRangeOptionInRoles((AvailableDataRangeOption)e.Node.TagObject);
				}
			}
		}

		private void treeViewDataAfterCheck(object sender, TreeNodeAdvEventArgs e)
		{
			if (e.Node.CheckState == CheckState.Checked)
			{
				Object itemToInsert = e.Node.TagObject;
				if (itemToInsert != null)
				{
					insertCheckedItemForSelectedRoles(itemToInsert, e.Node);
				}
			}
			else if (e.Node.CheckState == CheckState.Unchecked)
			{
				Object itemToRemove = e.Node.TagObject;
				if (itemToRemove != null)
				{
					removeUnCheckedItemFromSelectedRoles(itemToRemove, e.Node);
				}
			}

			treeViewData.BeforeCheck -= treeViewDataBeforeCheck;
			treeViewData.AfterCheck -= treeViewDataAfterCheck;

			changeStatusOfAllChildrenNodes(e.Node);
			changeStatusOfParentNode(e.Node.Parent);

			treeViewData.BeforeCheck += treeViewDataBeforeCheck;
			treeViewData.AfterCheck += treeViewDataAfterCheck;
		}

		private void permissionsExplorerFormClosing(object sender, FormClosingEventArgs e)
		{
			// queue the people also 
			PermissionsExplorerStateHolder.QueueDirtyPeopleCollection();

			if (PermissionsExplorerStateHolder.UnitOfWork.IsDirty())
			{
				DialogResult response =
					ShowYesNoMessage( UserTexts.Resources.DoYouWantToSaveChangesYouMade,
						Text, MessageBoxDefaultButton.Button1);

				switch (response)
				{
					case DialogResult.No:
						// Nothing to do. Just close the form.
						break;
					case DialogResult.Yes:
						// Save changes and close.
						toolStripButtonSave.PerformClick();
						break;
					default:
						// Do not close the form.
						e.Cancel = true;
						break;
				}
			}
			if (!e.Cancel && _permissionsViewerPresenter != null)
				_permissionsViewerPresenter.CloseViewer();

		}

		
		private void toolStripButtonRenameRoleClick(object sender, EventArgs e)
		{
			handleRenameRole();
		}

		private void copyToolStripMenuItemClick(object sender, EventArgs e)
		{
			handleRoleCopy();
		}

		private void pasteToolStripMenuItemClick(object sender, EventArgs e)
		{
			handleRolePaste();
		}

		private void contextMenuStripClipboardOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		private void instantiateClipboardControl()
		{
			_clipboardControl = new ClipboardControl();
		   _clipboardControl.ToolStripSplitButtonCut.Visible = false ;
			var clipboardhost = new ToolStripControlHost(_clipboardControl);
			toolStripExClipboard.Items.Add(clipboardhost);

			_clipboardControl.CopyClicked += clipboardControlCopyClicked;

			_clipboardControl.PasteClicked += clipboardControlPasteClicked;

			_clipboardControl.SetButtonState(ClipboardAction.Cut, false);
			_clipboardControl.SetButtonState(ClipboardAction.Paste, false);
			_clipboardControl.SetButtonState(ClipboardAction.Copy, false);
		}

		private void clipboardControlCopyClicked(object sender, EventArgs e)
		{
			handleRoleCopy();
		}

		private void clipboardControlPasteClicked(object sender, EventArgs e)
		{
			handleRolePaste();
		}

		private void handleRoleCopy()
		{
			bool canCopy = copy(listViewRoles);
			_clipboardControl.SetButtonState(ClipboardAction.Paste, canCopy);
		}

		private void handleRolePaste()
		{
			listViewRoles.ItemSelectionChanged -= listViewRolesItemSelectionChanged;
			listViewRoles.SelectedIndexChanged -= listViewRolesSelectedIndexChanged;

			paste(listViewRoles);
			clearControls();

			listViewRoles.ItemSelectionChanged += listViewRolesItemSelectionChanged;
			listViewRoles.SelectedIndexChanged += listViewRolesSelectedIndexChanged;
		}

		#region Database connected methods > methods that will open a database connection

		public void LoadDatabaseData()
		{
			bindThisList(PermissionsExplorerHelper.LoadAllApplicationRoles().ToList());
			loadAllApplicationFunctions(PermissionsExplorerHelper.LoadAllApplicationFunctions().ToList());
			_availableDataCollection = PermissionsExplorerHelper.GetAllAvailableDataForOneBusinessUnit().ToList();
		}

		private IList<IEnumerable<IBusinessUnit>> loadAvailableData(ICollection<IAvailableData> dataCollection, out IList<IEnumerable<ISite>> siteAll, out IList<IEnumerable<ITeam>> teamAll, out IList<IEnumerable<AvailableDataRangeOption>> dataRangeOptionAll)
		{
			IList<IEnumerable<IBusinessUnit>> buAll = new List<IEnumerable<IBusinessUnit>>();
			siteAll = new List<IEnumerable<ISite>>();
			teamAll = new List<IEnumerable<ITeam>>();
			dataRangeOptionAll = new List<IEnumerable<AvailableDataRangeOption>>();

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				// populate collections
				foreach (IAvailableData availableData in dataCollection)
				{
					var currentAvailableData = availableData;
					if (!PermissionsExplorerStateHolder.UnitOfWork.Contains(currentAvailableData))
					{
						currentAvailableData = uow.Merge(availableData);
					}

					if (currentAvailableData.AvailableBusinessUnits.Count > 0)
					{
						buAll.Add(currentAvailableData.AvailableBusinessUnits);
					}

					if (currentAvailableData.AvailableSites.Count > 0)
					{
						siteAll.Add(currentAvailableData.AvailableSites);
					}

					if (currentAvailableData.AvailableTeams.Count > 0)
					{
						teamAll.Add(currentAvailableData.AvailableTeams);
					}

					// AvailableData object always has a value to Enum AvailableDataRangeOption
					var dataRangeOptionCollection = new List<AvailableDataRangeOption> { currentAvailableData.AvailableDataRange };
					dataRangeOptionAll.Add(dataRangeOptionCollection);
				}
			}
			return buAll;
		}

		private void listViewRolesItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			try
			{
				Cursor.Current = Cursors.WaitCursor;
				toggleEnabledstate();
				handleRoleSelectionAndUnSelection(e);
				Cursor.Current = Cursors.Default;
			}
			catch (DataSourceException dataSourceException)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(dataSourceException);
				FormKill();
			}
		}

		private void handleRoleSelectionAndUnSelection(ListViewItemSelectionChangedEventArgs e)
		{
			var extentListItem = e.Item as ExtentListItem;

			if (extentListItem != null)
			{
				var selectedRole = extentListItem.TagObject as IApplicationRole;
				if (selectedRole != null)
				{
					treeViewData.BeforeCheck -= treeViewDataBeforeCheck;
					treeViewData.AfterCheck -= treeViewDataAfterCheck;
					listViewRoles.SelectedIndexChanged -= listViewRolesSelectedIndexChanged;
					//suspends all paintings until tree collapses
					treeViewData.BeginUpdate(BeginUpdateOptions.None);
					treeViewFunctions.BeginUpdate(BeginUpdateOptions.None);

					if (e.IsSelected)
					{
						if (selectedRole.Id.HasValue)
						{
							handleRoleSelection(selectedRole, e.ItemIndex);
						}
					}
					else
					{
						handleRoleUnSelection(selectedRole, e.ItemIndex);
					}

					changeCheckStateOfFunctionTree();
					validateFunctionsTreeParentNodesState(treeViewFunctions);
					// Set data tree parent nodes check state after roles are selected
					setDataTreeParentNodesCheckState();

					treeViewData.CollapseAll();
					treeViewData.EndUpdate();
					treeViewFunctions.CollapseAll();
					treeViewFunctions.EndUpdate();

					listViewRoles.SelectedIndexChanged += listViewRolesSelectedIndexChanged;
					treeViewData.BeforeCheck += treeViewDataBeforeCheck;
					treeViewData.AfterCheck += treeViewDataAfterCheck;
				}
			}
		}

		private void handleRoleSelection(IApplicationRole selectedRole, int itemIndex)
		{
			var peopleCollection = PermissionsExplorerStateHolder.GetPersonInPermissionsDataDictionary(selectedRole);
			if (peopleCollection == null)
			{
				peopleCollection = PermissionsExplorerHelper.LoadPeopleByApplicationRole(selectedRole).ToList();
			}
			bindPeople(peopleCollection);

			handleRoleSelection(itemIndex, selectedRole.ApplicationFunctionCollection.ToList());

			IAvailableData availableData = PermissionsExplorerStateHolder.GetAvailableDataInPermissionsDataDictionary(selectedRole) ??
										   AvailableData.FindByApplicationRole(_availableDataCollection, selectedRole);

			if (availableData != null && !_availableDataForSelectedRoles.Contains(availableData)) //function explicitly returns null if no AvailableData
			{
				_availableDataForSelectedRoles.Add(availableData);
				setDataTreeCheckState(_availableDataForSelectedRoles);
			}

			// Add role to the dictionary
			var permissionsDataHolder = new PermissionsDataHolder(peopleCollection, availableData, false);
			PermissionsExplorerStateHolder.AddRoleToPermissionsDataDictionary(selectedRole, permissionsDataHolder);
		}

		private void handleInsertPeople()
		{
			if (isRoleSelected())
			{
				var inner = _container.Resolve<ILifetimeScope>().BeginLifetimeScope();
				using (var peopleInsertScreen = new PeopleInsertScreen(this, inner))
				{
					if (peopleInsertScreen.ShowDialog(this) == DialogResult.OK)
					{
						foreach (ExtentListItem rItem in listViewRoles.SelectedItems)
						{
							var role = rItem.TagObject as IApplicationRole;
							var tempPersons = PermissionsExplorerHelper.LoadPeopleNotInApplicationRole(role,
																									   SelectedPersonsToAddToRole);
							foreach (var id in SelectedPersonsToAddToRole)
							{
								addApplicationRoleAndCache(id, role);
							}

							PermissionsExplorerStateHolder.AssignPersonInPermissionsDataDictionary(role, tempPersons);
							handleRoleSelection(role, rItem.Index);
						}
					   showPeopleHeader();
					 }
					if(peopleInsertScreen.DialogResult == DialogResult.Abort)
					{
						FormKill();
					}

				}
			}
		}

		#endregion

		private void permissionsExplorerKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.M && e.Shift && e.Alt)
			{
				toolStripExViewer.Visible = !toolStripExViewer.Visible;
			}
		}

		private void toolStripButtonShowViewerClick(object sender, EventArgs e)
		{
			if(_permissionsViewerPresenter == null || _permissionsViewerPresenter.Unloaded)
				_permissionsViewerPresenter = _container.Resolve<ILifetimeScope>().BeginLifetimeScope().Resolve<IPermissionViewerRolesPresenter>();

			_permissionsViewerPresenter.ShowViewer();
		}

		private void toolStripButtonAddRoleClick(object sender, EventArgs e)
		{
			if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(handleAddNewRole))
			{
				FormKill();
			}
		}

		private void toolStripButtonDeleteRoleClick(object sender, EventArgs e)
		{
			//set disabled instead?
			if (_lastInFocus != listViewRoles) return;
			if (!_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(handleDeleteRole))
			{
				FormKill();
			}
			
		}

		private void toolStripButtonAddPersonClick(object sender, EventArgs e)
		{
			_dataSourceExceptionHandler.AttemptDatabaseConnectionDependentAction(handleInsertPeople);
		}

		private void toolStripButtonRemovePersonClick(object sender, EventArgs e)
		{
			if (_lastInFocus != listViewPeople) return;
			handleRemovePeople();
		}

		private void backStageButton1Click(object sender, EventArgs e)
		{
			save();
		}

		private void backStageButton2Click(object sender, EventArgs e)
		{
			Close();
		}

		private void backStageButton3Click(object sender, EventArgs e)
		{
			var toggleManager = _container.Resolve<IToggleManager>();
			try
			{
				var settings = new SettingsScreen(new OptionCore(new OptionsSettingPagesProvider(toggleManager)));
				settings.Show();
			}
			catch (DataSourceException ex)
			{
				DatabaseLostConnectionHandler.ShowConnectionLostFromCloseDialog(ex);
			}
		}

		private void backStageButton4Click(object sender, EventArgs e)
		{
			if (!CloseAllOtherForms(this)) return;

			Close();

			////this canceled
			if (Visible)
				return;
			Application.Exit();
		}

	}
}
