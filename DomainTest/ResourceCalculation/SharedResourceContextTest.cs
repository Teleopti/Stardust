using System;
using System.Linq;
using System.Threading.Tasks;
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
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			using (Target.MakeSureExists(new DateOnlyPeriod(), false)) { }

			ResourceCalculationContext.InContext
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotCreateNewIfAlreadyExist()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Target.MakeSureExists(new DateOnlyPeriod(), false);

			ResourceCalculationContext.Fetch()
				.Should().Be.SameInstanceAs(context);
		}

		[Test]
		public void ShouldShareBetweenThreads()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Task.Factory.StartNew(() =>
			{
				Target.MakeSureExists(new DateOnlyPeriod(), false);
				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(context);
			}).Wait();
		}

		[Test]
		public void ShouldForceNewContext()
		{
			SchedulerStateHolder.Fill(new Scenario("_"), DateOnly.Today.ToDateOnlyPeriod(), Enumerable.Empty<IPerson>(), Enumerable.Empty<IPersistableScheduleData>(), Enumerable.Empty<ISkillDay>());

			Target.MakeSureExists(new DateOnlyPeriod(), false);
			var context = ResourceCalculationContext.Fetch();
			Target.MakeSureExists(new DateOnlyPeriod(), true);

			ResourceCalculationContext.Fetch()
				.Should().Not.Be.SameInstanceAs(context);
		}

		[TearDown]
		public void Teardown()
		{
			new ResourceCalculationContext(null);
		}
	}
}