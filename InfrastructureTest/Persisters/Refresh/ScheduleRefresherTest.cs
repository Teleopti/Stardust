using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Refresh;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Refresh
{
	[TestFixture]
	public class ScheduleRefresherTest
	{
		private class noMessageQueueRemoval : IMessageQueueRemoval
		{
			public readonly List<IEventMessage> RemovedMessages = new List<IEventMessage>();
			public void Remove(IEventMessage eventMessage)
			{
				RemovedMessages.Add(eventMessage);
			}

			public void Remove(PersistConflict persistConflict)
			{
			}
		}

		[Test]
		public void ShouldUpdatePersonAssignmentsInDictionary()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var personAssignmentRepository = new FakePersonAssignmentRepository(null);
			personAssignmentRepository.Has(personAssignment);
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				personAssignmentRepository,
				new FakePersonAbsenceRepository(null),
				new noMessageQueueRemoval()
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

			var personAbsence = PersonAbsenceFactory.CreatePersonAbsence(person, scenario, new DateTimePeriod(2013, 9, 4, 10, 2013, 9, 4, 11)).WithId();
			var personAbsenceRepository = new FakePersonAbsenceRepository(null);
			personAbsenceRepository.Add(personAbsence);
			var personAssignmentRepository = new FakePersonAssignmentRepository(null);
			personAssignmentRepository.Has(personAssignment);
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				personAssignmentRepository,
				personAbsenceRepository,
				new noMessageQueueRemoval()
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
			
			personAbsences.Length.Should().Be(2);
			personAbsences.Contains(personAbsence).Should().Be.True();
			personAbsences.Contains(newPersonAbsence).Should().Be.True();
		}



		[Test]
		public void ShouldNotRaiseConflictWhenSameVersionInMemoryAsFromMessage()
		{
			var period = new DateTimePeriod(2013, 9, 4, 2013, 9, 5);
			var person = PersonFactory.CreatePersonWithId();
			var personAssignment = PersonAssignmentFactory.CreatePersonAssignmentWithId(person, new DateOnly(2013, 9, 4));
			var updateScheduleDataFromMessages = MockRepository.GenerateMock<IUpdateScheduleDataFromMessages>();
			var personAssignmentRepository = new FakePersonAssignmentRepository(null);
			personAssignmentRepository.Has(personAssignment);
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				updateScheduleDataFromMessages,
				personAssignmentRepository,
				new FakePersonAbsenceRepository(null),
				new noMessageQueueRemoval()
				);
			var scheduleDictionary = MockRepository.GenerateMock<IScheduleDictionary>();
			var scheduleRange = new ScheduleRange(scheduleDictionary, new ScheduleParameters(personAssignment.Scenario, person, period), new PersistableScheduleDataPermissionChecker(new FullPermission()), new FullPermission());
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
				new FakePersonAssignmentRepository(null),
				new FakePersonAbsenceRepository(null),
				new noMessageQueueRemoval()
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
			var personAssignmentRepository = new FakePersonAssignmentRepository(null);
			personAssignmentRepository.Has(personAssignment);
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				personAssignmentRepository,
				new FakePersonAbsenceRepository(null),
				new noMessageQueueRemoval()
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
			var remover = new noMessageQueueRemoval();
			var target = new ScheduleRefresher(
				new FakePersonRepositoryLegacy(person),
				null,
				new FakePersonAssignmentRepository(null),
				new FakePersonAbsenceRepository(null),
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
			remover.RemovedMessages.Should().Contain(messages.Single());
		}
	}
}
