using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Persisters.NewStuff;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
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
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
				);
			var scheduleDictionary = new ScheduleDictionaryForTest(personAssignment.Scenario, period);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.Value, 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, messages, null, null);

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
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
				);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(personAssignment.Scenario, period, personAssignment);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.Value, 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};
			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.EqualTo(personAssignment);

			target.Refresh(scheduleDictionary, messages, null, null);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.Null();
		}

		[Test]
		public void ShouldCallRemover()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var remover = MockRepository.GenerateMock<IMessageQueueRemoval>();
			var target = new ScheduleRefresher(
				new FakePersonRepository(person),
				null,
				new FakePersonAssignmentRepository(),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				remover
				);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(personAssignment.Scenario, period, personAssignment);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.Value, 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, messages, null, null);
			remover.AssertWasCalled(x => x.Remove(messages.Single()));
		}
	}
}
