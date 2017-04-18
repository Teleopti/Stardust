using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping.Events;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
	[TestFixture]
	public class PeopleNavigatorPresenterTest
	{
		private MockRepository _mocks;
		private IOpenModuleCommand _openModuleCommand;
		private EventAggregator _eventAggregator;
		private PeopleNavigatorPresenter _target;
		private IPeopleNavigator _view;
		private IAddGroupPageCommand _addGroupPageCommand;
		private IDeleteGroupPageCommand _deleteGroupPageCommand;
		private IModifyGroupPageCommand _modifyGroupPageCommand;
		private IRenameGroupPageCommand _renameGroupPageCommand;
		private ISendInstantMessageCommand _sendInstantMessageCommand;
		private ISendInstantMessageEnableCommand _sendInstantMessageEnableCommand;
	    private IAddPersonEnableCommand _addPersonEnableCommand;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_openModuleCommand = _mocks.StrictMock<IOpenModuleCommand>();
			_addGroupPageCommand = _mocks.StrictMock<IAddGroupPageCommand>();
			_deleteGroupPageCommand = _mocks.StrictMock<IDeleteGroupPageCommand>();
			_modifyGroupPageCommand = _mocks.StrictMock<IModifyGroupPageCommand>();
			_renameGroupPageCommand = _mocks.StrictMock<IRenameGroupPageCommand>();
			_sendInstantMessageCommand = _mocks.StrictMock<ISendInstantMessageCommand>();
			_sendInstantMessageEnableCommand = _mocks.StrictMock<ISendInstantMessageEnableCommand>();
		    _addPersonEnableCommand = _mocks.StrictMock<IAddPersonEnableCommand>();
			_eventAggregator = new EventAggregator();
			_view = _mocks.StrictMock<IPeopleNavigator>();
			_target = new PeopleNavigatorPresenter(_eventAggregator, _openModuleCommand, _addGroupPageCommand, _deleteGroupPageCommand,
                _modifyGroupPageCommand, _renameGroupPageCommand, _sendInstantMessageCommand, _sendInstantMessageEnableCommand, _addPersonEnableCommand);
			_target.Init(_view);
		}

		[Test]
		public void ShouldUpdateViewHowCommandCanExecute()
		{
			Expect.Call(_openModuleCommand.CanExecute()).Return(false);
			Expect.Call(_addGroupPageCommand.CanExecute()).Return(false);
			Expect.Call(_deleteGroupPageCommand.CanExecute()).Return(false);
			Expect.Call(_modifyGroupPageCommand.CanExecute()).Return(false);
			Expect.Call(_renameGroupPageCommand.CanExecute()).Return(false);
			Expect.Call(_sendInstantMessageCommand.CanExecute()).Return(false);
			Expect.Call(_sendInstantMessageEnableCommand.CanExecute()).Return(false);
		    Expect.Call(_addPersonEnableCommand.CanExecute()).Return(false);
			Expect.Call(() => _view.OpenEnabled = false);
			Expect.Call(() => _view.AddGroupPageEnabled = false);
			Expect.Call(() => _view.DeleteGroupPageEnabled = false);
			Expect.Call(() => _view.ModifyGroupPageEnabled = false);
			Expect.Call(() => _view.RenameGroupPageEnabled = false);
			Expect.Call(() => _view.SendMessageVisible = false);
			Expect.Call(() => _view.SendMessageEnable = false);
            Expect.Call(() => _view.AddNewEnabled = false);
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<SelectedNodesChanged>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallOpenOnOpenClicked()
		{
			Expect.Call(_view.Open);
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<OpenModuleClicked>().Publish("");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallFindOnFindClicked()
		{
			Expect.Call(_view.FindPeople);
			_mocks.ReplayAll();
			_eventAggregator.GetEvent<FindPeopleClicked>().Publish("");
			_mocks.VerifyAll();
		}
	}
}