using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDic =  _mocks.StrictMock<IScheduleDictionary>();
            _stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _resourceOpt = _mocks.StrictMock<IResourceOptimizationHelper>();
            _meeting = _mocks.StrictMock<IMeeting>();
            _target = new MeetingImpactCalculator(_stateHolder, _resourceOpt, _meeting);
        }

        [Test]
        public void ShouldRemoveAddAndRecalculateDay()
        {
            var meetingDate = new DateOnly(2011, 2, 7);
            var person1 = _mocks.StrictMock<IPerson>();
            var meetingPerson1 = _mocks.StrictMock<IMeetingPerson>();
            var meetingPerson2 = _mocks.StrictMock<IMeetingPerson>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var personMeeting = _mocks.StrictMock<IPersonMeeting>();
            _parameters = _mocks.StrictMock<IScheduleParameters>();
            var range = new ScheduleExposingAddRemove(_scheduleDic, _parameters);
            //var range = _mocks.DynamicMock<ScheduleRange>(_scheduleDic, _parameters, _authService);

            //remove
            Expect.Call(_meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson1, meetingPerson2 }));
            Expect.Call(_meeting.StartDate).Return(meetingDate).Repeat.Times(2);
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDic).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDic.SchedulesForDay(meetingDate)).Return(new List<IScheduleDay> { scheduleDay }).Repeat.Times(2);
            Expect.Call(scheduleDay.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting })).Repeat.Times(2);
            Expect.Call(personMeeting.BelongsToMeeting).Return(_meeting).Repeat.Times(2);
            Expect.Call(meetingPerson1.Person).Return(person1);
            Expect.Call(meetingPerson2.Person).Return(person1);
            Expect.Call(_scheduleDic[person1]).Return(range).Repeat.Times(2);
            //Expect.Call(() => range.Remove(personMeeting)).Repeat.Twice();

            //add
            Expect.Call(_meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson1, meetingPerson2 }));
            Expect.Call(meetingPerson1.Person).Return(person1);
            Expect.Call(meetingPerson2.Person).Return(person1);
            Expect.Call(_meeting.GetPersonMeetings(person1)).Return(new List<IPersonMeeting> { personMeeting }).Repeat.Twice();
            Expect.Call(_scheduleDic[person1]).Return(range).Repeat.Times(2);
            //Expect.Call(() => range.Add(personMeeting)).Repeat.Twice();

            Expect.Call(() => _resourceOpt.ResourceCalculateDate(meetingDate, false, true));
            
            _mocks.ReplayAll();
            _target.RecalculateResources(meetingDate);
            _mocks.VerifyAll();

        }

        [Test]
        public void ShouldRemoveAndRecalculateDay()
        {
            var meetingDate = new DateOnly(2011, 2, 7);
            var person1 = _mocks.StrictMock<IPerson>();
            var meetingPerson1 = _mocks.StrictMock<IMeetingPerson>();
            var meetingPerson2 = _mocks.StrictMock<IMeetingPerson>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var personMeeting = _mocks.StrictMock<IPersonMeeting>();
            _parameters = _mocks.StrictMock<IScheduleParameters>();
            var range = new ScheduleExposingAddRemove(_scheduleDic, _parameters);

            Expect.Call(_meeting.MeetingPersons).Return(
                new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson> { meetingPerson1, meetingPerson2 }));
            Expect.Call(_stateHolder.Schedules).Return(_scheduleDic).Repeat.AtLeastOnce();
            Expect.Call(_meeting.StartDate).Return(meetingDate).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDic.SchedulesForDay(meetingDate)).Return(new List<IScheduleDay> { scheduleDay }).Repeat.AtLeastOnce();
            Expect.Call(scheduleDay.PersonMeetingCollection()).Return(
                new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting> { personMeeting })).Repeat.AtLeastOnce();
            Expect.Call(personMeeting.BelongsToMeeting).Return(_meeting).Repeat.AtLeastOnce();
            Expect.Call(meetingPerson1.Person).Return(person1);
            Expect.Call(meetingPerson2.Person).Return(person1);
            Expect.Call(_scheduleDic[person1]).Return(range).Repeat.AtLeastOnce();
            Expect.Call(() => range.Remove(personMeeting)).Repeat.AtLeastOnce();
            
            Expect.Call(() => _resourceOpt.ResourceCalculateDate(meetingDate, false, true));

            _mocks.ReplayAll();
            _target.RemoveAndRecalculateResources(_meeting, meetingDate);
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
                return;
            }

            public override void Remove(IScheduleData scheduleData)
            {
                return;
            }
        }
    }

    
}