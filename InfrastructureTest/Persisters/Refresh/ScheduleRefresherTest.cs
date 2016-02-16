using System;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

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
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(personAssignment),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
				);
			var scheduleDictionary = new ScheduleDictionaryForTest(personAssignment.Scenario, period);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, messages, null, null, _ => true);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.EqualTo(personAssignment);
		}


		[Test]
		public void ShouldKeepPreviousAbsencesInDictionaryWhenInsertingNewAbsence()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var scenario = personAssignment.Scenario;

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 9, 4, 10, 2013, 9, 4, 11));
			personAbsence.SetId(Guid.NewGuid());
			var personAbsenceRepository = new FakePersonAbsenceRepository();
			personAbsenceRepository.Add(personAbsence);

			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(personAssignment),
				personAbsenceRepository,
				MockRepository.GenerateMock<IMessageQueueRemoval>()
			);

			var scheduleDictionary = new ScheduleDictionaryForTest(scenario, period);
			scheduleDictionary.AddPersonAbsence(personAbsence);

			var newPersonAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 9, 4, 13, 2013, 9, 4, 14));
			newPersonAbsence.SetId(Guid.NewGuid());
			personAbsenceRepository.Add(newPersonAbsence);

			var newAbsenceMessage = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = newPersonAbsence.Period.StartDateTime, 
					EventEndDate = newPersonAbsence.Period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, newAbsenceMessage, null, null, _ => true);

			var scheduleDay = scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4));
			var personAbsences = scheduleDay.PersonAbsenceCollection();
			
			personAbsences.Count.Should().Be(2);
			personAbsences[0].Should().Be.EqualTo(newPersonAbsence);
			personAbsences[1].Should().Be.EqualTo(personAbsence);
		}



		[Test]
		public void ShouldNotRaiseConflictWhenSameVersionInMemoryAsFromMessage()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var updateScheduleDataFromMessages = MockRepository.GenerateMock<IUpdateScheduleDataFromMessages>();
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				updateScheduleDataFromMessages,
				new FakePersonAssignmentRepository(personAssignment),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
				);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = new ScheduleRange(scheduleDictionary, new ScheduleParameters(personAssignment.Scenario, person, period));
			scheduleRange.Add(personAssignment);

			scheduleDictionary.Stub(x => x.Scenario).Return(personAssignment.Scenario);
			scheduleDictionary.Stub(x => x[person]).Return(scheduleRange);
			scheduleDictionary.Stub(x => x.DifferenceSinceSnapshot())
							  .Return(new DifferenceCollection<IPersistableScheduleData>
				                  {
					                  new DifferenceCollectionItem<IPersistableScheduleData>(personAssignment, personAssignment)
				                  });

			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			var conflictsBuffer = new Collection<PersistConflict>();

			target.Refresh(scheduleDictionary, messages, null, conflictsBuffer, _ => true);

			conflictsBuffer.Should().Be.Empty();
			updateScheduleDataFromMessages.AssertWasNotCalled(x => x.FillReloadedScheduleData(personAssignment));
		}

		[Test]
		public void ShouldDeleteAssignmentFromDictionary()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
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

			target.Refresh(scheduleDictionary, messages, null, null, _ => true);

			scheduleDictionary[person].ScheduledDay(new DateOnly(2013, 9, 4)).PersonAssignment().Should().Be.Null();
		}

		[Test]
		public void ShouldIgnorePersonAssignmentUpdateWhenPersonNotRelevant()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(personAssignment),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				MockRepository.GenerateMock<IMessageQueueRemoval>()
				);
			var scheduleDictionary = new ScheduleDictionaryForTest(personAssignment.Scenario, period);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, messages, null, null, _ => false);

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
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(),
				MockRepository.GenerateMock<IPersonAbsenceRepository>(),
				remover
				);
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(personAssignment.Scenario, period, personAssignment);
			var messages = new[] {new EventMessage
				{
					InterfaceType = typeof (IScheduleChangedEvent), 
					DomainObjectId = person.Id.GetValueOrDefault(), 
					EventStartDate = period.StartDateTime, 
					EventEndDate = period.EndDateTime
				}};

			target.Refresh(scheduleDictionary, messages, null, null, _ => true);
			remover.AssertWasCalled(x => x.Remove(messages.Single()));
		}
	}
}
