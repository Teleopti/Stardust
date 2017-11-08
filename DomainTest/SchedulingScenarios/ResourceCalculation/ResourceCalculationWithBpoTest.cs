using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ResourceCalculation
{
	[DomainTest]
	public class ResourceCalculationWithBpoTest
	{
		public IResourceCalculation Target;

		[Test]
		public void ShouldConsiderBpoResourceWhenResourceCalculate()
		{
			var scenario = new Scenario();
			var activity = new Activity().WithId();
			var dateOnly = DateOnly.Today;
			var skill = new Skill("_").For(activity).InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 9).WithId();
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, 0);
			var bpo = new BpoResource(5, new[]{skillDay.Skill}, skillDay.SkillStaffPeriodCollection.First().Period);
			var resCalcData = ResourceCalculationDataCreator.WithData(scenario, dateOnly, skillDay, bpo);
			
			Target.ResourceCalculate(dateOnly, resCalcData);

			skillDay.SkillStaffPeriodCollection.First().CalculatedResource
				.Should().Be.EqualTo(5); 
		}
	}
}