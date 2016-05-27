using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationContextTest
	{
		public CascadingResourceCalculation Target;

		[Test]
		public void ShouldRestoreCallersContext()
		{
			var presentContext = new ResourceCalculationDataContainer(null, 0);
			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => presentContext)))
			{
				Target.ForDay(DateOnly.Today);

				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(presentContext);
			}
		}

		[Test]
		public void ShouldNotReturnContextIfThereWasNone()
		{
			Target.ForDay(DateOnly.Today);

			ResourceCalculationContext.InContext
				.Should().Be.False();
		}
	}
}