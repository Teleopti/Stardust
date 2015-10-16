using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Meetings.Commands;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class AddMeetingFromPanelCommandTest
    {
        private MockRepository _mocks;
        private AddMeetingFromPanelCommand _target;
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
            _target = new AddMeetingFromPanelCommand(_repositoryFactory, _unitOfWorkFactory, _personSelectorPresenter, _meetingOverviewFactory);
        }

        [Test]
        public void ShouldReturnFalseIfNotAllowed()
        {
            using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
            {
                _target = new AddMeetingFromPanelCommand(_repositoryFactory, _unitOfWorkFactory, _personSelectorPresenter, _meetingOverviewFactory);
                Assert.That(_target.CanExecute(), Is.False);
            }
        }

        [Test]
        public void ShouldReturnTrueIfAllowed()
        {
            Assert.That(_target.CanExecute(), Is.True);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldGetScenarioAndPersonsFromRepositoryAndCallView()
        {
            var view = _mocks.StrictMock<IPersonSelectorView>();
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var scenarioRep = _mocks.StrictMock<IScenarioRepository>();
            var personsRep = _mocks.StrictMock<IPersonRepository>();
            var activitiesRep = _mocks.StrictMock<IActivityRepository>();
            var scenario = _mocks.StrictMock<IScenario>();
            var settingsRep = _mocks.StrictMock<ISettingDataRepository>();
            var cmnName = new CommonNameDescriptionSetting();
            var guids = new HashSet<Guid>();
            ICollection<IPerson> persons = new List<IPerson>();
            Expect.Call(_personSelectorPresenter.View).Return(view);
            Expect.Call(view.Cursor = Cursors.WaitCursor);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            
            Expect.Call(_repositoryFactory.CreateActivityRepository(uow)).Return(activitiesRep);
            Expect.Call(activitiesRep.LoadAllSortByName()).Return(new List<IActivity> {new Activity("act")});

            Expect.Call(_repositoryFactory.CreateScenarioRepository(uow)).Return(scenarioRep);
            Expect.Call(scenarioRep.LoadDefaultScenario()).Return(scenario);

            Expect.Call(_repositoryFactory.CreatePersonRepository(uow)).Return(personsRep);
            Expect.Call(personsRep.Get(Guid.Empty)).Return(new Person());
            Expect.Call(_repositoryFactory.CreateGlobalSettingDataRepository(uow)).Return(settingsRep);
            Expect.Call(settingsRep.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting())).Return(cmnName).IgnoreArguments();

            Expect.Call(_personSelectorPresenter.SelectedPersonGuids).Return(guids);
            Expect.Call(personsRep.FindPeople(guids)).Return(persons);
            
            Expect.Call(uow.Dispose);
            Expect.Call(
                () => _meetingOverviewFactory.ShowMeetingComposerView(view, null, true)).IgnoreArguments();
            Expect.Call(view.Cursor = Cursors.Default);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldCallViewToShowErrorOnDataSourceError()
        {
            var view = _mocks.StrictMock<IPersonSelectorView>();
            var exception = new DataSourceException();
            Expect.Call(_personSelectorPresenter.View).Return(view);
            Expect.Call(view.Cursor = Cursors.WaitCursor);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Throw(exception);
            Expect.Call(() => view.ShowDataSourceException(exception, Resources.AddMeeting));
            Expect.Call(view.Cursor = Cursors.Default);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldNotShowDialogWhenNoActivities()
        {
            var view = _mocks.StrictMock<IPersonSelectorView>();
            var uow = _mocks.StrictMock<IUnitOfWork>();
            var activitiesRep = _mocks.StrictMock<IActivityRepository>();
            Expect.Call(_personSelectorPresenter.View).Return(view);
            Expect.Call(view.Cursor = Cursors.WaitCursor);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
            Expect.Call(_repositoryFactory.CreateActivityRepository(uow)).Return(activitiesRep);
            Expect.Call(activitiesRep.LoadAllSortByName()).Return(new List<IActivity>());
            Expect.Call(uow.Dispose);
            Expect.Call(view.Cursor = Cursors.Default);
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

}