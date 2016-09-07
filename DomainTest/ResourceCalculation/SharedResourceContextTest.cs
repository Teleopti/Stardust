using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SpeedUpManualChanges_37029)]
	public class SharedResourceContextTest
	{
		public ISharedResourceContext Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		[Test]
		public void ShouldMakeSureContextIsAlive()
		{
			using (Target.MakeSureExists(new DateOnlyPeriod())) { }

			ResourceCalculationContext.InContext
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCreateNewIfAlreadyExist()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod());
			var context = ResourceCalculationContext.Fetch();
			Target.MakeSureExists(new DateOnlyPeriod());

			ResourceCalculationContext.Fetch()
				.Should().Be.SameInstanceAs(context);
		}

		[TearDown]
		public void Teardown()
		{
			new ResourceCalculationContext(null);
		}
	}
}