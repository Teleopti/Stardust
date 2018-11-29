using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture, SetCulture("sv-SE"), SetUICulture("en-US")]
	[DomainTest]
    public class SchedulePartStringVisualizerTest
    {
        private IPerson _agent;
        private IScenario _scenario;
        
        private ScheduleParameters param;
        private ScheduleRange scheduleRange;
        private IPersonAssignment ass1;

        private IMeeting meeting1;
        private IMeeting meeting2;
        private IScheduleDictionary dic;

        private IDictionary<IPerson, IScheduleRange> underlyingDictionary;

        private void setup()
        {
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            IPerson person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

            _agent = PersonFactory.CreatePersonWithPersonPeriod(person, new DateOnly(1999, 1, 1), new List<ISkill>(), new Contract("ctr"), new PartTimePercentage("ptc"));
            underlyingDictionary = new Dictionary<IPerson, IScheduleRange>();
            dic = new ScheduleDictionaryForTest(_scenario,
                                                new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1, 2010, 1, 1)),
                                                underlyingDictionary);
            param = new ScheduleParameters(_scenario, _agent,
                                          new DateTimePeriod(2000, 1, 1, 2010, 1, 1));
            scheduleRange = new ScheduleRange(dic, param, new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());

            ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShiftAndPersonalShift(_agent,
                             _scenario, ActivityFactory.CreateActivity("PersonalShiftActivity"), TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2001, 1, 1, 0, 0, 0), new DateTime(2001, 1, 2, 0, 0, 0), TimeZoneInfoFactory.StockholmTimeZoneInfo()), ShiftCategoryFactory.CreateShiftCategory("Morgon"));

            meeting1 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting1", "location1", "description1", ActivityFactory.CreateActivity("activity1"), _scenario);
            meeting1.StartDate = new DateOnly(2001, 1, 1);
            meeting1.EndDate = new DateOnly(2001, 1, 2);
            meeting1.StartTime = TimeSpan.FromHours(8);
            meeting1.EndTime = TimeSpan.FromHours(9);
            meeting2 = new Meeting(_agent, new List<IMeetingPerson>(), "meeting2", "location2", "description2", ActivityFactory.CreateActivity("activity2"), _scenario);
            meeting2.StartDate = new DateOnly(2001, 1, 1);
            meeting2.EndDate = new DateOnly(2001, 1, 2);
            meeting2.StartTime = TimeSpan.FromHours(8);
            meeting2.EndTime = TimeSpan.FromHours(9);

            IMeetingPerson meetingPersonRequired = new MeetingPerson(_agent, false);
            IMeetingPerson meetingPersonOptional = new MeetingPerson(_agent, true);

            meeting1.AddMeetingPerson(meetingPersonRequired);
            meeting2.AddMeetingPerson(meetingPersonOptional);

            //add to schedule
            scheduleRange.Add(ass1);
            scheduleRange.AddRange(meeting1.GetPersonMeetings(dic.Period.LoadedPeriod(), _agent));
            scheduleRange.AddRange(meeting2.GetPersonMeetings(dic.Period.LoadedPeriod(), _agent));
        }

        [Test]
        public void VerifyToolTipPersonalAssignments()
        {
	        setup();
			string meetingPeriod1 = DateTime.MinValue.Add(meeting1.StartTime).ToShortTimeString() +
                                    " - " + DateTime.MinValue.Add(meeting1.EndTime).ToShortTimeString();

            string meetingPeriod2 = DateTime.MinValue.Add(meeting2.StartTime).ToShortTimeString() +
                                    " - " + DateTime.MinValue.Add(meeting2.EndTime).ToShortTimeString();
            string expected = " - Personal activity" + ": \r\n    " + "PersonalShiftActivity: " + ass1.Period.StartDateTimeLocal(TimeZoneInfoFactory.StockholmTimeZoneInfo()).ToShortTimeString() + " - " + ass1.Period.EndDateTimeLocal(TimeZoneInfoFactory.StockholmTimeZoneInfo()).ToShortTimeString() +
								"\r\n" + " - Meeting: \r\n    " + "meeting1" + ": " + meetingPeriod1 +
                               "\r\n" + " - Meeting: \r\n    " + "meeting2" + ": " + meetingPeriod2 + " (" + UserTexts.Resources.Optional + ")";

            underlyingDictionary.Clear();
            underlyingDictionary.Add(scheduleRange.Person, scheduleRange);

            Assert.AreEqual(expected, ScheduleDayStringVisualizer.GetToolTipPersonalAssignments(scheduleRange.ScheduledDay(new DateOnly(2001,1,1)) , _agent.PermissionInformation.DefaultTimeZone(), new CultureInfo("sv-SE", false)));
        }

    }
}
