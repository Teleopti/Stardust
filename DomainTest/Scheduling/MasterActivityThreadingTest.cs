using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
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
		public ShiftProjectionCacheFetcher Target;

		[Test]
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
					var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity,
						new TimePeriodWithSegment(8, 0, 10, 0, 60), new TimePeriodWithSegment(16, 0, 18, 0, 60),
						new ShiftCategory().WithId())).WithId(ruleSetId);
					return Target.Execute(ruleSet);		
				}));
			}
		
			Task.WaitAll(tasks.ToArray());
		}
	}
}