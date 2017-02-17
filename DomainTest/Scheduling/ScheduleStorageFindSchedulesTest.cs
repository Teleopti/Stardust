using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class ScheduleStorageFindSchedulesTest
	{
		public IScheduleStorage Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		[Test]
		[Ignore("42639")]
		public void ShouldNotIncludeNotSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			var date = DateOnly.Today;
			var dateWithNotSelectedAgentAss = date.AddDays(2);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8,17));

			var schedules = Target.FindSchedulesForPersons(
					new ScheduleDateTimePeriod(date.ToDateTimePeriod(TimeZoneInfo.Utc), new[] {selectedAgent, notSelectedAgent}),
					scenario,
					new PersonProvider(new[] {selectedAgent, notSelectedAgent}),
					new ScheduleDictionaryLoadOptions(false, false),
					new[] {selectedAgent});

			schedules[notSelectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Be.Empty();
		}
	}
}