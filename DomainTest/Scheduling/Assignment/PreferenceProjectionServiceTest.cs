using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.DomainTest.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class PreferenceProjectionServiceTest
    {
        private IActivity activity;
        private IPerson person;

        [SetUp]
        public void Setup()
        {
            person = new Person();
            activity = new Activity("for test");
        }

        [Test]
        public void VerifyPersonalShiftAndMeetingAreAddedWithoutMainShift()
        {
            IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(activity,
                                                                               person,
                                                                               createPeriod(10, 11),
                                                                               ScenarioFactory.CreateScenarioAggregate());
            IPersonMeeting personMeeting = CreatePersonMeeting(createPeriod(13, 14));

            PreferenceProjectionService target = new PreferenceProjectionService(person, createPeriod(0, 24));
            target.Add(personAssignment);
            target.Add(personMeeting);

            IVisualLayerCollection projection = target.CreateProjection();
            Assert.AreEqual(2, projection.Count());
            IList<IVisualLayer> list = new List<IVisualLayer>(projection);
            Assert.AreEqual(createPeriod(10,11), list[0].Period);
            Assert.AreEqual(createPeriod(13,14), list[1].Period);
        }

        [Test]
        public void VerifyPersonalShiftAndMeetingAreNotAddedWithMainShift()
        {
            IPersonAssignment personAssignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(activity,
                                                                               person,
                                                                               createPeriod(10, 11),
                                                                               ScenarioFactory.CreateScenarioAggregate());
            IPersonAssignment personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person,
                                                                                                        createPeriod(1,2),
                                                                                                        new ShiftCategory("d"),
                                                                                                        new Scenario("d"));
            IPersonMeeting personMeeting = CreatePersonMeeting(createPeriod(13, 14));

            PreferenceProjectionService target = new PreferenceProjectionService(person, createPeriod(0, 24));
            target.Add(personAssignment);
            target.Add(personMeeting);
            target.Add(personAssignment2);

            IVisualLayerCollection projection = target.CreateProjection();
            Assert.AreEqual(1, projection.Count());
            Assert.AreEqual(createPeriod(1, 2), projection.First().Period);
        }

        private static DateTimePeriod createPeriod(int startHour, int endHour)
        {
            DateTime date = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime start = date.AddHours(startHour);
            DateTime end = date.AddHours(endHour);
            return new DateTimePeriod(start, end);
        }

        private IPersonMeeting CreatePersonMeeting(DateTimePeriod period)
        {
            IMeeting mainMeeting = new Meeting(PersonFactory.CreatePerson(), new List<IMeetingPerson>(), "subject", "location", "description",
                    ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());

            IPersonMeeting personMeeting = new PersonMeeting(mainMeeting, new MeetingPerson(person, true), period);

            personMeeting.BelongsToMeeting.AddMeetingPerson(new MeetingPerson(person, true));

            return personMeeting;
        }
    }
}
