using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Archiving;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Archiving
{
	[DomainTest]
	[Toggle(Toggles.Wfm_ArchiveSchedule_41498)]
	public class ArchiveScheduleHandlerTest
	{
		public IScheduleStorage ScheduleStorage;
		public ArchiveScheduleHandler Target;
		public FakePersonRepository PersonRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		public void ShouldMoveOneAssignment()
		{
			var defaultScenario = new Scenario("default") { DefaultScenario = true }.WithId();
			var trackingId = Guid.NewGuid();
			var period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 5);
			var person = new Person().WithId();
			var assignment = new PersonAssignment(person, defaultScenario, period.StartDate);
			var moveToScenario = new Scenario("target").WithId();

			ScheduleStorage.Add(assignment);
			PersonRepository.Add(person);
			ScenarioRepository.Add(defaultScenario);
			ScenarioRepository.Add(moveToScenario);

			var @event = new ArchiveScheduleEvent
			{
				PersonId = person.Id.GetValueOrDefault(),
				EndDate = period.EndDate.Date,
				FromScenario = defaultScenario.Id.GetValueOrDefault(),
				StartDate = period.StartDate.Date,
				ToScenario = moveToScenario.Id.GetValueOrDefault(),
				TrackingId = trackingId
			};
			Target.Handle(@event);

			var result = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(true, true), period, moveToScenario);
			result[person].ScheduledDayCollection(period).Should().Not.Be.Empty();
			var archivedAssignment = PersonAssignmentRepository.LoadAll().FirstOrDefault(x => x.Scenario.Equals(moveToScenario));
			archivedAssignment.Should().Not.Be.Null();
		}
	}
}