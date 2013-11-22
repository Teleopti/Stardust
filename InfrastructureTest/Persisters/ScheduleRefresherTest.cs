﻿using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters
{
	[TestFixture]
	public class ScheduleRefresherTest
	{
		[Test]
		public void ShouldUpdatePersonAssignmentsInDictionary()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var target = new ScheduleRefresher(
				new FakePersonRepository(person), 
				null,
				new FakePersonAssignmentRepository(personAssignment), 
				MockRepository.GenerateMock<IPersonAbsenceRepository>()
				);
			var scheduleDictionary = new ScheduleDictionaryForTest(personAssignment.Scenario, period);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, null, messages, null, null, _ => true);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.EqualTo(personAssignment);
		}

		[Test]
		public void ShouldDeleteAssignmentFromDictionary()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var target = new ScheduleRefresher(
				new FakePersonRepository(person),
				null,
				new FakePersonAssignmentRepository(),
				MockRepository.GenerateMock<IPersonAbsenceRepository>()
				);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(personAssignment.Scenario, period, personAssignment);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};
			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.EqualTo(personAssignment);

			target.Refresh(scheduleDictionary, null, messages, null, null, _ => true);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.Null();
		}

		[Test]
		public void ShouldIgnorePersonAssignmentUpdateWhenPersonNotRelevant()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var target = new ScheduleRefresher(
				new FakePersonRepository(person),
				null,
				new FakePersonAssignmentRepository(personAssignment),
				MockRepository.GenerateMock<IPersonAbsenceRepository>()
				);
			var scheduleDictionary = new ScheduleDictionaryForTest(personAssignment.Scenario, period);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, null, messages, null, null, _ => false);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.Null();
		}
	}
}
