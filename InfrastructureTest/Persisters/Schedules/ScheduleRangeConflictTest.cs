using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public abstract class ScheduleRangeConflictTest : ScheduleRangePersisterBaseTest
	{
		protected abstract void WhenOtherHasChanged(IScheduleRange othersScheduleRange);
		protected abstract void WhenImChanging(IScheduleRange myScheduleRange);
		protected abstract void Then(IEnumerable<PersistConflict> conflicts);
		protected abstract void Then(IScheduleRange myScheduleRange);

		[Test]
		public void DoTheTest()
		{
			var otherDic = LoadScheduleDictionary();
			var otherRange = otherDic[Person];
			WhenOtherHasChanged(otherRange);

			var myDic = LoadScheduleDictionary();
			Target.Persist(otherRange);

			var myRange = myDic[Person];
			WhenImChanging(myRange);

			if (ExpectOptimistLockException)
			{
				Assert.Throws<OptimisticLockException>(() => Target.Persist(myRange));
			}
			else
			{
				var conflicts = Target.Persist(myRange);
				Then(conflicts);
				Then(myRange);

				var canLoadAfterChangeDicVerifier = LoadScheduleDictionary()[Person];
				if (!conflicts.Any())
				{
					//if no conflicts, db version should be same as users schedulerange
					Then(canLoadAfterChangeDicVerifier);
				}
				GeneralAsserts(myRange, conflicts);
			}
		}
	}
}