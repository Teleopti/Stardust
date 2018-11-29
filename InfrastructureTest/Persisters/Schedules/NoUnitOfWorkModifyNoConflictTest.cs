using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class NoUnitOfWorkModifyNoConflictTest : ScheduleRangeConflictTest
	{
		private readonly DateOnly date = new DateOnly(2001, 1, 1);

		protected override IEnumerable<IPersistableScheduleData> Given()
		{
			return new[] { new PersonAssignment(Person, Scenario, date) };
		}

		protected override IScheduleRangePersister CreateTarget()
		{
			return new ScheduleRangePersister(CurrentUnitOfWorkFactory.Make(),
					new DifferenceEntityCollectionService<IPersistableScheduleData>(),
					ConflictCollector(),
					MockRepository.GenerateMock<IScheduleDifferenceSaver>(),
					new EmptyInitiatorIdentifier(), new ClearScheduleEvents());
		}

		protected override void WhenImChanging(IScheduleRange myScheduleRange)
		{
			var start = new DateTime(2001, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var day = myScheduleRange.ScheduledDay(date);
			day.CreateAndAddActivity(Activity, new DateTimePeriod(start, start.AddHours(2)), ShiftCategory);
			DoModify(day);
		}

		protected override void Then(IEnumerable<PersistConflict> conflicts)
		{
			conflicts.Should().Be.Empty();
		}

		protected override void Then(IScheduleRange myScheduleRange)
		{
			Action fakeSavedToDb = myScheduleRange.TakeSnapshot;
			fakeSavedToDb();
		}

		protected override IScheduleRangeConflictCollector ConflictCollector()
		{
			var conflictCollector = MockRepository.GenerateMock<IScheduleRangeConflictCollector>();
			conflictCollector.Stub(x => x.GetConflicts(null, null)).IgnoreArguments().Return(Enumerable.Empty<PersistConflict>());
			return conflictCollector;
		}
	}
}