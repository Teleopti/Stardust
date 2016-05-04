using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Meetings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingImpactCalculatorTest
    {
        private MockRepository _mocks;
        private IScheduleDictionary _scheduleDic;
        private IResourceOptimizationHelper _resourceOpt;
        private IMeeting _meeting;
        private MeetingImpactCalculator _target;
        private ISchedulerStateHolder _stateHolder;
        private IScheduleParameters _parameters;
    	private IPerson _person;
    	private DateOnly _meetingDate;
    	private DateTimePeriod _period;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        	_person = PersonFactory.CreatePerson();
			
            _meetingDate = new DateOnly(2011, 2, 7);
    		_period =
    			new DateOnlyPeriod(_meetingDate, _meetingDate).ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
            _scheduleDic =  _mocks.StrictMultiMock<IScheduleDictionary>(typeof(IPermissionCheck));
            _stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _resourceOpt = _mocks.StrictMock<IResourceOptimizationHelper>();
            _meeting = _mocks.StrictMock<IMeeting>();
            _target = new MeetingImpactCalculator(_stateHolder, _resourceOpt, _meeting);
        }

        [Test]
        public void ShouldRemoveAddAndRecalculateDay()
        {
            var meetingPerson1 = _mocks.StrictMock<IMeetingPerson>();
            var meetingPerson2 = _mocks.StrictMock<IMeetingPerson>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var personMeeting = _mocks.StrictMock<IPersonMeeting>();
        	var schedulePeriod = _mocks.StrictMock<IScheduleDateTimePeriod>();
	        _parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(), _person, _period);

            var range = new ScheduleExposingAddRemove(_scheduleDic, _parameters);

            //remove
            Expect.Call(_meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson1, meetingPerson2 })).Repeat.Twice();
            Expect.Call(_meeting.StartDate).Return(_meetingDate).Repeat.Times(2);
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDic).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDic.SchedulesForDay(_meetingDate)).Return(new List<IScheduleDay> { scheduleDay }).Repeat.Times(2);
            Expect.Call(scheduleDay.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting })).Repeat.Times(2);
            Expect.Call(personMeeting.BelongsToMeeting).Return(_meeting).Repeat.Times(2);
            Expect.Call(meetingPerson1.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(meetingPerson2.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDic[_person]).Return(range).Repeat.Times(2);
            
            //add
            Expect.Call(_meeting.GetPersonMeetings(_person)).Return(new List<IPersonMeeting> { personMeeting }).Repeat.Twice();
			Expect.Call(_scheduleDic.Period).Return(schedulePeriod);
        	Expect.Call(personMeeting.Period).Return(_period);
        	Expect.Call(personMeeting.Period).Return(_period.MovePeriod(TimeSpan.FromDays(2)));
			Expect.Call(schedulePeriod.LoadedPeriod()).Return(_period);
			Expect.Call(_scheduleDic[_person]).Return(range);

            Expect.Call(() => _resourceOpt.ResourceCalculateDate(_meetingDate, true, false));

        	var permissionCheck = (IPermissionCheck) _scheduleDic;
        	Expect.Call(permissionCheck.SynchronizationObject).Return(new object()).Repeat.AtLeastOnce();
        	Expect.Call(()=>permissionCheck.UsePermissions(true));
        	Expect.Call(()=>permissionCheck.UsePermissions(false));
            
            _mocks.ReplayAll();
            _target.RecalculateResources(_meetingDate);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveAndRecalculateDay()
        {
            var meetingPerson1 = _mocks.StrictMock<IMeetingPerson>();
            var meetingPerson2 = _mocks.StrictMock<IMeetingPerson>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var personMeeting = _mocks.StrictMock<IPersonMeeting>();
            _parameters = new ScheduleParameters(ScenarioFactory.CreateScenarioAggregate(),_person,_period);
            var range = new ScheduleExposingAddRemove(_scheduleDic, _parameters);

            Expect.Call(_meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson1, meetingPerson2 }));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDic).Repeat.AtLeastOnce();
            Expect.Call(_meeting.StartDate).Return(_meetingDate).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDic.SchedulesForDay(_meetingDate)).Return(new List<IScheduleDay> { scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting })).Repeat.AtLeastOnce();
            Expect.Call(personMeeting.BelongsToMeeting).Return(_meeting).Repeat.AtLeastOnce();
            Expect.Call(meetingPerson1.Person).Return(_person);
            Expect.Call(meetingPerson2.Person).Return(_person);
            Expect.Call(_scheduleDic[_person]).Return(range).Repeat.AtLeastOnce();
            Expect.Call(() => range.Remove(personMeeting)).Repeat.AtLeastOnce();
            
            Expect.Call(() => _resourceOpt.ResourceCalculateDate(_meetingDate, true, false));

            _mocks.ReplayAll();
            _target.RemoveAndRecalculateResources(_meeting, _meetingDate);
            _mocks.VerifyAll();

        }

        private class ScheduleExposingAddRemove : ScheduleRange
        {

            public ScheduleExposingAddRemove(IScheduleDictionary owner, IScheduleParameters parameters)
                : base(owner, parameters)
            {
            }

            public override void Add(IScheduleData scheduleData)
            {
            }

            public override void Remove(IScheduleData scheduleData)
            {
            }
        }
    }

    
}