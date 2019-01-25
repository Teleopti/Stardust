using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class CascadingResourceCalculationContextTest : ResourceCalculationScenario
	{
		public IResourceCalculation Target;

		[Test]
		public void ShouldRestoreCallersContext()
		{
			var presentContext = new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), null, 0, false, new ActivityDivider());
			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => presentContext)))
			{
				Target.ResourceCalculate(DateOnly.Today, new ResourceCalculationData(new SchedulingResultStateHolder(new List<IPerson>(), new FakeScheduleDictionary(), new Dictionary<ISkill, IEnumerable<ISkillDay>>()), false, false));

				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(presentContext);
			}
		}
	}
}