using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Meetings.Commands;
using Teleopti.Ccc.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.WinCode.Meetings.Overview;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Meetings.Commands
{
    [TestFixture]
    public class AddMeetingCommandTest
    {
        private MockRepository _mocks;
        private IAddMeetingCommand _target;
        private IMeetingOverviewView _view;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IUnitOfWorkFactory _unitOfWorkFactory;
        private IActivityRepository _activityRepository;
        private ISettingDataRepository _settingDataRepository;
        private IMeetingOverviewViewModel _model;
        private ICanModifyMeeting _canModifyMeeting;
        private IPersonRepository _personRepository;
        private IToggleManager _toggleManager;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
            _settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();
            _activityRepository = _mocks.StrictMock<IActivityRepository>();
            _personRepository = _mocks.StrictMock<IPersonRepository>();
            _currentUnitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
	        _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _model = _mocks.StrictMock<IMeetingOverviewViewModel>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
            _toggleManager = _mocks.StrictMock<IToggleManager>();
            _target = new AddMeetingCommand(_view, _settingDataRepository, _activityRepository, _personRepository,
                _currentUnitOfWorkFactory, _model, _canModifyMeeting, _toggleManager);
        
        }

        [Test]
        public void CouldNotExecuteIfNoPeriodIsSelected()
        {
            Expect.Call(_view.SelectedPeriod()).Return(new DateTimePeriod());
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateNewMeetingModelAndCallView()
        {
            var scenario = _mocks.StrictMock<IScenario>();
            var meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
            var activity = new Activity("akta dej");
            Expect.Call(_model.CurrentScenario).Return(scenario);
            Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
	        Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork());
            Expect.Call(_activityRepository.LoadAllSortByName()).Return(new List<IActivity> {activity});
            Expect.Call(_personRepository.Get(Guid.Empty)).Return(((IUnsafePerson) TeleoptiPrincipal.Current).Person);
            Expect.Call(_settingDataRepository.FindValueByKey("CommonNameDescription",
                                                              new CommonNameDescriptionSetting())).Return(
                                                                  new CommonNameDescriptionSetting()).IgnoreArguments();
            Expect.Call(_view.SelectedPeriod()).Return(new DateTimePeriod(2011,3,25,2011,3,25)).Repeat.Twice();
            Expect.Call(_canModifyMeeting.CanExecute).Return(true);
            Expect.Call(() => _view.EditMeeting(meetingViewModel, _toggleManager)).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

}