using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class IllegalResourceCalculationTest
	{
		public IResourceCalculation ResourceCalculation;

		[Test]
		public void ShouldThrowIfNotInContext()
		{
			Assert.Throws<NoCurrentResourceCalculationContextException>(() =>
				ResourceCalculation.ResourceCalculate(
					DateOnly.Today, 
					new ResourceCalculationData(ScheduleDictionaryCreator.WithData(new Scenario(), DateOnly.Today.ToDateOnlyPeriod()),
				Enumerable.Empty<ISkill>(), 
				new Dictionary<ISkill, IEnumerable<ISkillDay>>(), 
				false, 
				false)));
		}
	}
}