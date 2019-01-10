using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;


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
	    private IIntraIntervalFinderService _intraIntervalFinderService;
	    private ISkillPriorityProvider _skillPriorityProvider;
		private IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;

		[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
			_staffingCalculatorServiceFacade = new StaffingCalculatorServiceFacade();
			_settingDataRepository = _mocks.StrictMock<ISettingDataRepository>();
            _activityRepository = _mocks.StrictMock<IActivityRepository>();
            _personRepository = _mocks.StrictMock<IPersonRepository>();
            _currentUnitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
	        _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _model = _mocks.StrictMock<IMeetingOverviewViewModel>();
            _canModifyMeeting = _mocks.StrictMock<ICanModifyMeeting>();
			_skillPriorityProvider = new SkillPriorityProvider();
	        _intraIntervalFinderService = _mocks.StrictMock<IIntraIntervalFinderService>();
            _target = new AddMeetingCommand(_view, _settingDataRepository, _activityRepository, _personRepository,
                _currentUnitOfWorkFactory, _model, _canModifyMeeting, _intraIntervalFinderService, _skillPriorityProvider, _staffingCalculatorServiceFacade);
        
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
            Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
	        Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork());
            Expect.Call(_activityRepository.LoadAllSortByName()).Return(new List<IActivity> {activity});
            Expect.Call(_personRepository.Get(Guid.Empty)).Return(SetupFixtureForAssembly.loggedOnPerson);
            Expect.Call(_settingDataRepository.FindValueByKey("CommonNameDescription",
                                                              new CommonNameDescriptionSetting())).Return(
                                                                  new CommonNameDescriptionSetting()).IgnoreArguments();
            Expect.Call(_view.SelectedPeriod()).Return(new DateTimePeriod(2011,3,25,2011,3,25)).Repeat.Twice();
            Expect.Call(_canModifyMeeting.CanExecute).Return(true);
            Expect.Call(() => _view.EditMeeting(meetingViewModel, _intraIntervalFinderService, null, null)).IgnoreArguments();
            _mocks.ReplayAll();
            _target.Execute();
            _mocks.VerifyAll();
        }
    }

}