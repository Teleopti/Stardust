using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Overview;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Meetings.Overview
{
    [TestFixture]
    public class MeetingChangerAndPersisterTest
    {
        private MockRepository _mocks;
        private IMeetingRepository _meetingRepository;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private MeetingChangerAndPersister _target;
        private IMeetingOverviewViewModel _model;
        private IScenario _scenario;
        private TimeZoneInfo _timeZone;
        private IMeetingOverviewView _view;
        private IMeetingParticipantPermittedChecker _meetingParticipantPermittedChecker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _meetingRepository = _mocks.StrictMock<IMeetingRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _view = _mocks.StrictMock<IMeetingOverviewView>();
            _meetingParticipantPermittedChecker = _mocks.StrictMock<IMeetingParticipantPermittedChecker>();
			_scenario = _mocks.StrictMock<IScenario>();
			_model = new MeetingOverviewViewModel { CurrentScenario = _scenario }; //_mocks.StrictMock<IMeetingOverviewViewModel>();
            _target = new MeetingChangerAndPersister(_meetingRepository, _unitOfWorkFactory, _model, _meetingParticipantPermittedChecker);

            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"));   
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOnlyChangeEndTimeWhenDurationChangeOnMeeting()
		{
			var meeting = _mocks.DynamicMock<IMeeting>();
			var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
			var period = new DateOnlyPeriod(2011, 4, 3, 2011, 4, 3);
			_model.SelectedPeriod = period;
			var bu = _mocks.StrictMock<IBusinessUnit>();

			Expect.Call(meeting.MeetingDuration()).Return(TimeSpan.FromHours(2));
			Expect.Call(meeting.StartTime).Return(TimeSpan.FromHours(12));
			Expect.Call(() => meeting.EndTime = TimeSpan.FromHours(15));

			Expect.Call(meeting.Id).Return(Guid.Empty);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			Expect.Call(meeting.MeetingPersons).Return(new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson>{new MeetingPerson(new Person(), false)}));
			Expect.Call(() => unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();

			Expect.Call(meeting.GetOrFillWithBusinessUnit_DONTUSE()).Return(bu);
			Expect.Call(() => unitOfWork.Reassociate(bu));
			//Expect.Call(_model.SelectedPeriod).Return(period);
			Expect.Call(_meetingParticipantPermittedChecker.ValidatePermittedPersons(new List<IPerson>(),
			                                                                         new DateOnly(2011, 4, 3), _view,
			                                                                         null)).IgnoreArguments().Return(true);
			Expect.Call(_meetingRepository.Load(Guid.Empty)).Return(meeting);
			Expect.Call(() => unitOfWork.Merge(meeting));
			Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
			Expect.Call(unitOfWork.Dispose);

			_mocks.ReplayAll();
			_target.ChangeDurationAndPersist(meeting, TimeSpan.FromHours(1), _view);
			_mocks.VerifyAll();
		}

    	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldChangeStartAndUseDurationChangeWhenSavingStartTime()
        {
            var userStartDateTime = new DateTime(2011, 3, 30, 9, 0, 0);
            var meeting = _mocks.DynamicMock<IMeeting>();
            var unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            var userTimeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
            var period = new DateOnlyPeriod(2011, 4, 3, 2011, 4, 3);
    		_model.SelectedPeriod = period;
            var bu = _mocks.StrictMock<IBusinessUnit>(); 

            Expect.Call(meeting.TimeZone).Return(_timeZone);
            Expect.Call(meeting.MeetingDuration()).Return(TimeSpan.FromHours(2));

            Expect.Call(meeting.GetRecurringDates()).Return(new List<DateOnly> {new DateOnly()});
            Expect.Call(() => meeting.StartDate = new DateOnly(2011, 3, 30));
            Expect.Call(() => meeting.EndDate = new DateOnly(2011, 3, 30));
            // should add a hour to finland
            Expect.Call(() => meeting.StartTime = TimeSpan.FromHours(10));
            Expect.Call(() => meeting.EndTime = TimeSpan.FromHours(13));
            //Expect.Call(_model.CurrentScenario).Return(_scenario);

            Expect.Call(meeting.Id).Return(null);
            Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(meeting.MeetingPersons).Return(new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson>()));
            Expect.Call(() => unitOfWork.Reassociate(new List<IPerson>())).IgnoreArguments();
            Expect.Call(meeting.GetOrFillWithBusinessUnit_DONTUSE()).Return(bu);
            Expect.Call(() => unitOfWork.Reassociate(bu));
            //Expect.Call(_model.SelectedPeriod).Return(period);
            Expect.Call(_meetingParticipantPermittedChecker.ValidatePermittedPersons(new List<IPerson>(),
                                                                                    new DateOnly(2011, 4, 3), _view,
                                                                                    null)).IgnoreArguments().Return(true);
			//Expect.Call(_meetingRepository.Load(Guid.Empty)).Return(meeting);
			//Expect.Call(() => unitOfWork.Merge(meeting));
			Expect.Call(() =>_meetingRepository.Add(meeting));
            Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            Expect.Call(unitOfWork.Dispose);

            _mocks.ReplayAll();
            _target.ChangeStartDateTimeAndPersist(meeting, userStartDateTime, TimeSpan.FromHours(1), userTimeZone, _view);
            _mocks.VerifyAll();
        }
    }
}