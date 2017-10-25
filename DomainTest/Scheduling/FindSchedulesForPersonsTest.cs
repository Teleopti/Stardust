using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	[TestFixture(true, false)]
	[TestFixture(false, false)]
	[TestFixture(true, true)]
	[TestFixture(false, true)]
	public class FindSchedulesForPersonsTest : IConfigureToggleManager
	{
		private readonly bool _loadByPerson;
		private readonly bool _resourcePlannerFasterLoading46307;
		public IFindSchedulesForPersons Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;

		public FindSchedulesForPersonsTest(bool loadByPerson, bool resourcePlannerFasterLoading46307)
		{
			_loadByPerson = loadByPerson;
			_resourcePlannerFasterLoading46307 = resourcePlannerFasterLoading46307;
		}

		[Test]
		public void ShouldNotIncludeNotSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			var date = new DateOnly(2000,1,1);
			var dateWithNotSelectedAgentAss = date.AddWeeks(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneMonth(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneDay(dateWithNotSelectedAgentAss).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateTimePeriod(TimeZoneInfo.Utc),
					new[] { selectedAgent }, true);

			schedules[notSelectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeSelectedAgentsScheduleOutsideChoosenPeriod()
		{
			var date = DateOnly.Today;
			var dateWithNotSelectedAgentAss = date.AddDays(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(selectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), dateWithNotSelectedAgentAss, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(scenario,
					new PersonProvider(new[] { selectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateTimePeriod(TimeZoneInfo.Utc),
					new[] { selectedAgent }, true);

			schedules[selectedAgent].ScheduledDay(dateWithNotSelectedAgentAss).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldIncludeNotSelectedAgentsScheduleInsideChoosenPeriod()
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneWeek(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(notSelectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), date, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateTimePeriod(TimeZoneInfo.Utc),
					new[] { selectedAgent }, true);

			schedules[notSelectedAgent].ScheduledDay(date).PersonAssignment(true).ShiftLayers
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldNotIncludeSelectedAgentsScheduleOutsideSelectedAgentPeriod()
		{
			var date = new DateOnly(2000,1,1);
			var dateWithAssignment = date.AddWeeks(3);
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var notSelectedAgent = new Person().WithSchedulePeriodOneMonth(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			var selectedAgentAss = new PersonAssignment(selectedAgent, scenario, dateWithAssignment);
			PersonAssignmentRepository.Has(selectedAgentAss);

			var schedules = Target.FindSchedulesForPersons(scenario,
					new PersonProvider(new[] { selectedAgent, notSelectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateTimePeriod(TimeZoneInfo.Utc),
					new[] { selectedAgent }, true);

			schedules[selectedAgent].Contains(selectedAgentAss, true).Should().Be.False();
		}

		[Test]
		public void ShouldIncludeSelectedAgentsScheduleOnceOnly()
		{
			var date = DateOnly.Today;
			var scenario = new Scenario("_");
			var selectedAgent = new Person().WithSchedulePeriodOneDay(date).InTimeZone(TimeZoneInfo.Utc).WithId();
			PersonAssignmentRepository.Has(selectedAgent, scenario, new Activity("_"), new ShiftCategory("_"), date, new TimePeriod(8, 17));

			var schedules = Target.FindSchedulesForPersons(scenario,
					new PersonProvider(new[] { selectedAgent }) { DoLoadByPerson = _loadByPerson },
					new ScheduleDictionaryLoadOptions(false, false),
					date.ToDateTimePeriod(TimeZoneInfo.Utc),
					new[] { selectedAgent }, true);

			schedules[selectedAgent].ScheduledDay(date).PersistableScheduleDataCollection().Count()
				.Should().Be.EqualTo(1);
		}

		public void Configure(FakeToggleManager toggleManager)
		{
			if(_resourcePlannerFasterLoading46307)
				toggleManager.Enable(Toggles.ResourcePlanner_FasterLoading_46307);
		}
	}
}