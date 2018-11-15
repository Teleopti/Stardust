using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	public class PlanningGroupTest
	{
		[Test]
		public void ShouldRemoveNonDefaultPlanningGroupSetting()
		{
			var planningGroup = new PlanningGroup();
			var defaultSetting = new PlanningGroupSettings();
			planningGroup.AddSetting(defaultSetting);
			planningGroup.RemoveSetting(defaultSetting);

			planningGroup.Settings.Should().Be.Empty();
		}
		
		[Test]
		public void CannotRemoveDefaultPlanningGroupSetting()
		{
			var planningGroup = new PlanningGroup();
			var defaultSetting = PlanningGroupSettings.CreateDefault();
			planningGroup.AddSetting(defaultSetting);
			Assert.Throws<ArgumentException>(() => planningGroup.RemoveSetting(defaultSetting));
		}
	}
}