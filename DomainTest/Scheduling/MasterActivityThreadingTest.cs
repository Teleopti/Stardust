using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class MasterActivityThreadingTest
	{
		public ILifetimeScope Container;

		[Test]
		[Ignore("failed test for #80374")]
		public void ShouldNotCrashWhenCacheShiftProjectWithMasterActivity()
		{
			var masterActivity = new MasterActivity();
			masterActivity.ActivityCollection.Add(new Activity().WithId());
			var ruleSetId = Guid.NewGuid();
			
			const int numberOfThreads = 100;
			var tasks = new List<Task>();
			for (var i = 0; i < numberOfThreads; i++)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					using (var scope = Container.BeginLifetimeScope())
					{
						var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity,
							new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60),
							new ShiftCategory().WithId())).WithId(ruleSetId);
						return scope.Resolve<ShiftProjectionCacheManager>().ShiftProjectionCachesFromRuleSets(
							new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc),
							new[] {ruleSet}, false);		
					}
				}));
			}
		
			Task.WaitAll(tasks.ToArray());

		}
	}
}