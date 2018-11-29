using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class OpenMeetingsOverviewCommandTest
    {
        private MockRepository _mocks;
        private OpenMeetingsOverviewCommand _target;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonSelectorPresenter _personSelectorPresenter;
        private IMeetingOverviewViewFactory _meetingOverviewFactory;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _personSelectorPresenter = _mocks.StrictMock<IPersonSelectorPresenter>();
            _meetingOverviewFactory = _mocks.StrictMock<IMeetingOverviewViewFactory>();
            _target = new OpenMeetingsOverviewCommand( _repositoryFactory, _unitOfWorkFactory, _personSelectorPresenter, _meetingOverviewFactory);
        }

        [Test]
        public void ShouldReturnFalseIfNotAllowed()
        {
            using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
            {
                _target = new OpenMeetingsOverviewCommand(_repositoryFactory, _unitOfWorkFactory, _personSelectorPresenter, _meetingOverviewFactory);
                Assert.That(_target.CanExecute(), Is.False);
            }
        }

        [Test]
        public void ShouldReturnTrueIfAllowed()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target = new OpenMeetingsOverviewCommand(_repositoryFactory, _unitOfWorkFactory, _personSelectorPresenter, _meetingOverviewFactory);
				Assert.That(_target.CanExecute(), Is.True);
			}
		}

        [Test]
        public void ShouldGetScenarioAndPersonsFromRepositoryAndCallView()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var view = _mocks.StrictMock<IPersonSelectorView>();
				var uow = _mocks.StrictMock<IUnitOfWork>();
				var scenarioRep = _mocks.StrictMock<IScenarioRepository>();
				var scenario = _mocks.StrictMock<IScenario>();
				var guids = new HashSet<Guid>();
				var date = new DateOnly(2012, 1, 30);
				Expect.Call(_personSelectorPresenter.View).Return(view);
				Expect.Call(view.Cursor = Cursors.WaitCursor);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
				Expect.Call(_repositoryFactory.CreateScenarioRepository(uow)).Return(scenarioRep);
				Expect.Call(scenarioRep.LoadDefaultScenario()).Return(scenario);
				Expect.Call(_personSelectorPresenter.SelectedPersonGuids).Return(guids);
				Expect.Call(_personSelectorPresenter.SelectedDate).Return(date).Repeat.Twice();
				Expect.Call(uow.Dispose);
				Expect.Call(
					() => _meetingOverviewFactory.Create(guids, new DateOnlyPeriod(date, date), scenario));
				Expect.Call(view.Cursor = Cursors.Default);
				_mocks.ReplayAll();
				_target.Execute();
				_mocks.VerifyAll();
			}
		}

        [Test]
        public void ShouldCallViewToShowErrorOnDataSourceError()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var view = _mocks.StrictMock<IPersonSelectorView>();
				var exception = new DataSourceException();
				Expect.Call(_personSelectorPresenter.View).Return(view);
				Expect.Call(view.Cursor = Cursors.WaitCursor);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Throw(exception);
				Expect.Call(() => view.ShowDataSourceException(exception, Resources.MeetingOverview));
				Expect.Call(view.Cursor = Cursors.Default);
				_mocks.ReplayAll();
				_target.Execute();
				_mocks.VerifyAll();
			}
		}
    }

}