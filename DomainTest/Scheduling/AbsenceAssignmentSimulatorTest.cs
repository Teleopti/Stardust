using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class AbsenceAssignmentSimulatorTest
    {
        private AbsenceAssignmentSimulator _target;
        private MockRepository _mocks;
        private IPerson _person;
        private DateTimePeriod _absencePeriod;
        private IAbsenceRequest _absenceRequest;
        private IScheduleDictionary _scheduleDictionary;
        private IPersonRequest _personRequest;
        private IScheduleDayChangeCallback _scheduleDayChangeCallback;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
            _scheduleDayChangeCallback = _mocks.StrictMock<IScheduleDayChangeCallback>();

            CreateAbsenceRequest();

            _target = new AbsenceAssignmentSimulator(_scheduleDictionary,_scheduleDayChangeCallback);
        }

        [Test]
        public void VerifyAssignAbsence()
        {
            
            IScheduleRange scheduleRange = _mocks.StrictMock<IScheduleRange>();
            IScheduleDay schedulePart = _mocks.StrictMock<IScheduleDay>();

            using (_mocks.Record())
            {
                DateOnlyPeriod tempPeriod =
                    _absencePeriod.ToDateOnlyPeriod(_person.PermissionInformation.DefaultTimeZone());

                Expect.Call(_scheduleDictionary[_person]).Return(scheduleRange).Repeat.Once();
                Expect.Call(scheduleRange.Person).Return(_person).Repeat.Once();
                Expect.Call(schedulePart.Period).Return(_absencePeriod).Repeat.Once();
                Expect.Call(scheduleRange.ScheduledDay(tempPeriod.StartDate)).Return(schedulePart).Repeat.Once();
                schedulePart.CreateAndAddAbsence(null);
                LastCall.IgnoreArguments();

                Expect.Call(_scheduleDictionary.Modify(ScheduleModifier.Request, new List<IScheduleDay> { schedulePart },
                    null, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance))).IgnoreArguments().Return(
                    new List<IBusinessRuleResponse>()).Repeat.Once();
            }

            using (_mocks.Playback())
            {
                _target.AssignAbsence(_absenceRequest);
            }
        }

        private void CreateAbsenceRequest()
        {
            // Create person
            _person = PersonFactory.CreatePerson();
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _person.PermissionInformation.SetDefaultTimeZone((timeZone));

            IAbsence absence = new Absence
            {
                Description = new Description("time tracker absence 2"),
                InContractTime = true,
                InPaidTime = true,
                InWorkTime = true,
                Requestable = true,
                Tracker = Tracker.CreateDayTracker()
            };
            _absencePeriod = new DateTimePeriod(new DateTime(2010, 3, 1, 23, 0, 0, DateTimeKind.Utc),
                                                               new DateTime(2010, 3, 2, 23, 0, 0, DateTimeKind.Utc));
            _absenceRequest = new AbsenceRequest(absence,_absencePeriod);
            _personRequest = new PersonRequest(_person, _absenceRequest);
            Assert.IsNotNull(_personRequest);
        }
    }
}
