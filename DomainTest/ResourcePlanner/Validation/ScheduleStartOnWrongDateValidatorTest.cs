using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourcePlanner.Validation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourcePlanner.Validation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_ShowSwitchedTimeZone_46303)]
	public class ScheduleStartOnWrongDateValidatorTest
	{
		public Func<ISchedulerStateHolder> StateHolder; //just a way to be able to create a Ischeduledictionary... MAybe some easier way? Claes?
		public SchedulingValidator Target;

		[Test]
		public void ShouldJumpThroughIfScheduleIsNullToSupportCheapPreChecks()
		{
			Assert.DoesNotThrow(() =>
			{			
				Target.Validate(null, new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod());
			});
		}

		/*
		[Test]
		public void Klagge_Here_You_Can_Continue()
		{
			var state = StateHolder.Fill(null, null, null);
			var schedules = state.Schedules;
			
			var result = Target.Validate(schedules, new[]{new Person(), }, DateOnly.Today.ToDateOnlyPeriod());
			
			result.Should()...
		}
		*/
	}
}