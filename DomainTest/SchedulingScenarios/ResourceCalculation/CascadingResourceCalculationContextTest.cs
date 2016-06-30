﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_CascadingSkills_38524)]
	public class CascadingResourceCalculationContextTest
	{
		public IResourceOptimizationHelper Target;

		[Test]
		public void ShouldRestoreCallersContext()
		{
			var presentContext = new ResourceCalculationDataContainer(null, 0);
			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => presentContext)))
			{
				Target.ResourceCalculate(DateOnly.Today, new ResourceCalculationData(new SchedulingResultStateHolder(new List<IPerson>(), new FakeScheduleDictionary(), new Dictionary<ISkill, IEnumerable<ISkillDay>>()), false, false));

				ResourceCalculationContext.Fetch()
					.Should().Be.SameInstanceAs(presentContext);
			}
		}

		[Test]
		public void ShouldNotReturnContextIfThereWasNone()
		{
			Target.ResourceCalculate(DateOnly.Today, new ResourceCalculationData(new SchedulingResultStateHolder(new List<IPerson>(), new FakeScheduleDictionary(), new Dictionary<ISkill, IEnumerable<ISkillDay>>()), false, false));

			ResourceCalculationContext.InContext
				.Should().Be.False();
		}
	}
}