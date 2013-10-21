using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ConflictScheduleTagEagerLoadingTest : ScheduleRangePersisterIntegrationTest
	{
		private readonly DateOnly date = new DateOnly(2000,1,1);

		protected override void Given(ICollection<IPersistableScheduleData> scheduleDataInDatabaseAtStart)
		{
			scheduleDataInDatabaseAtStart.Add(new AgentDayScheduleTag(Person, date, Scenario, ScheduleTag));
		}

		protected override void WhenOtherHasChanged(IScheduleRange othersScheduleRange)
		{
			var day = othersScheduleRange.ScheduledDay(date);
			day.Clear<IAgentDayScheduleTag>();
			DoModify(day);
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var day = myScheduleRange.ScheduledDay(date);
			day.Clear<IAgentDayScheduleTag>();
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			var dbConflict = (IAgentDayScheduleTag)conflicts.Single().DatabaseVersion;
			LazyLoadingManager.IsInitialized(dbConflict.Person).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.UpdatedBy).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.ScheduleTag).Should().Be.True();
			LazyLoadingManager.IsInitialized(dbConflict.Scenario).Should().Be.True();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
		}
	}
}