using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationWithNoContextTest
	{
		public IResourceCalculation Target;
		
		[Test]
		public void ShouldConsiderBpoResourceWhenResourceCalculate()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var dateOnly = DateOnly.Today;
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 10);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, skillDay);

			Assert.Throws<NoCurrentResourceCalculationContextException>(() =>
			{
				Target.ResourceCalculate(dateOnly, resCalcData);
			});
		}
	}
}