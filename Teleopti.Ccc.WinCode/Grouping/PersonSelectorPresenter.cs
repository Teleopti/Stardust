using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Grouping.Commands;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping
{
	public interface IPersonSelectorPresenter : IDisposable
	{
		HashSet<Guid> SelectedPersonGuids { get; }
		HashSet<Guid> CheckedPersonGuids { get; }
		DateOnly SelectedDate { get; }
		void LoadTabs();
		IApplicationFunction ApplicationFunction { get; set; }
		bool ShowPersons { get; set; }
		bool ShowUsers { get; set; }
		bool ShowFind { get; set; }
		IPersonSelectorView View { get; }
		string SelectedGroupPageKey();
		void SetSelectedTab(string groupPageKey);
		bool IsOnOrganizationTab { get; }
		HashSet<Guid> SelectedTeamGuids { get; }
		void SetSelectedPersonGuids(HashSet<Guid> guidsToCheck);
	}

	public class PersonSelectorPresenter : IPersonSelectorPresenter
	{
		private IPersonSelectorView _personSelectorView;
		private ICommandProvider _commandProvider;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;
		private IEventAggregator _eventAggregator;
		private IAddGroupPageCommand _addGroupPageCommand;
		private IDeleteGroupPageCommand _deleteGroupPageCommand;
		private IModifyGroupPageCommand _modifyGroupPageCommand;
		private IRenameGroupPageCommand _renameGroupPageCommand;
		private IOpenModuleCommand _openModuleCommand;
		private IEventAggregator _globalEventAggregator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "10"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "4")]
		public PersonSelectorPresenter(IPersonSelectorView personSelectorView, ICommandProvider commandProvider,
			 IUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository, IEventAggregator eventAggregator,
			 IAddGroupPageCommand addGroupPageCommand, IDeleteGroupPageCommand deleteGroupPageCommand,
			 IModifyGroupPageCommand modifyGroupPageCommand, IRenameGroupPageCommand renameGroupPageCommand, IOpenModuleCommand openModuleCommand,
			 IEventAggregatorLocator eventAggregatorLocator)
		{
			_personSelectorView = personSelectorView;
			_commandProvider = commandProvider;
			_unitOfWorkFactory = unitOfWorkFactory;
			_personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
			_eventAggregator = eventAggregator;
			_globalEventAggregator = eventAggregatorLocator.GlobalAggregator();
			_addGroupPageCommand = addGroupPageCommand;
			_deleteGroupPageCommand = deleteGroupPageCommand;
			_modifyGroupPageCommand = modifyGroupPageCommand;
			_renameGroupPageCommand = renameGroupPageCommand;
			_openModuleCommand = openModuleCommand;
			_eventAggregator.GetEvent<AddGroupPageClicked>().Subscribe(addGroupPageClicked);
			_eventAggregator.GetEvent<DeleteGroupPageClicked>().Subscribe(deleteGroupPageClicked);
			_eventAggregator.GetEvent<ModifyGroupPageClicked>().Subscribe(modifyGroupPageClicked);
			_globalEventAggregator.GetEvent<GroupPageAdded>().Subscribe(groupPageAdded);
			_globalEventAggregator.GetEvent<GroupPageDeleted>().Subscribe(groupPageDeleted);
			_globalEventAggregator.GetEvent<GroupPageRenamed>().Subscribe(groupPageRenamed);
			_eventAggregator.GetEvent<RenameGroupPageClicked>().Subscribe(renameGroupPage);
			_eventAggregator.GetEvent<SelectedNodesChanged>().Subscribe(selectedNodesChanged);
			_globalEventAggregator.GetEvent<PeopleSaved>().Subscribe(peopleSaved);
			_eventAggregator.GetEvent<RefreshGroupPageClicked>().Subscribe(refreshGroupPage);
			_eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Subscribe(nodeCheckChanged);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "t")]
		private void nodeCheckChanged(string obj)
		{
			var t = CheckedPersonGuids;
		}

		private void refreshGroupPage(string obj)
		{
			var tab = _personSelectorView.SelectedTab;
			if (tab != null)
			{
				var command = tab.Tag as IExecutableCommand;
				if (command != null)
				{
					command.Execute();
				}
			}
		}

		private void peopleSaved(string obj)
		{
			refreshGroupPage(obj);
		}

		private void selectedNodesChanged(string obj)
		{
			updateView();
		}

		private void renameGroupPage(string obj)
		{
			_renameGroupPageCommand.Execute();
		}

		private void modifyGroupPageClicked(string obj)
		{
			_modifyGroupPageCommand.Execute();
		}

		private void groupPageDeleted(EventGroupPage obj)
		{
			var tabControl = _personSelectorView.TabControl;
			//not loaded yet
			if (tabControl.TabPages == null) return;
			foreach (TabPageAdv tab in tabControl.TabPages)
			{
				var command = tab.Tag as ILoadUserDefinedTabsCommand;
				if (command != null && command.Id.Equals(obj.Id))
				{
					var isSelected = tabControl.SelectedIndex == tab.TabIndex - 1;
					tabControl.TabPages.Remove(tab);
					if (isSelected)
						tabControl.SelectedIndex = 0;
				}
			}
		}

		private void groupPageRenamed(EventGroupPage obj)
		{
			var tabControl = _personSelectorView.TabControl;
			foreach (TabPageAdv tab in tabControl.TabPages)
			{
				var command = tab.Tag as ILoadUserDefinedTabsCommand;
				if (command != null && command.Id.Equals(obj.Id))
				{
					tab.Text = obj.Name;
					if (tabControl.SelectedIndex == tab.TabIndex - 1)
						command.Execute();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void groupPageAdded(EventGroupPage eventGroupPage)
		{
			// the control isn't loaded yet
			if (_personSelectorView.TabControl.TabPages == null) return;

			var tab = new TabPageAdv(eventGroupPage.Name)
			{
				Tag = _commandProvider.GetLoadUserDefinedTabsCommand(_personSelectorView, eventGroupPage.Id, ApplicationFunction)
			};

			_personSelectorView.AddNewPageTab(tab);
		}

		private void deleteGroupPageClicked(string obj)
		{
			_deleteGroupPageCommand.Execute();
		}

		private void addGroupPageClicked(string obj)
		{
			_addGroupPageCommand.Execute();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void LoadTabs()
		{
			var tabs = new List<TabPageAdv>
									{
										 new TabPageAdv(Resources.Contract)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Contract,
																												  _personSelectorView,
																												  Resources.Contract,ApplicationFunction),
											  },
										 new TabPageAdv(Resources.ContractSchedule)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(
															  PersonSelectorField.ContractSchedule, _personSelectorView,
															  Resources.ContractSchedule,ApplicationFunction)
											  },
										 new TabPageAdv(Resources.PartTimePercentageHeader)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(
															  PersonSelectorField.PartTimePercentage, _personSelectorView,
															  Resources.PartTimePercentageHeader,ApplicationFunction)
											  },
										 new TabPageAdv(Resources.Note)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Note,
																												  _personSelectorView,
																												  Resources.Note,ApplicationFunction)
											  },
										 new TabPageAdv(Resources.RuleSetBag)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.ShiftBag,
																												  _personSelectorView,
																												  Resources.RuleSetBag,ApplicationFunction)
											  },
										 new TabPageAdv(Resources.Skill)
											  {
													Tag =
														 _commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Skill,
																												  _personSelectorView,
																												  Resources.Skill,ApplicationFunction)
											  }
									};

			using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				var userTabs = _personSelectorReadOnlyRepository.GetUserDefinedTabs();
				foreach (var userDefinedTabLight in userTabs.OrderBy(u => u.Name))
				{
					tabs.Add(new TabPageAdv(userDefinedTabLight.Name)
					{
						Tag = _commandProvider.GetLoadUserDefinedTabsCommand(_personSelectorView, userDefinedTabLight.Id, ApplicationFunction)
					});
				}
			}

			_personSelectorView.ResetTabs(tabs.ToArray(), _commandProvider.GetLoadOrganizationCommand(ApplicationFunction, ShowPersons, ShowUsers));
		}

		public HashSet<Guid> SelectedPersonGuids
		{
			get
			{
				var allGuids = new HashSet<Guid>();
				foreach (var selectedNode in _personSelectorView.SelectedNodes)
				{
					addGuidsRecursive(allGuids, selectedNode);
				}
				return allGuids;
			}
		}

		public HashSet<Guid> CheckedPersonGuids
		{
			get
			{
				var allGuids = new HashSet<Guid>();
				foreach (var node in _personSelectorView.AllNodes)
				{
					addGuidsRecursiveFromChecked(allGuids, node);
				}
				_personSelectorView.PreselectedPersonIds = allGuids;
				return allGuids;
			}
		}

		public DateOnly SelectedDate
		{
			get { return _personSelectorView.SelectedDate; }
		}

		public IApplicationFunction ApplicationFunction { get; set; }

		public bool ShowPersons { get; set; }

		public bool ShowUsers { get; set; }

		public bool ShowFind { get; set; }

		public IPersonSelectorView View
		{
			get
			{
				return _personSelectorView;
			}
		}

		public bool IsOnOrganizationTab
		{
			get
			{
				var tab = _personSelectorView.SelectedTab;
				if (tab != null)
				{
					var command = tab.Tag as ILoadOrganizationCommand;
					if (command != null)
						return true;
				}
				return false;
			}
		}

		public HashSet<Guid> SelectedTeamGuids
		{
			get
			{
				var retTeams = new HashSet<Guid>();
				if (IsOnOrganizationTab)
				{
					foreach (var selectedNode in _personSelectorView.SelectedNodes)
					{
						var guids = selectedNode.TagObject as List<Guid>;
						if (guids != null)
						{
							foreach (var guid in guids)
							{
								retTeams.Add(guid);
							}
						}
					}
				}
				return retTeams;
			}
		}

		public string SelectedGroupPageKey()
		{
			var key = string.Empty;
			if (_personSelectorView.TabControl.SelectedTab != null)
			{
				var command = _personSelectorView.TabControl.SelectedTab.Tag as ILoadGroupPageCommand;
				if (command != null)
					key = command.Key;
			}

			return key;
		}

		public void SetSelectedTab(string groupPageKey)
		{
			foreach (TabPageAdv tabPage in _personSelectorView.TabControl.TabPages)
			{
				var command = tabPage.Tag as ILoadGroupPageCommand;
				if (command != null && groupPageKey == command.Key)
					_personSelectorView.TabControl.SelectedIndex = tabPage.TabIndex - 1;
			}
		}

		public void SetSelectedPersonGuids(HashSet<Guid> guidsToCheck)
		{
			foreach (var node in _personSelectorView.AllNodes)
			{
				selectNodesRecursive(guidsToCheck, node);
			}
		}

		private void selectNodesRecursive(HashSet<Guid> guidsToCheck, TreeNodeAdv fromThisNode)
		{
			var lst = fromThisNode.Tag as IList<Guid>;
			if (lst != null && lst.Count == 1)
			{
				fromThisNode.Checked = guidsToCheck.Contains(lst.First());
			}

			foreach (var node in fromThisNode.Nodes)
			{
				selectNodesRecursive(guidsToCheck, (TreeNodeAdv)node);
			}
		}

		private static void addGuidsRecursive(HashSet<Guid> addToThis, TreeNodeAdv fromThisNode)
		{
			var lst = fromThisNode.Tag as IList<Guid>;
			if (lst != null)
				addToThis.UnionWith(lst);
			foreach (var node in fromThisNode.Nodes)
			{
				addGuidsRecursive(addToThis, (TreeNodeAdv)node);
			}
		}

		private static void addGuidsRecursiveFromChecked(HashSet<Guid> addToThis, TreeNodeAdv fromThisNode)
		{
			var lst = fromThisNode.Tag as IList<Guid>;
			if (lst != null && fromThisNode.Checked)
				addToThis.UnionWith(lst);
			foreach (var node in fromThisNode.Nodes)
			{
				addGuidsRecursiveFromChecked(addToThis, (TreeNodeAdv)node);
			}
		}

		private void updateView()
		{
			_personSelectorView.OpenEnabled = _openModuleCommand.CanExecute();
			_personSelectorView.FindVisible = ShowFind;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_eventAggregator.GetEvent<AddGroupPageClicked>().Unsubscribe(addGroupPageClicked);
				_eventAggregator.GetEvent<DeleteGroupPageClicked>().Unsubscribe(deleteGroupPageClicked);
				_eventAggregator.GetEvent<ModifyGroupPageClicked>().Unsubscribe(modifyGroupPageClicked);
				_globalEventAggregator.GetEvent<GroupPageAdded>().Unsubscribe(groupPageAdded);
				_globalEventAggregator.GetEvent<GroupPageDeleted>().Unsubscribe(groupPageDeleted);
				_globalEventAggregator.GetEvent<GroupPageRenamed>().Unsubscribe(groupPageRenamed);
				_eventAggregator.GetEvent<RenameGroupPageClicked>().Unsubscribe(renameGroupPage);
				_eventAggregator.GetEvent<SelectedNodesChanged>().Unsubscribe(selectedNodesChanged);
				_globalEventAggregator.GetEvent<PeopleSaved>().Unsubscribe(peopleSaved);
				_eventAggregator.GetEvent<RefreshGroupPageClicked>().Unsubscribe(refreshGroupPage);
				_eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Unsubscribe(nodeCheckChanged);

				if (_personSelectorView != null)
				{
					_personSelectorView.Dispose();
					_personSelectorView = null;
				}
				if (_commandProvider != null)
					_commandProvider.Dispose();
				_commandProvider = null;
				_unitOfWorkFactory = null;
				_personSelectorReadOnlyRepository = null;
				_eventAggregator = null;
				_globalEventAggregator = null;
				_addGroupPageCommand = null;
				_deleteGroupPageCommand = null;
				_modifyGroupPageCommand = null;
				_renameGroupPageCommand = null;
				_openModuleCommand = null;
			}


		}

	}
}
