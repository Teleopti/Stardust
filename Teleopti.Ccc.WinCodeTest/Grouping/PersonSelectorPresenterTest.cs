using System;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Events;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Grouping.Commands;
using Teleopti.Ccc.WinCode.Grouping.Events;
using Teleopti.Ccc.WinCode.PeopleAdmin;
using Teleopti.Ccc.WinCode.PeopleAdmin.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
	[TestFixture]
	public class PersonSelectorPresenterTest
	{
		private MockRepository _mocks;
		private IPersonSelectorView _view;
		private ICommandProvider _commandProvider;
		private PersonSelectorPresenter _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonSelectorReadOnlyRepository _repositoryFactory;
		private ICommonNameDescriptionSetting _commonNameSetting;
		private readonly IApplicationFunction _myApplicationFunction =
			ApplicationFunction.FindByPath(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions,
										   DefinedRaptorApplicationFunctionPaths.OpenPersonAdminPage);

		private EventAggregator _eventAggregator;
		private IAddGroupPageCommand _addGroupPageCommand;
		private IDeleteGroupPageCommand _deleteGroupPageCommand;
		private IModifyGroupPageCommand _modifyGroupPageCommand;
		private IRenameGroupPageCommand _renameGroupPageCommand;
		private IOpenModuleCommand _openCommand;
		private IEventAggregatorLocator _eventAggregatorLocator;
		private EventAggregator _globalEventAggregator;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_view = _mocks.StrictMock<IPersonSelectorView>();
			_commandProvider = _mocks.StrictMock<ICommandProvider>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_repositoryFactory = _mocks.StrictMock<IPersonSelectorReadOnlyRepository>();
			_commonNameSetting = _mocks.StrictMock<ICommonNameDescriptionSetting>();
			_eventAggregatorLocator = _mocks.StrictMock<IEventAggregatorLocator>();
			_eventAggregator = new EventAggregator();
			_globalEventAggregator = new EventAggregator();

			_addGroupPageCommand = _mocks.StrictMock<IAddGroupPageCommand>();
			_deleteGroupPageCommand = _mocks.StrictMock<IDeleteGroupPageCommand>();
			_modifyGroupPageCommand = _mocks.StrictMock<IModifyGroupPageCommand>();
			_renameGroupPageCommand = _mocks.StrictMock<IRenameGroupPageCommand>();
			_openCommand = _mocks.StrictMock<IOpenModuleCommand>();
			Expect.Call(_eventAggregatorLocator.GlobalAggregator()).Return(_globalEventAggregator);
			_mocks.ReplayAll();
			_target = new PersonSelectorPresenter(_view, _commandProvider, _unitOfWorkFactory, _repositoryFactory, _eventAggregator,
				_addGroupPageCommand, _deleteGroupPageCommand, _modifyGroupPageCommand, _renameGroupPageCommand, _openCommand, _eventAggregatorLocator) { ShowPersons = true, ApplicationFunction = _myApplicationFunction };
			_mocks.BackToRecordAll();
		}

		[Test]
		public void ShouldSetTabsInView()
		{
			var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
			var rootGuid = Guid.NewGuid();
			
			_target.ShowUsers = true;
			
			Expect.Call(_commandProvider.GetLoadOrganizationCommand(_myApplicationFunction, true, true)).Return(new LoadOrganizationCommand(_unitOfWorkFactory, _repositoryFactory, _view, _commonNameSetting, _myApplicationFunction, true, true));
            Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Contract, _view, Resources.Contract, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.Contract, _unitOfWorkFactory, _repositoryFactory, _view, Resources.Contract, _commonNameSetting, _myApplicationFunction));
            Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.ContractSchedule, _view, Resources.ContractSchedule, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.ContractSchedule, _unitOfWorkFactory, _repositoryFactory, _view, Resources.ContractSchedule, _commonNameSetting, _myApplicationFunction));
			Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.PartTimePercentage, _view, Resources.PartTimePercentageHeader, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.PartTimePercentage, _unitOfWorkFactory, _repositoryFactory, _view, Resources.PartTimePercentage, _commonNameSetting, _myApplicationFunction));
            Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Note, _view, Resources.Note, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.Note, _unitOfWorkFactory, _repositoryFactory, _view, Resources.Note, _commonNameSetting, _myApplicationFunction));
            Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.ShiftBag, _view, Resources.RuleSetBag, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.ShiftBag, _unitOfWorkFactory, _repositoryFactory, _view, Resources.RuleSetBag, _commonNameSetting, _myApplicationFunction));
            Expect.Call(_commandProvider.GetLoadBuiltInTabsCommand(PersonSelectorField.Skill, _view, Resources.Skill, _myApplicationFunction)).Return(new LoadBuiltInTabsCommand(PersonSelectorField.Skill, _unitOfWorkFactory, _repositoryFactory, _view, Resources.Skill, _commonNameSetting, _myApplicationFunction));

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			Expect.Call(_repositoryFactory.GetUserDefinedTabs()).Return(new List<IUserDefinedTabLight> { new UserDefinedTabLight { Id = rootGuid, Name = "Tabbe" } });
			Expect.Call(_commandProvider.GetLoadUserDefinedTabsCommand(_view, rootGuid, _myApplicationFunction)).Return(
				new LoadUserDefinedTabsCommand(_unitOfWorkFactory, _repositoryFactory, _view, rootGuid, _commonNameSetting, _myApplicationFunction))
				.IgnoreArguments();
			Expect.Call(unitOfWork.Dispose);
			Expect.Call(() => _view.ResetTabs(new TabPageAdv[0], null)).IgnoreArguments();
			_mocks.ReplayAll();
			_target.LoadTabs();
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldDeliverSelectedPersonsGuids()
		{
			var root = new TreeNodeAdv {Tag = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}};
			var next = new TreeNodeAdv {Tag = new List<Guid> {Guid.NewGuid(), Guid.NewGuid()}};
			var nextAgain = new TreeNodeAdv {Tag = new List<Guid>()};
			next.Nodes.Add(nextAgain);
			root.Nodes.Add(next);
		   
			var nodes = new List<TreeNodeAdv>{root};
			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid>{ Guid.NewGuid()} });
			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid() } });

			Expect.Call(_view.SelectedNodes).Return(nodes);
			_mocks.ReplayAll();
			var guids = _target.SelectedPersonGuids;
			Assert.That(guids.Count,Is.EqualTo(6));
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldAddNewTabPageOnAddedEvent()
		{
			var id = Guid.NewGuid();
			var tabControl = new TabControlAdv();
			var newPage = new EventGroupPage {Id = id, Name = "test"};
			Expect.Call(_view.TabControl).Return(tabControl);
			Expect.Call(_commandProvider.GetLoadUserDefinedTabsCommand(_view, id, _myApplicationFunction));
			Expect.Call(() =>_view.AddNewPageTab(null)).IgnoreArguments();
			_mocks.ReplayAll();
			_globalEventAggregator.GetEvent<GroupPageAdded>().Publish(newPage);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldRemoveTabOnDeletedEvent()
		{
			var id = Guid.NewGuid();
			var deletedPage = new EventGroupPage {Id = id, Name = "test"};
			var tabControl = new TabControlAdv();
			var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
			tabControl.TabPages.Add(new TabPageAdv("f�rsta"));
			tabControl.TabPages.Add(new TabPageAdv("andra"));
			tabControl.TabPages.Add(new TabPageAdv("tredje") {Tag = command});
			tabControl.SelectedIndex = 2;
			Expect.Call(_view.TabControl).Return(tabControl);
			Expect.Call(command.Id).Return(id);
			_mocks.ReplayAll();
			Assert.That(tabControl.TabPages.Count, Is.EqualTo(3));
			_globalEventAggregator.GetEvent<GroupPageDeleted>().Publish(deletedPage);
			Assert.That(tabControl.TabPages.Count, Is.EqualTo(2));
			Assert.That(tabControl.SelectedIndex, Is.EqualTo(0));
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldNotRemoveTabOnDeletedEventIfViewNotLoaded()
		{
			var id = Guid.NewGuid();
			var deletedPage = new EventGroupPage { Id = id, Name = "test" };
			var tabControl = _mocks.StrictMock<TabControlAdv>();
			
			Expect.Call(_view.TabControl).Return(tabControl);
			Expect.Call(tabControl.TabPages).Return(null);
			_mocks.ReplayAll();     
			_globalEventAggregator.GetEvent<GroupPageDeleted>().Publish(deletedPage);
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldNotAddTabOnAddEventIfViewNotLoaded()
		{
			var id = Guid.NewGuid();
			var added = new EventGroupPage { Id = id, Name = "test" };
			var tabControl = _mocks.StrictMock<TabControlAdv>();

			Expect.Call(_view.TabControl).Return(tabControl);
			Expect.Call(tabControl.TabPages).Return(null);
			_mocks.ReplayAll();
			_globalEventAggregator.GetEvent<GroupPageAdded>().Publish(added);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldGetSelectedDateFromView()
		{
			Expect.Call(_view.SelectedDate).Return(new DateOnly(2012, 1, 25));
			_mocks.ReplayAll();
			Assert.That(_target.SelectedDate, Is.EqualTo(new DateOnly(2012, 1, 25)));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldHaveView()
		{
			Assert.That(_target.View, Is.EqualTo(_view));
		}

		[Test]
		public void ShouldCallDeleteCommandOnEvent()
		{
			Expect.Call(() => _deleteGroupPageCommand.Execute());
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<DeleteGroupPageClicked>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallAddCommandOnEvent()
		{
			Expect.Call(() => _addGroupPageCommand.Execute());
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<AddGroupPageClicked>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallModifyCommandOnEvent()
		{
			Expect.Call(() => _modifyGroupPageCommand.Execute());
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<ModifyGroupPageClicked>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallRenameCommandOnEvent()
		{
			Expect.Call(() => _renameGroupPageCommand.Execute());
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<RenameGroupPageClicked>().Publish("");
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldChangeTabWhenRenamed()
		{
			var id = Guid.NewGuid();
			var renamedPage = new EventGroupPage { Id = id, Name = "new name" };
			var tabControl = new TabControlAdv();
			var command = _mocks.StrictMock<ILoadUserDefinedTabsCommand>();
			tabControl.TabPages.Add(new TabPageAdv("f�rsta"));
			tabControl.TabPages.Add(new TabPageAdv("andra"));
			tabControl.TabPages.Add(new TabPageAdv("tredje") { Tag = command });
			tabControl.SelectedIndex = 2;
			Expect.Call(_view.TabControl).Return(tabControl);
			Expect.Call(command.Id).Return(id);
			Expect.Call(command.Execute);
			_mocks.ReplayAll();
			Assert.That(tabControl.TabPages.Count, Is.EqualTo(3));
			_globalEventAggregator.GetEvent<GroupPageRenamed>().Publish(renamedPage);
			Assert.That(tabControl.TabPages[2].Text, Is.EqualTo("new name"));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldUpdateViewOnSelectedNodesChanged()
		{
			_target.ShowFind = true;
			Expect.Call(_openCommand.CanExecute()).Return(false);
			Expect.Call(_view.OpenEnabled = false);
			Expect.Call(_view.FindVisible = true);
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<SelectedNodesChanged>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReloadIfTabNotUserDefinedAndPeopleSaved()
		{
			var command = _mocks.StrictMock<ILoadOrganizationCommand>();
			var selectedTab = new TabPageAdv("f�rsta") { Tag = command };
			Expect.Call(_view.SelectedTab).Return(selectedTab);
			Expect.Call(command.Execute);
			_mocks.ReplayAll();
			_globalEventAggregator.GetEvent<PeopleSaved>().Publish("");
			_mocks.VerifyAll();
		}
		[Test]
		public void ShouldDeliverCheckedPersonsGuids()
		{
			var root = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			var next = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			next.Checked = true;
			var nextAgain = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			nextAgain.Checked = true;
			next.Nodes.Add(nextAgain);
			root.Nodes.Add(next);

			var nodes = new List<TreeNodeAdv> { root };
			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid() } });
			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid() } });

			Expect.Call(_view.AllNodes).Return(nodes);
			Expect.Call(_view.PreselectedPersonIds = new HashSet<Guid>()).IgnoreArguments();
			_mocks.ReplayAll();
			var guids = _target.CheckedPersonGuids;
			Assert.That(guids.Count, Is.EqualTo(4));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldSetCheckedPersonsGuidsOnNodeChecked()
		{
			var root = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			var next = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			next.Checked = true;
			var nextAgain = new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
			nextAgain.Checked = true;
			next.Nodes.Add(nextAgain);
			root.Nodes.Add(next);

			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid() } });
			nextAgain.Nodes.Add(new TreeNodeAdv { Tag = new List<Guid> { Guid.NewGuid() } });
			Expect.Call(_view.PreselectedPersonIds).Return(new HashSet<Guid>());
			//Expect.Call(_view.PreselectedPersonIds.Add());
			_mocks.ReplayAll();
			
			_eventAggregator.GetEvent<GroupPageNodeCheckedChange>().Publish(new GroupPageNodeCheckData
			{
				AgentId = Guid.NewGuid(),
				Node = next
			});
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCallLoadCommandOnRefreshEvent()
		{
			var command = _mocks.StrictMock<IExecutableCommand>();
			var tab = new TabPageAdv("tab") {Tag = command};
			Expect.Call(_view.SelectedTab).Return(tab);
			Expect.Call(command.Execute);
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<RefreshGroupPageClicked>().Publish("");
			_mocks.VerifyAll();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
		public void ShouldCheckCommandToSeeWhichTabIsSelected()
		{
			var command = _mocks.StrictMock<ILoadOrganizationCommand>();
			var tab = new TabPageAdv{Tag = command};
			Expect.Call(_view.SelectedTab).Return(tab);
			_mocks.ReplayAll();
			Assert.That(_target.IsOnOrganizationTab,Is.True);
			_mocks.VerifyAll();
			_mocks.BackToRecordAll();
			var otherCommand = _mocks.StrictMock<ILoadBuiltInTabsCommand>();
			tab = new TabPageAdv { Tag = otherCommand };
			Expect.Call(_view.SelectedTab).Return(tab);
			_mocks.ReplayAll();
			Assert.That(_target.IsOnOrganizationTab, Is.False);
			_mocks.VerifyAll();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnTeamIdsIfOnOrganizationTab()
        {
            var team1Id = Guid.NewGuid();
            var team2Id = Guid.NewGuid();
            var team3Id = Guid.NewGuid();

            var site = new TreeNodeAdv { TagObject = new List<Guid> { team1Id, team2Id, team3Id } };
            
            var command = _mocks.StrictMock<ILoadOrganizationCommand>();
            var tab = new TabPageAdv { Tag = command };
            Expect.Call(_view.SelectedTab).Return(tab);
            Expect.Call(_view.SelectedNodes).Return(new List<TreeNodeAdv> { site });
            _mocks.ReplayAll();
            Assert.That(_target.SelectedTeamGuids.Count, Is.EqualTo(3));
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldSelectCorrectTab()
        {
            var tabControl = new TabControlAdv();
            var command = _mocks.StrictMock<ILoadGroupPageCommand>();
            tabControl.TabPages.Add(new TabPageAdv("f�rsta") );
            tabControl.TabPages.Add(new TabPageAdv("andra") { Tag = command });
            tabControl.TabPages.Add(new TabPageAdv("tredje"));
            tabControl.SelectedIndex = 0;
            Expect.Call(_view.TabControl).Return(tabControl).Repeat.AtLeastOnce();
            Expect.Call(command.Key).Return("KEY");
            _mocks.ReplayAll();
            _target.SetSelectedTab("KEY");
            Assert.That(tabControl.SelectedIndex, Is.EqualTo(1));
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), Test]
        public void ShouldReturnKeyFromSelectedTab()
        {
            var tabControl = new TabControlAdv();
            var command = _mocks.StrictMock<ILoadGroupPageCommand>();
            tabControl.TabPages.Add(new TabPageAdv("f�rsta"));
            tabControl.TabPages.Add(new TabPageAdv("andra") { Tag = command });
            tabControl.TabPages.Add(new TabPageAdv("tredje"));
            tabControl.SelectedIndex = 1;
            Expect.Call(_view.TabControl).Return(tabControl).Repeat.AtLeastOnce();
            Expect.Call(command.Key).Return("KEY");
            _mocks.ReplayAll();
            
            Assert.That(_target.SelectedGroupPageKey(), Is.EqualTo("KEY"));
            _mocks.VerifyAll();
        }
	}
}
