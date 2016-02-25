using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class IntradayOptimizationContextHandleNestedResourcesTest
	{
		public IntradayOptimizationContext Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldCreateNewContext()
		{
			SchedulerStateHolder().SchedulingResultState.Schedules = new FakeScheduleDictionary();
			using (Target.Create(new DateOnlyPeriod()))
			{
				ResourceCalculationContext.Fetch().Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldReuseOldContextInNewContext()
		{
			SchedulerStateHolder().SchedulingResultState.Schedules = new FakeScheduleDictionary();
			using (new ResourceCalculationContext(new ResourceCalculationDataContainer(null, 1)))
			{
				var contextBefore = ResourceCalculationContext.Fetch();
				using (Target.Create(new DateOnlyPeriod()))
				{
					ResourceCalculationContext.Fetch().Should().Be.SameInstanceAs(contextBefore);
				}
			}
		}

		[Test]
		public void ShouldKeepOldContextAfterInnerDispose()
		{
			SchedulerStateHolder().SchedulingResultState.Schedules = new FakeScheduleDictionary();
			using (new ResourceCalculationContext(new ResourceCalculationDataContainer(null, 1)))
			{
				var contextBefore = ResourceCalculationContext.Fetch();
				using (Target.Create(new DateOnlyPeriod()))
				{
				}
				ResourceCalculationContext.Fetch().Should().Be.SameInstanceAs(contextBefore);
			}
		}
	}
}