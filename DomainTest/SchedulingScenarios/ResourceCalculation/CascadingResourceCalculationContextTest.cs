using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class CascadingResourceCalculationContextTest
	{
		public CascadingResourceCalculation Target;

		[Test]
		public void ShouldRestoreCallersContext()
		{
			var presentContext = new ResourceCalculationDataContainer(null, 0);
			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => presentContext)))
			{
				Target.ResourceCalculateDate(DateOnly.Today, false, false);

				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(presentContext);
			}
		}

		[Test]
		public void ShouldNotReturnContextIfThereWasNone()
		{
			Target.ResourceCalculateDate(DateOnly.Today, false, false);

			ResourceCalculationContext.InContext
				.Should().Be.False();
		}
	}
}