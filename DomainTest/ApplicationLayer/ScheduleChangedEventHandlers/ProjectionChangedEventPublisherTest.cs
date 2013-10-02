using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ProjectionChangedEventPublisherTest
	{
		[Test]
		public void ShouldPublishWithDayOffData()
		{
			var scenario = ScenarioFactory.CreateScenarioWithId(" ", true);
			var person = PersonFactory.CreatePersonWithPersonPeriodTeamSite(new DateOnly(2013, 10, 02));
			var publisher = new FakePublishEventsFromEventHandlers();
			var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
			scenarioRepository.Stub(x => x.Get(scenario.Id.Value)).Return(scenario);
			var personAssignment = PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2013, 10, 02), new DayOffTemplate(new Description("Day off", "DO")));
			var scheduleDictionary = ScheduleDictionaryForTest.WithPersonAssignment(scenario, new DateTime(2013, 10, 02), personAssignment);
			var scheduleRepository = MockRepository.GenerateMock<IScheduleRepository>();
			scheduleRepository.Stub(x => x.FindSchedulesOnlyInGivenPeriod(null, null, new DateOnlyPeriod(), scenario))
			                  .IgnoreArguments()
			                  .Return(scheduleDictionary);
			var target = new ProjectionChangedEventPublisher(publisher,scenarioRepository, new FakePersonRepository(person), scheduleRepository, new ProjectionChangedEventBuilder());

			target.Handle(new ScheduleChangedEvent
				{
					StartDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2013, 10, 02, 0, 0, 0, DateTimeKind.Utc).AddHours(24).AddMinutes(-1),
					PersonId = person.Id.Value,
					ScenarioId = scenario.Id.Value
				});

			var scheduleDay = publisher.Published<ProjectionChangedEvent>().ScheduleDays.Single(x => x.Date == new DateTime(2013, 10, 02));
			scheduleDay.IsDayOff.Should().Be.True();
			scheduleDay.ShortName.Should().Be("DO");
			scheduleDay.Name.Should().Be("Day off");
		}

	}
}